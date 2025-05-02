using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using System.Net.Http.Headers;

namespace SosyalMedya_Web.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MessageController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("mesajlar")]
        public async Task<IActionResult> Index()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                TempData["Message"] = "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın.";
                TempData["Success"] = false;
                return RedirectToAction("Login", "Auth");
            }

            // Kullanıcının son mesajları olan diğer kullanıcıları getir
            var httpClient = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Son mesajları olan kullanıcıların listesini getir
            var userListResponse = await httpClient.GetAsync($"https://localhost:5190/api/Users/getall");
            
            if (!userListResponse.IsSuccessStatusCode)
            {
                return View(new List<MessageUserListViewModel>());
            }

            var userListJson = await userListResponse.Content.ReadAsStringAsync();
            var userListApiResponse = JsonConvert.DeserializeObject<ApiListDataResponse<UserDto>>(userListJson);

            if (!userListApiResponse.Success || userListApiResponse.Data == null)
            {
                return View(new List<MessageUserListViewModel>());
            }

            var userList = userListApiResponse.Data
                .Where(u => u.Id != currentUserId)
                .Select(u => new MessageUserListViewModel
                {
                    UserId = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    ImagePath = string.IsNullOrEmpty(u.ImagePath) ? "/images/default.jpg" : u.ImagePath,
                    LastMessageContent = "",
                    LastMessageDate = DateTime.MinValue,
                    UnreadMessageCount = 0
                })
                .ToList();

            // Her kullanıcı için son mesaj bilgilerini getir
            foreach (var user in userList)
            {
                var messagesResponse = await httpClient.GetAsync($"https://localhost:5190/api/Messages/getconversation?user1Id={currentUserId}&user2Id={user.UserId}");
                
                if (messagesResponse.IsSuccessStatusCode)
                {
                    var messagesJson = await messagesResponse.Content.ReadAsStringAsync();
                    var messagesApiResponse = JsonConvert.DeserializeObject<ApiDataResponse<List<Message>>>(messagesJson);
                    
                    if (messagesApiResponse.Success && messagesApiResponse.Data != null && messagesApiResponse.Data.Any())
                    {
                        var lastMessage = messagesApiResponse.Data.OrderByDescending(m => m.SentDate).First();
                        user.LastMessageContent = lastMessage.Content.Length > 30 
                            ? lastMessage.Content.Substring(0, 27) + "..." 
                            : lastMessage.Content;
                        user.LastMessageDate = lastMessage.SentDate;
                        user.UnreadMessageCount = messagesApiResponse.Data.Count(m => !m.IsRead && m.ReceiverId == currentUserId);
                    }
                }
            }

            // Son mesajı olanları üstte göster ve tarih sırasına göre sırala
            userList = userList
                .OrderByDescending(u => u.LastMessageDate != DateTime.MinValue)
                .ThenByDescending(u => u.LastMessageDate)
                .ToList();

            return View(userList);
        }

        [HttpGet("mesajlar/{userId}")]
        public async Task<IActionResult> Conversation(int userId)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                TempData["Message"] = "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın.";
                TempData["Success"] = false;
                return RedirectToAction("Login", "Auth");
            }

            // Diğer kullanıcının bilgilerini getir
            var httpClient = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var otherUserResponse = await httpClient.GetAsync($"https://localhost:5190/api/Users/getbyid?id={userId}");
            if (!otherUserResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var otherUserJson = await otherUserResponse.Content.ReadAsStringAsync();
            var otherUserApiResponse = JsonConvert.DeserializeObject<ApiDataResponse<UserDto>>(otherUserJson);

            if (!otherUserApiResponse.Success || otherUserApiResponse.Data == null)
            {
                return NotFound();
            }

            var otherUser = otherUserApiResponse.Data;
            ViewData["OtherUserId"] = otherUser.Id;
            ViewData["OtherUserName"] = $"{otherUser.FirstName} {otherUser.LastName}";
            ViewData["OtherUserImage"] = string.IsNullOrEmpty(otherUser.ImagePath) 
                ? "https://localhost:5190/images/default.jpg" 
                : $"https://localhost:5190/{otherUser.ImagePath}";
            
            ViewData["CurrentUserId"] = currentUserId;
            ViewData["Token"] = token;

            // Mesajları getir
            var messagesResponse = await httpClient.GetAsync($"https://localhost:5190/api/Messages/getconversation?user1Id={currentUserId}&user2Id={userId}");
            
            List<MessageDto> messages = new List<MessageDto>();
            
            if (messagesResponse.IsSuccessStatusCode)
            {
                var messagesJson = await messagesResponse.Content.ReadAsStringAsync();
                var messagesApiResponse = JsonConvert.DeserializeObject<ApiDataResponse<List<Message>>>(messagesJson);
                
                if (messagesApiResponse.Success && messagesApiResponse.Data != null)
                {
                    // CurrentUser bilgilerini getir
                    var currentUserResponse = await httpClient.GetAsync($"https://localhost:5190/api/Users/getbyid?id={currentUserId}");
                    string currentUserName = "";
                    string currentUserImage = "https://localhost:5190/images/default.jpg";
                    
                    if (currentUserResponse.IsSuccessStatusCode)
                    {
                        var currentUserJson = await currentUserResponse.Content.ReadAsStringAsync();
                        var currentUserApiResponse = JsonConvert.DeserializeObject<ApiDataResponse<UserDto>>(currentUserJson);
                        
                        if (currentUserApiResponse.Success && currentUserApiResponse.Data != null)
                        {
                            currentUserName = $"{currentUserApiResponse.Data.FirstName} {currentUserApiResponse.Data.LastName}";
                            currentUserImage = string.IsNullOrEmpty(currentUserApiResponse.Data.ImagePath) 
                                ? "https://localhost:5190/images/default.jpg" 
                                : $"https://localhost:5190/{currentUserApiResponse.Data.ImagePath}";
                        }
                    }

                    // Okunmamış mesajları okundu olarak işaretle
                    foreach (var message in messagesApiResponse.Data.Where(m => !m.IsRead && m.ReceiverId == currentUserId))
                    {
                        await httpClient.PostAsync($"https://localhost:5190/api/Messages/markasread?messageId={message.Id}", null);
                    }

                    messages = messagesApiResponse.Data.Select(m => new MessageDto
                    {
                        Id = m.Id,
                        Content = m.Content,
                        SentDate = m.SentDate,
                        IsRead = m.IsRead,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        SenderName = m.SenderId == currentUserId ? currentUserName : ViewData["OtherUserName"].ToString(),
                        SenderImage = m.SenderId == currentUserId ? currentUserImage : ViewData["OtherUserImage"].ToString(),
                        ReceiverName = m.ReceiverId == currentUserId ? currentUserName : ViewData["OtherUserName"].ToString(),
                        ReceiverImage = m.ReceiverId == currentUserId ? currentUserImage : ViewData["OtherUserImage"].ToString()
                    }).OrderBy(m => m.SentDate).ToList();
                }
            }

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageViewModel messageVM)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın." });
            }

            var message = new Message
            {
                SenderId = currentUserId.Value,
                ReceiverId = messageVM.ReceiverId,
                Content = messageVM.Content
            };
            
            var httpClient = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonConvert.SerializeObject(message), System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://localhost:5190/api/Messages/add", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                
                return Json(new { success = true, message = "Mesaj gönderildi" });
            }
            
            return Json(new { success = false, message = "Mesaj gönderilemedi" });
        }
        
        [HttpGet]
        public async Task<IActionResult> GetNewMessages(int otherUserId, string lastMessageDate)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş olabilir." });
            }
            
            DateTime parsedLastMessageDate;
            if (!DateTime.TryParse(lastMessageDate, out parsedLastMessageDate))
            {
                parsedLastMessageDate = DateTime.MinValue;
            }
            
            var httpClient = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("Token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var messagesResponse = await httpClient.GetAsync($"https://localhost:5190/api/Messages/getconversation?user1Id={currentUserId}&user2Id={otherUserId}");
            
            if (messagesResponse.IsSuccessStatusCode)
            {
                var messagesJson = await messagesResponse.Content.ReadAsStringAsync();
                var messagesApiResponse = JsonConvert.DeserializeObject<ApiDataResponse<List<Message>>>(messagesJson);
                
                if (messagesApiResponse.Success && messagesApiResponse.Data != null && messagesApiResponse.Data.Any())
                {
                    var newMessages = messagesApiResponse.Data.Where(m => m.SentDate > parsedLastMessageDate).ToList();
                    return Json(new { success = true, hasNewMessages = newMessages.Any() });
                }
            }
            
            return Json(new { success = true, hasNewMessages = false });
        }
    }

    public class SendMessageViewModel
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; }
    }

    public class MessageUserListViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string ImagePath { get; set; }
        public string LastMessageContent { get; set; }
        public DateTime LastMessageDate { get; set; }
        public int UnreadMessageCount { get; set; }
    }
} 