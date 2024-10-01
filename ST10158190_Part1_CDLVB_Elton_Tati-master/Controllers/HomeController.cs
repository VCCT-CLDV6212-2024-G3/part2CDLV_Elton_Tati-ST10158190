using Microsoft.AspNetCore.Mvc;
using ST10158190Part1_CLDV_B.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using ST10158190Part1_CLDV_B.Services;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
//ST10158190



namespace ST10158190Part1_CLDV_B.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HomeController> _logger;
        private readonly TableService _tableService;
        private readonly FileService _fileService;
        private readonly QueueService _queueService;

        public HomeController(HttpClient httpClient, ILogger<HomeController> logger, TableService tableService, FileService fileService, QueueService queueService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tableService = tableService;
            _fileService = fileService;
            _queueService = queueService;
        }
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var containerName = "product-image";
                var blobName = file.FileName;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    var content = new MultipartFormDataContent();
                    stream.Position = 0;
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    content.Add(streamContent, "file", file.FileName);

                  
                    var functionUrl ="https://st10158190-function.azurewebsites.net/api/UploadBlob?code=vpmuYfgCeISL5QC0EPdEJOgZ43lt--QJDIvfLCPVDeSPAzFuMhi4gQ%3D%3D";
                    var response = await _httpClient.PostAsync(functionUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        ViewBag.Message = $"File upload failed: {response.ReasonPhrase}";
                    }
                    else
                    {
                        ViewBag.Message = "File uploaded successfully!";
                    }
                }
            }
            else
            {
                ViewBag.Message = "No file selected.";
            }

            return View();
        }
        [HttpGet]
        public IActionResult AddCustomerProfile()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddCustomerProfile(string firstName, string lastName, string email, string phoneNumber)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phoneNumber))
            {
                ViewBag.Message = "All fields are required.";
                return View();
            }

            var customerProfile = new
            {
                tableName = "CustomerProfiles", 
                partitionKey = "Customer",
                rowKey = Guid.NewGuid().ToString(),
                firstName,
                lastName,
                email,
                phoneNumber
            };

            
            var functionUrl = "https://st10158190-function.azurewebsites.net/api/StoreTableInfo?code=LIzgRDD2ONaxjBSYfA7dw9tkSY4p6zgV3x59d8PvU122AzFuJWyKGw%3D%3D";

            using var httpClient = new HttpClient();
            var jsonContent = JsonConvert.SerializeObject(customerProfile);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            
            var response = await httpClient.PostAsync(functionUrl, content);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = await response.Content.ReadAsStringAsync();
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("CustomerProfile", $"Error adding customer profile: {errorMessage}");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadContract(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var functionUrl = "https://st10158190-function.azurewebsites.net/api/UploadFile?code=GhcFvXce2y6wLKkW4UVPqQ9-mGixahh1b0VeD3b6hOtCAzFu3EoTZQ%3D%3D";

                using var httpClient = new HttpClient();
                using var stream = file.OpenReadStream();
                using var content = new MultipartFormDataContent();

               
                var fileContent = new StreamContent(stream)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) }
                };
                content.Add(fileContent, "file", file.FileName);

                
                var response = await httpClient.PostAsync(functionUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "File upload failed: " + await response.Content.ReadAsStringAsync();
                }
                else
                {
                    ViewBag.Message = "Contract uploaded successfully.";
                }
            }
            else
            {
                ViewBag.Message = "No file selected.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult UploadLod()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> UploadLog(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    await _fileService.UploadFileAsync("logs", file.FileName, stream);
                }
                ViewBag.Message = "Log uploaded successfully.";
            }
            else
            {
                ViewBag.Message = "No file selected.";
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ProcessOrder(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                var functionUrl = "https://st10158190-function.azurewebsites.net/api/ProcessQueueMessage?code=lS8llMjR20GdA6MCGc281jRtZ3z8HeTy4y8y3eVUY5aFAzFuzg8a1A%3D%3D";

                using var httpClient = new HttpClient();
                var content = new StringContent(orderId, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(functionUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    ViewBag.Message = "Order processing failed: " + await response.Content.ReadAsStringAsync();
                }
            }
            else
            {
                ViewBag.Message = "Please enter a valid Order ID.";
            }

            return View();
        }


        public IActionResult Index()
        {
            return View();
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
