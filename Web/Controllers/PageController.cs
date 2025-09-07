using Microsoft.AspNetCore.Mvc;
using SnapLink.api.Controllers;
using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Web.Models;

namespace Web.Controllers
{
    [Route("page")] 
    public class PageController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PageController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Index(string name)
        {
            ViewBag.PageName = name;
            ViewBag.RequireAccessCode = false;

            var client = _httpClientFactory.CreateClient("SnapLinkApi");

            var token = Request.Cookies[$"PageToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"/page/private/{name}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ViewBag.RequireAccessCode = true;
                    return View(); 
                }

                TempData["Message"] = $"Página '{name}' não encontrada.";
                return RedirectToAction("Index", "Home");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Result<PageResponse>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewBag.Page = result?.Data;

            List<PageFileResponse> files = new List<PageFileResponse>();
            if (ViewBag.Page != null)
            {
                var fileResponse = await client.GetAsync($"/pagefile/page/{ViewBag.Page.Id}");
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
                    TempData["Message"] = "Não foi possível listar os arquivos da página.";
                }
            }

            return View(files);
        }



        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePage(CreatePageRequest request)
        {
            var client = _httpClientFactory.CreateClient("SnapLinkApi");
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/page", content);

            if (response.IsSuccessStatusCode)
            {
                return Redirect($"/page/{request.Name}");
            }

            var error = await response.Content.ReadAsStringAsync();
            var resultMessage = JsonSerializer.Deserialize<ResultMessageDTO>(error, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            TempData["Message"] = resultMessage?.message ?? "Erro desconhecido ao criar página.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("access-private")]
        public async Task<IActionResult> AccessPrivate(string name, string accessCode)
        {
            var client = _httpClientFactory.CreateClient("SnapLinkApi");

            var request = new
            {
                Name = name,
                AccessCode = accessCode
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/page/access", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Código de acesso inválido.";
                return RedirectToAction("Index", "Home");
            }

            var tokenJson = await response.Content.ReadAsStringAsync();

            var tokenResult = JsonSerializer.Deserialize<TokenResponseDTO>(tokenJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var token = tokenResult?.Token;

            if (!string.IsNullOrEmpty(token))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append($"PageToken", token, cookieOptions);
            }

            return Redirect($"/page/{name}");
        }
    }

    // DTO para desserializar o token retornado da API
    public class TokenResponseDTO
    {
        public string Token { get; set; }
    }
}
