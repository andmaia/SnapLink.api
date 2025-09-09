using Microsoft.AspNetCore.Mvc;
using SnapLink.api.Crosscutting.DTO.Request;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Response;
using Web.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Web.Controllers
{
    public class PageFileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PageFileController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient GetHttpClientWithToken()
        {
            var client = _httpClientFactory.CreateClient("SnapLinkApi");
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["PageToken"];

           
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Cookie", $"PageToken={token}");
            }
            return client;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFile(CreatePageFileRequest request)
        {
            if (request == null )
            {
                TempData["PageFileMessage"] = "O arquivo enviado está vazio ou excede o tamanho permitido.";
                return Redirect($"/");
            }

            var client = GetHttpClientWithToken();
            request.ContentType = request.Data.ContentType;
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(request.PageId.ToString()), "PageId");
            form.Add(new StringContent(request.ContentType.ToString()), "ContentType");
            form.Add(new StringContent(request.FileName), "FileName");
            form.Add(new StringContent(request.TimeToExpire.ToString()), "TimeToExpire");
            form.Add(new StringContent(request.PageName), "PageName"); 
            form.Add(new StringContent(request.IsPagePrivate.ToString()), "IsPagePrivate"); 


            if (request.Data != null)
                form.Add(new StreamContent(request.Data.OpenReadStream()), "Data", request.Data.FileName);

            var response = await client.PostAsync("/PageFile/upload", form);

            if (response.IsSuccessStatusCode)
            {
                return Redirect($"/page/{request.PageName}");
            }

            var error = await response.Content.ReadAsStringAsync();
            var resultMessage = JsonSerializer.Deserialize<ResultMessageDTO>(error, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            TempData["PageFileMessage"] = resultMessage?.message ?? "Erro desconhecido ao criar arquivo.";

            return Redirect($"/page/{request.PageName}");
        }
        [HttpGet]
        public async Task<IActionResult> ListFiles(string pageId, string pageName)
        {
            if (string.IsNullOrWhiteSpace(pageId))
            {
                TempData["PageFileMessage"] = "Página inválida.";
                return RedirectToAction("Index");
            }

            var client = GetHttpClientWithToken();
            var response = await client.GetAsync($"/pagefile/page/{pageId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["PageFileMessage"] = "Não foi possível listar os arquivos.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<Result<List<PageResponse>>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


            if (!result.Success || result.Data == null)
            {
                TempData["PageFileMessage"] = result.Message ?? "Nenhum arquivo encontrado.";
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }


        [HttpGet]
        public async Task<IActionResult> DownloadFile(string id, string pageName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["PageFileMessage"] = "Arquivo inválido.";
                return Redirect($"/page/{pageName}");
            }

            var client = GetHttpClientWithToken();
            var response = await client.GetAsync($"/pagefile/download/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["PageFileMessage"] = "Erro desconhecido ao baixar arquivo.";
                return Redirect($"/page/{pageName}");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? $"file-{id}";

            return File(bytes, contentType, fileName);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteFile(string id, string pageName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["PageFileMessage"] = "Arquivo inválido.";
                return Redirect($"/page/{pageName}");
            }

            var client = GetHttpClientWithToken();
            var response = await client.DeleteAsync($"/pagefile/{id}");

            if (response.IsSuccessStatusCode)
                return Redirect($"/page/{pageName}");

            TempData["PageFileMessage"] = "Erro ao remover o arquivo.";
            return Redirect($"/page/{pageName}"); 
        }

    }
}
