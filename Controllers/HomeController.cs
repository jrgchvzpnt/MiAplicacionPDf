using MiAplicacion.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.Text.Json;


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
        public async Task<IActionResult> GuardarPDF()
        {
            try
            {
                string requestBody;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "PDFs", "archivo.pdf");
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    await fs.WriteAsync(Convert.FromBase64String(requestBody));
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Se produjo un error al guardar el PDF: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerarPDF()
         {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();
                Console.WriteLine(requestBody); // Agrega esta línea para imprimir el contenido del objeto JSON en la consola
                
                var jsonDocument = JsonDocument.Parse(requestBody);
                var content = jsonDocument.RootElement.GetProperty("content").GetString();

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "PDFs");
                var filePath = Path.Combine(directoryPath, "archivo.pdf");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var writer = new PdfWriter(filePath))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);
                        document.Add(new Paragraph(content));
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