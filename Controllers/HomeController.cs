using MiAplicacion.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Html2pdf;
using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

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


        public IActionResult CargarDocumento()
        {
            try
            {
                var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "PDFs/Ultrasonido Abdominal.docx");
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(uploads, false))
                {
                    var docPart = wordDocument.MainDocumentPart;
                    if (docPart != null)
                    {
                        var doc = docPart.Document;
                        if (doc != null)
                        {
                            var body = doc.Body;
                            if (body != null)
                            {
                                var text = body.InnerXml;
                                return Content(text, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                            }
                        }
                    }
                    return Content("No se encontró contenido en el documento.", "text/plain");
                }
            }
            catch (Exception ex)
            {
                return Content("Ocurrió un error al cargar el documento: " + ex.Message);
            }
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
                        var writer = new PdfWriter(stream).SetSmartMode(true);
                        var pdf = new PdfDocument(writer);
                        var document = new iText.Layout.Document(pdf);

                       
                        
                        // Leer el contenido del archivo IFormFile y escribirlo en el documento PDF
                        using (var fileStream = file.OpenReadStream())
                        {
                            
                            // Ajusta esta parte según las necesidades de tu aplicación
                            // Por ejemplo, aquí se copia el contenido del archivo en el documento PDF
                            await fileStream.CopyToAsync(stream);
                        }
                        

                        document.Close();

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

        //[HttpPost]
        //public async Task<IActionResult> RecibirPDF(IFormFile file)
        //{
        //    try
        //    {
        //        if (file != null && file.Length > 0)
        //        {
        //            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "PDFs"); // Ruta donde se guardarán los archivos PDF

        //            if (!Directory.Exists(uploads))
        //            {
        //                Directory.CreateDirectory(uploads);
        //            }

        //            var filePath = Path.Combine(uploads, file.FileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(stream);
        //            }

        //            return Content("Archivo PDF guardado correctamente en el servidor.");
        //        }
        //        else
        //        {
        //            return Content("No se recibió ningún archivo PDF.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejar cualquier excepción que pueda ocurrir durante el proceso de guardado
        //        return Content("Ocurrió un error al guardar el archivo PDF en el servidor: " + ex.Message);
        //    }
        //}


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
                        var document = new iText.Layout.Document(pdf);
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