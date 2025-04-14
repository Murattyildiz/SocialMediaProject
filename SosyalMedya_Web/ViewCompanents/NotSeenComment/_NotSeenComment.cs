using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;

namespace SosyalMedya_Web.ViewCompanents.NotSeenComment
{
    public class _NotSeenComment : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public _NotSeenComment(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {

            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //getarticlewithdetailsbyuserid?id=

            int userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;

            var responseMessage = await _httpClientFactory.CreateClient().GetAsync("https://localhost:5190/api/Articles/getarticlewithdetailsbyuserid?id=" + userId);
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();

                var apiDataResponse = JsonConvert.DeserializeObject<ApiListDataResponse<ArticleDetail>>(jsonResponse);

                int notSeenComment = apiDataResponse.Data
                    .SelectMany(article => article.CommentDetails)
                    .Count(comment => comment.Status == false && comment.UserId != userId && comment.Id != -1);


                return View(notSeenComment);
            }
            return View();
        }
    }
}