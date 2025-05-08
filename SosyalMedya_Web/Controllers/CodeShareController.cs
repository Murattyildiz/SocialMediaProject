using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SosyalMedya_Web.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;

namespace SosyalMedya_Web.Controllers
{
    [Authorize]
    public class CodeShareController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;

        public CodeShareController(IHttpClientFactory httpClientFactory, IConfiguration configuration) 
            : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiUrl = _configuration["ApiUrl"] ?? "https://localhost:5190"; // Default API URL if not found in configuration
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
            var response = await client.GetAsync($"{_apiUrl}/api/CodeShares/getall");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);
                
                if (apiResponse.Success)
                {
                    // Use JObject to properly handle the conversion
                    var jsonSettings = new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.Local,
                        DateParseHandling = DateParseHandling.DateTime
                    };
                    
                    var codeShares = JsonConvert.DeserializeObject<List<CodeShareViewModel>>(
                        apiResponse.Data.ToString(), jsonSettings);
                    
                    return View(codeShares);
                }
            }
            
            return View(new List<CodeShareViewModel>());
        }

        public async Task<IActionResult> Detail(int id)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
            var response = await client.GetAsync($"{_apiUrl}/api/CodeShares/getbyid?id={id}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);
                
                if (apiResponse.Success)
                {
                    var jsonSettings = new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.Local,
                        DateParseHandling = DateParseHandling.DateTime
                    };
                    
                    var codeShare = JsonConvert.DeserializeObject<CodeShareViewModel>(
                        apiResponse.Data.ToString(), jsonSettings);
                    
                    // Get AI analysis
                    var analysisResponse = await client.GetAsync($"{_apiUrl}/api/CodeShares/analyze/{id}");
                    if (analysisResponse.IsSuccessStatusCode)
                    {
                        var analysisContent = await analysisResponse.Content.ReadAsStringAsync();
                        var analysisApiResponse = JsonConvert.DeserializeObject<ApiResponse>(analysisContent);
                        
                        if (analysisApiResponse != null && analysisApiResponse.Success && analysisApiResponse.Data != null)
                        {
                            ViewData["CodeAnalysis"] = analysisApiResponse.Data.ToString();
                        }
                    }
                    
                    // Set current user ID for the view
                    ViewBag.CurrentUserId = GetCurrentUserId();
                    
                    return View(codeShare);
                }
            }
            
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View(new CreateCodeShareViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCodeShareViewModel model)
        {
            // Debug information
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"Title: {model.Title}");
            Console.WriteLine($"Description: {model.Description}");
            Console.WriteLine($"Language: {model.Language}");
            Console.WriteLine($"CodeContent length: {(model.CodeContent?.Length ?? 0)}");
            Console.WriteLine($"Tags: {model.Tags}");
            Console.WriteLine($"FileName: {model.FileName}");
            Console.WriteLine($"User ID: {GetCurrentUserId()}");
            
            // Set ViewBag data for debug
            ViewBag.DebugInfo = new Dictionary<string, string>
            {
                { "Title", model.Title },
                { "Description", model.Description },
                { "Language", model.Language },
                { "CodeContent", model.CodeContent?.Substring(0, Math.Min(20, model.CodeContent?.Length ?? 0)) + "..." },
                { "UserId", GetCurrentUserId().ToString() }
            };
            
            // Always check if the user is authenticated and has a valid token
            if (string.IsNullOrEmpty(GetToken()))
            {
                ModelState.AddModelError("", "Oturum süresi dolmuş. Lütfen tekrar giriş yapın.");
                return View(model);
            }
            
            // Only validate user ID as other validations are handled by data annotations
            if (GetCurrentUserId() <= 0)
            {
                ModelState.AddModelError("", "Kullanıcı kimliği alınamadı.");
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Create API request with explicit object structure
                    var apiRequest = new
                    {
                        UserId = GetCurrentUserId(),
                        Title = model.Title,
                        Description = model.Description,
                        CodeContent = model.CodeContent,
                        Language = model.Language,
                        Tags = model.Tags ?? string.Empty,
                        FileName = model.FileName ?? string.Empty,
                        SharingDate = DateTime.Now,
                        ViewCount = 0,
                        DownloadCount = 0
                    };

                    Console.WriteLine($"Request data: {JsonConvert.SerializeObject(apiRequest)}");

                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    var content = new StringContent(JsonConvert.SerializeObject(apiRequest), Encoding.UTF8, "application/json");
                    Console.WriteLine($"API URL: {_apiUrl}/api/CodeShares/add");
                    
                    var tokenPart = GetToken()?.Length > 10 ? GetToken().Substring(0, 10) + "..." : GetToken();
                    Console.WriteLine($"Token: {tokenPart}");
                    Console.WriteLine($"Content: {await content.ReadAsStringAsync()}");
                    
                    var response = await client.PostAsync($"{_apiUrl}/api/CodeShares/add", content);
                    
                    Console.WriteLine($"Response status: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Success response: {responseContent}");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error response: {responseContent}");
                        ModelState.AddModelError("", $"API Hatası: {responseContent}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    ModelState.AddModelError("", $"İşlem sırasında bir hata oluştu: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("ModelState errors:");
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"{entry.Key}: {error.ErrorMessage}");
                    }
                }
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int codeShareId, string commentText)
        {
            var requestData = new
            {
                codeComment = new
                {
                    CodeShareId = codeShareId,
                    UserId = GetCurrentUserId(),
                    CommentText = commentText
                }
            };

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
            
            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_apiUrl}/api/CodeComments/add", content);
            
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Detail", new { id = codeShareId });
            }
            
            TempData["Error"] = "Yorum eklenirken bir hata oluştu.";
            return RedirectToAction("Detail", new { id = codeShareId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId, int codeShareId)
        {
            var requestData = new
            {
                codeComment = new
                {
                    Id = commentId
                }
            };

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
            
            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_apiUrl}/api/CodeComments/delete", content);
            
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Detail", new { id = codeShareId });
            }
            
            TempData["Error"] = "Yorum silinirken bir hata oluştu.";
            return RedirectToAction("Detail", new { id = codeShareId });
        }
        
        public async Task<IActionResult> DownloadCodeAsZip(int id)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
            
            // Download the ZIP file from the API
            var response = await client.GetAsync($"{_apiUrl}/api/CodeShares/download/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var zipContent = await response.Content.ReadAsByteArrayAsync();
                var contentDisposition = response.Content.Headers.ContentDisposition;
                var fileName = contentDisposition?.FileName ?? $"code-{id}.zip";
                
                return File(zipContent, "application/zip", fileName);
            }
            
            TempData["Error"] = "Kod ZIP olarak indirilirken bir hata oluştu.";
            return RedirectToAction("Detail", new { id = id });
        }
    }
} 