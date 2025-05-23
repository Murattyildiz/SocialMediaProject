﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using SosyalMedya_Web.Models;
using SosyalMedya_Web.Utilities.Helpers;
using System.Security.Claims;
using System.Text;

namespace SosyalMedya_Web.Controllers
{
    public class AuthController : Controller
    {
        private const string AdminRole = "admin";
        private const string UserRole = "user";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiUrl = _configuration["ApiUrl"] ?? "https://localhost:5190"; // Default API URL
        }
        [HttpGet("giris-yap")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet("kayit-ol")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("RegisterPost")]
        public async Task<IActionResult> RegisterPost(UserForRegister userForRegister)
        {
            if (userForRegister.Password != userForRegister.ConfirmPassword)
            {
                TempData["RegisterFail"] = "Şifreler eşleşmiyor!";
                return RedirectToAction("Register", "Auth");
            }

            // API'nin beklediği modele dönüştürme için ConfirmPassword'ü kaldırıyoruz
            var registerModel = new
            {
                email = userForRegister.Email,
                password = userForRegister.Password,
                firstName = userForRegister.FirstName,
                lastName = userForRegister.LastName,
                gender = userForRegister.Gender,
                phoneNumber = userForRegister.PhoneNumber
            };

            var httpClient = _httpClientFactory.CreateClient();
            var jsonRegister = JsonConvert.SerializeObject(registerModel);
            StringContent content = new StringContent(jsonRegister, Encoding.UTF8, "application/json");
            var responseMessage = await httpClient.PostAsync($"{_apiUrl}/api/auth/register", content);

            if (responseMessage.IsSuccessStatusCode)
            {
                // Başarılı kayıt, giriş sayfasına yönlendir
                TempData["Success"] = true;
                TempData["Message"] = "Kayıt işlemi başarılı! Lütfen giriş yapın.";
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                string responseContent = await responseMessage.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                TempData["RegisterFail"] = errorResponse?.Message ?? "Kayıt işlemi sırasında bir hata oluştu.";
                return RedirectToAction("Register", "Auth");
            }
        }

        [HttpPost("LoginPost")]
        public async Task<IActionResult> LoginPost(UserForLogin userForLogin)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonLogin = JsonConvert.SerializeObject(userForLogin);
            StringContent conten = new StringContent(jsonLogin, Encoding.UTF8, "application/json");
            var responseMessage = await httpClient.PostAsync($"{_apiUrl}/api/auth/login", conten);

            if (responseMessage.IsSuccessStatusCode)
            {
                var userForLoginSuccess = await GetUserForLogin(responseMessage);
                TempData["Message"] = userForLoginSuccess.Message;
                TempData["Success"] = userForLoginSuccess.Success;
                var jwtToken = userForLoginSuccess.Data.Token;
                var roleClaims = ExtractRoleClaimsFromJwtToken.GetRoleClaims(jwtToken);
                var userId = ExtractUserIdentityFromJwtToken.GetUserIdentityFromJwtToken(jwtToken);

                HttpContext.Session.SetString("Token", jwtToken);
                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("Email", userForLogin.Email);
                return await SignInUserByRole(roleClaims);
            }
            else
            {
                var userForLoginError = await GetUserForLogin(responseMessage);
                TempData["LoginFail"] = userForLoginError.Message;
                return RedirectToAction("Login", "Auth");
            }
        }

        private async Task<IActionResult> SignInUserByRole(List<string> roleClaims)
        {
            if (roleClaims != null && roleClaims.Any())
            {
                if (roleClaims.Contains(AdminRole))
                {
                    return await SignInUserByRoleClaim(AdminRole);
                }
                if (roleClaims.Contains(UserRole))
                {
                    return await SignInUserByRoleClaim(UserRole);
                }
            }
            return RedirectToAction("Login", "Auth");
        }

        private async Task<IActionResult> SignInUserByRoleClaim(string role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
            var userIdentity = new ClaimsIdentity(claims, role);
            var userPrincipal = new ClaimsPrincipal(userIdentity);
            await HttpContext.SignInAsync(userPrincipal);
            return RedirectToAction("Index", "Home");
        }

        private async Task<ApiDataResponse<UserForLogin>> GetUserForLogin(HttpResponseMessage responseMessage)
        {
            string responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiDataResponse<UserForLogin>>(responseContent);
        }
    }
}