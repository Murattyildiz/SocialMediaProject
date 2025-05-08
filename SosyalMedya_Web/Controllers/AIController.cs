using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SosyalMedya_Web.Controllers
{
    [Authorize]
    public class AIController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;

        public AIController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiUrl = _configuration["ApiUrl"] ?? "https://localhost:5190"; // Default API URL if not found in configuration
        }

        [HttpGet]
        public async Task<IActionResult> AnalyzeUserInterests(int userId)
        {
            // Get articles by user ID
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
            var response = await client.GetAsync($"{_apiUrl}/api/Articles/getbyuserid?userId={userId}");

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
                    
                    var articles = JsonConvert.DeserializeObject<List<ArticleDetail>>(
                        apiResponse.Data.ToString(), jsonSettings);
                    
                    // Get code shares by user ID
                    var codeShareResponse = await client.GetAsync($"{_apiUrl}/api/CodeShares/getbyuserid?userId={userId}");
                    List<CodeShareViewModel> codeShares = new List<CodeShareViewModel>();
                    
                    if (codeShareResponse.IsSuccessStatusCode)
                    {
                        var codeShareContent = await codeShareResponse.Content.ReadAsStringAsync();
                        var codeShareApiResponse = JsonConvert.DeserializeObject<ApiResponse>(codeShareContent);
                        
                        if (codeShareApiResponse.Success)
                        {
                            codeShares = JsonConvert.DeserializeObject<List<CodeShareViewModel>>(
                                codeShareApiResponse.Data.ToString(), jsonSettings);
                        }
                    }
                    
                    // Perform simple analysis
                    var analysis = new
                    {
                        TotalPosts = articles.Count,
                        TotalCodeShares = codeShares.Count,
                        TopTopics = GetTopTopics(articles),
                        TopCodeTags = GetTopCodeTags(codeShares),
                        TopLanguages = GetTopLanguages(codeShares),
                        ActivitySummary = GetActivitySummary(articles, codeShares)
                    };
                    
                    return Json(analysis);
                }
            }
            
            return Json(new { Error = "Failed to analyze user interests" });
        }

        private List<KeyValuePair<string, int>> GetTopTopics(List<ArticleDetail> articles)
        {
            return articles
                .GroupBy(a => a.TopicTitle)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();
        }

        private List<KeyValuePair<string, int>> GetTopCodeTags(List<CodeShareViewModel> codeShares)
        {
            var tagCounts = new Dictionary<string, int>();
            
            foreach (var codeShare in codeShares)
            {
                if (!string.IsNullOrEmpty(codeShare.Tags))
                {
                    var tags = codeShare.Tags.Split(',').Select(t => t.Trim());
                    foreach (var tag in tags)
                    {
                        if (tagCounts.ContainsKey(tag))
                            tagCounts[tag]++;
                        else
                            tagCounts[tag] = 1;
                    }
                }
            }
            
            return tagCounts
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();
        }

        private List<KeyValuePair<string, int>> GetTopLanguages(List<CodeShareViewModel> codeShares)
        {
            return codeShares
                .Where(c => !string.IsNullOrEmpty(c.Language))
                .GroupBy(c => c.Language)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();
        }

        private string GetActivitySummary(List<ArticleDetail> articles, List<CodeShareViewModel> codeShares)
        {
            var allItems = new List<Tuple<DateTime, string>>();
            
            foreach (var article in articles)
            {
                try
                {
                    DateTime articleDate;
                    
                    // Try to interpret the SharingDate as a DateTime
                    if (article.SharingDate != null)
                    {
                        if (DateTime.TryParse(article.SharingDate.ToString(), out articleDate))
                        {
                            allItems.Add(new Tuple<DateTime, string>(articleDate, "post"));
                        }
                        else
                        {
                            // Use current date if parsing fails
                            allItems.Add(new Tuple<DateTime, string>(DateTime.Now, "post"));
                        }
                    }
                    else
                    {
                        // Use current date if SharingDate is null
                        allItems.Add(new Tuple<DateTime, string>(DateTime.Now, "post"));
                    }
                }
                catch
                {
                    // Use current date if any exception occurs
                    allItems.Add(new Tuple<DateTime, string>(DateTime.Now, "post"));
                }
            }
            
            foreach (var codeShare in codeShares)
            {
                try
                {
                    // Try to use SharingDate directly
                    allItems.Add(new Tuple<DateTime, string>(codeShare.SharingDate, "code"));
                }
                catch
                {
                    try
                    {
                        DateTime codeShareDate;
                        // Try to parse SharingDate as a string
                        if (codeShare.SharingDate != null && 
                            DateTime.TryParse(codeShare.SharingDate.ToString(), out codeShareDate))
                        {
                            allItems.Add(new Tuple<DateTime, string>(codeShareDate, "code"));
                        }
                        else
                        {
                            // Use current date if parsing fails
                            allItems.Add(new Tuple<DateTime, string>(DateTime.Now, "code"));
                        }
                    }
                    catch
                    {
                        // Use current date if any exception occurs
                        allItems.Add(new Tuple<DateTime, string>(DateTime.Now, "code"));
                    }
                }
            }

            // Sort by date
            allItems = allItems.OrderByDescending(x => x.Item1).ToList();
            
            // Calculate overall activity level
            string activityLevel;
            int itemsPerMonth = 0;
            
            if (allItems.Any())
            {
                var monthsSpan = (DateTime.Now - allItems.Min(x => x.Item1)).TotalDays / 30;
                if (monthsSpan > 0)
                    itemsPerMonth = (int)(allItems.Count / monthsSpan);
            }
            
            if (itemsPerMonth >= 10)
                activityLevel = "Çok aktif";
            else if (itemsPerMonth >= 5)
                activityLevel = "Aktif";
            else if (itemsPerMonth >= 1)
                activityLevel = "Orta düzeyde aktif";
            else
                activityLevel = "Az aktif";
            
            // Calculate code to post ratio
            var codeRatio = allItems.Count > 0 ? (double)codeShares.Count / allItems.Count : 0;
            string focusArea;
            
            if (codeRatio >= 0.7)
                focusArea = "Ağırlıklı kod paylaşımına odaklı";
            else if (codeRatio >= 0.3)
                focusArea = "Hem içerik hem kod paylaşımına odaklı";
            else
                focusArea = "Ağırlıklı içerik paylaşımına odaklı";
            
            return $"{activityLevel}, {focusArea}, son {Math.Min(allItems.Count, 30)} günde toplam {allItems.Count} paylaşım.";
        }
    }
} 