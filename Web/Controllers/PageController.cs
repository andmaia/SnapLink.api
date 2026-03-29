using Microsoft.AspNetCore.Mvc;
using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;
using System.Text;
using System.Text.Json;
using Web.Models;
using Web.Services;

namespace Web.Controllers
{
    [Route("page")]
    public class PageController : Controller
    {
        private readonly ISnapLinkApiClient _apiClient;

        public PageController(ISnapLinkApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet("")]
        public IActionResult RedirectToRoot() => Redirect("/");

        [HttpGet("{name}")]
        public async Task<IActionResult> Index(string name)
        {
            ViewBag.PageName = name;
            ViewBag.RequireAccessCode = false;
            ViewBag.Erros = new List<string>();

            var token = Request.Cookies["PageToken"];

            var response = await _apiClient.GetAsync($"page/private/{name}", token);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ViewBag.RequireAccessCode = true;
                    return View();
                }

                var error = await response.Content.ReadAsStringAsync();
                var resultMessage = JsonSerializer.Deserialize<ResultMessageDTO>(error, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                TempData["Errors"] = JsonSerializer.Serialize(resultMessage?.Errors ?? new List<string> { "Erro desconhecido" });
                return RedirectToAction("Index", "Home");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Result<PageResponse>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewBag.Page = result?.Data;

            var files = new List<PageFileResponse>();

            if (ViewBag.Page != null)
            {
                var fileResponse = await _apiClient.GetAsync($"pagefile/page/{ViewBag.Page.Id}", token);

                if (fileResponse.IsSuccessStatusCode)
                {
                    var fileJson = await fileResponse.Content.ReadAsStringAsync();
                    var fileResult = JsonSerializer.Deserialize<Result<List<PageFileResponse>>>(fileJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (fileResult?.Success == true && fileResult.Data != null)
                        files = fileResult.Data;
                }
                else
                {
                    TempData["Errors"] = JsonSerializer.Serialize(new List<string> { "Erro ao listar página" });
                }
            }

            return View(files);
        }

        public IActionResult Upload() => View();

        [HttpPost("create")]
        public async Task<IActionResult> CreatePage(CreatePageRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync("page", content);

            if (response.IsSuccessStatusCode)
                return Redirect($"/page/{request.Name}");

            var error = await response.Content.ReadAsStringAsync();
            var resultMessage = JsonSerializer.Deserialize<ResultMessageDTO>(error, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            TempData["Errors"] = JsonSerializer.Serialize(resultMessage?.Errors ?? new List<string> { "Erro desconhecido" });
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("access-private")]
        public async Task<IActionResult> AccessPrivate(string name, string accessCode)
        {
            var payload = new { Name = name, AccessCode = accessCode };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync("page/access", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var resultMessage = JsonSerializer.Deserialize<ResultMessageDTO>(error, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                TempData["Errors"] = JsonSerializer.Serialize(resultMessage?.Errors ?? new List<string> { "Erro desconhecido" });
                return RedirectToAction("Index", "Home");
            }

            var tokenJson = await response.Content.ReadAsStringAsync();
            var tokenResult = JsonSerializer.Deserialize<TokenResponseDTO>(tokenJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var token = tokenResult?.Data?.Token;

            if (!string.IsNullOrEmpty(token))
            {
                Response.Cookies.Append("PageToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SameSite = SameSiteMode.Strict
                });
            }

            return Redirect($"/page/{name}");
        }
    }
}