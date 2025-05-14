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
            try
            {
                // Get articles by user ID
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                var response = await client.GetAsync($"{_apiUrl}/api/Articles/getarticlewithdetailsbyuserid?id={userId}");

                List<ArticleDetail> articles = new List<ArticleDetail>();
                List<CodeShareViewModel> codeShares = new List<CodeShareViewModel>();

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiListDataResponse<ArticleDetail>>(content);

                    if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
                    {
                        articles = apiResponse.Data;
                    }
                    
                    // Get code shares by user ID
                    var codeShareResponse = await client.GetAsync($"{_apiUrl}/api/CodeShares/getbyuserid?userId={userId}");
                    
                    if (codeShareResponse.IsSuccessStatusCode)
                    {
                        var codeShareContent = await codeShareResponse.Content.ReadAsStringAsync();
                        var codeShareApiResponse = JsonConvert.DeserializeObject<ApiListDataResponse<CodeShareViewModel>>(codeShareContent);
                        
                        if (codeShareApiResponse != null && codeShareApiResponse.Success && codeShareApiResponse.Data != null)
                        {
                            codeShares = codeShareApiResponse.Data;
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
                
                return Json(new { Error = "Failed to analyze user interests", Message = "API veri alınamadı" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Analysis failed", Message = ex.Message });
            }
        }

        private List<KeyValuePair<string, int>> GetTopTopics(List<ArticleDetail> articles)
        {
            if (articles == null || !articles.Any())
                return new List<KeyValuePair<string, int>>();
                
            return articles
                .Where(a => !string.IsNullOrEmpty(a.TopicTitle))
                .GroupBy(a => a.TopicTitle)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();
        }

        private List<KeyValuePair<string, int>> GetTopCodeTags(List<CodeShareViewModel> codeShares)
        {
            if (codeShares == null || !codeShares.Any())
                return new List<KeyValuePair<string, int>>();
                
            var tagCounts = new Dictionary<string, int>();
            
            foreach (var codeShare in codeShares)
            {
                if (!string.IsNullOrEmpty(codeShare.Tags))
                {
                    var tags = codeShare.Tags.Split(',').Select(t => t.Trim());
                    foreach (var tag in tags)
                    {
                        if (!string.IsNullOrEmpty(tag))
                        {
                            if (tagCounts.ContainsKey(tag))
                                tagCounts[tag]++;
                            else
                                tagCounts[tag] = 1;
                        }
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
            if (codeShares == null || !codeShares.Any())
                return new List<KeyValuePair<string, int>>();
                
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
            if ((articles == null || !articles.Any()) && (codeShares == null || !codeShares.Any()))
                return "Henüz yeterli veri bulunmuyor.";
                
            var allItems = new List<Tuple<DateTime, string>>();
            
            if (articles != null)
            {
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
                                allItems.Add(new Tuple<DateTime, string>(DateTime.Now.AddDays(-7), "post"));
                            }
                        }
                        else
                        {
                            // Use current date if SharingDate is null
                            allItems.Add(new Tuple<DateTime, string>(DateTime.Now.AddDays(-7), "post"));
                        }
                    }
                    catch
                    {
                        // Use current date if any exception occurs
                        allItems.Add(new Tuple<DateTime, string>(DateTime.Now.AddDays(-7), "post"));
                    }
                }
            }
            
            if (codeShares != null)
            {
                foreach (var codeShare in codeShares)
                {
                    try
                    {
                        allItems.Add(new Tuple<DateTime, string>(codeShare.SharingDate, "code"));
                    }
                    catch
                    {
                        // Use current date if any exception occurs
                        allItems.Add(new Tuple<DateTime, string>(DateTime.Now.AddDays(-7), "code"));
                    }
                }
            }

            if (!allItems.Any())
                return "Henüz yeterli paylaşım bulunmuyor.";

            // Sort by date
            allItems = allItems.OrderByDescending(x => x.Item1).ToList();
            
            // Calculate overall activity level
            string activityLevel;
            int itemsPerMonth = 0;
            
            if (allItems.Any())
            {
                var monthsSpan = Math.Max(1, (DateTime.Now - allItems.Min(x => x.Item1)).TotalDays / 30);
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
            int codeCount = allItems.Count(x => x.Item2 == "code");
            var codeRatio = allItems.Count > 0 ? (double)codeCount / allItems.Count : 0;
            string focusArea;
            
            if (codeRatio >= 0.7)
                focusArea = "Ağırlıklı kod paylaşımına odaklı";
            else if (codeRatio >= 0.3)
                focusArea = "Hem içerik hem kod paylaşımına odaklı";
            else
                focusArea = "Ağırlıklı içerik paylaşımına odaklı";
            
            return $"{activityLevel}, {focusArea}, toplam {allItems.Count} paylaşım.";
        }
    }
} 