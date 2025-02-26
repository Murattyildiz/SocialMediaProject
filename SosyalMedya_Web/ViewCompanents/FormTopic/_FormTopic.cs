﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;

namespace SosyalMedya_Web.ViewComponents.FormTopic
{
    public class _FormTopic : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public _FormTopic(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var responseMessage = await _httpClientFactory.CreateClient().GetAsync("https://localhost:5190/api/Topics/getall");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonResponse = await responseMessage.Content.ReadAsStringAsync();

                var apiDataResponse = JsonConvert.DeserializeObject<ApiListDataResponse<Topics>>(jsonResponse);
                var trueTopic = apiDataResponse.Data.Where(x => x.Status == true).ToList();
                return apiDataResponse.Success ? View(trueTopic) : View();
            }
            return View();
        }
    }
}