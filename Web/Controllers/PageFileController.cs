// PageFileController.cs
using Microsoft.AspNetCore.Mvc;
using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;
using System.Text.Json;
using Web.Models;
using Web.Services;

namespace Web.Controllers
{
    public class PageFileController : Controller
    {

        private readonly ISnapLinkApiClient _apiClient;
        private readonly ISnapLinkUploadClient _uploadClient;

        public PageFileController(ISnapLinkApiClient apiClient, ISnapLinkUploadClient uploadClient)
        {
            _apiClient = apiClient;
            _uploadClient = uploadClient;
        }

        private string? GetToken() =>
            HttpContext.Request.Cookies["PageToken"];

        [HttpPost]
        public async Task<IActionResult> CreateFile(CreatePageFileRequest request)
        {
            if (request == null)
            {
                TempData["PageFileMessage"] = "O arquivo enviado está vazio ou excede o tamanho permitido.";
                return Redirect("/");
            }

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

            var response = await _uploadClient.PostMultipartAsync("pageFile/upload", form, GetToken());

            if (response.IsSuccessStatusCode)
                return Redirect($"/page/{request.PageName}");

            var error = await response.Content.ReadAsStringAsync();
            var resultMessage = JsonSerializer.Deserialize<ResultMessageDTO>(error, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ViewBag.Errors = resultMessage?.Errors ?? new List<string> { "Erro desconhecido ao criar página" };
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

            var response = await _apiClient.GetAsync($"pagefile/page/{pageId}", GetToken());

            if (!response.IsSuccessStatusCode)
            {
                TempData["PageFileMessage"] = "Não foi possível listar os arquivos.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Result<List<PageResponse>>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || !result.Success || result.Data == null)
            {
                TempData["PageFileMessage"] = result?.Message ?? "Nenhum arquivo encontrado.";
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

            var response = await _apiClient.GetAsync($"pagefile/download/{id}", GetToken());

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

            var response = await _apiClient.DeleteAsync($"pagefile/{id}", GetToken());

            if (response.IsSuccessStatusCode)
                return Redirect($"/page/{pageName}");

            TempData["PageFileMessage"] = "Erro ao remover o arquivo.";
            return Redirect($"/page/{pageName}");
        }
    }
}