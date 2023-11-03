using MiAplicacion.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Html2pdf;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Org.BouncyCastle.Asn1.Ocsp;


namespace MiAplicacion.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecibirPDF(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "PDFs"); // Ruta donde se guardarán los archivos PDF

                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }

                    var filePath = Path.Combine(uploads, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Content("Archivo PDF guardado correctamente en el servidor.");
                }
                else
                {
                    return Content("No se recibió ningún archivo PDF.");
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir durante el proceso de guardado
                return Content("Ocurrió un error al guardar el archivo PDF en el servidor: " + ex.Message);
            }
        }


        //codigo funciona que el formato
        [HttpPost]
        public async Task<IActionResult> GenerarPDF()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var jsonDocument = JsonDocument.Parse(requestBody);
                var content = jsonDocument.RootElement.GetProperty("content").GetString();

                var cleanedContent = content.Replace("\\n", "\n").Trim(); // Limpiar saltos de línea y caracteres de escape
                var finalContent = cleanedContent.Replace("\n", string.Empty).Replace("\r", string.Empty); // Eliminar saltos de línea

                var filePath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "PDFs", "archivo.pdf");
                using (var writer = new PdfWriter(filePath))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);
                        var htmlContent = finalContent; // Tu contenido HTML aquí

                        var page = pdf.AddNewPage(); // Agregar una nueva página al documento
                        var elements = HtmlConverter.ConvertToElements(htmlContent);
                        foreach (var element in elements)
                        {
                            document.Add((IBlockElement)element);
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Se produjo un error al generar el PDF: {ex.Message}");
            }
        }





        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}