using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapLink.api.Application.Services;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting;
using SnapLink.api.Infra;
using SnapLink.Api.Controllers;
using SnapLink.api.Crosscutting.Events;


[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PageFileController : MainController
{
    private readonly IPageFileService _service;
    private readonly IPageRepository _pageRepository;
    private readonly IValidator<CreatePageFileRequest> _validator;
    private readonly string _jwtKey;

    public PageFileController(
        IPageFileService service,
        IPageRepository pageRepository,
        IValidator<CreatePageFileRequest> validator,
        string jwtKey)
    {
        _service = service;
        _pageRepository = pageRepository;
        _validator = validator;
        _jwtKey = jwtKey;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Create([FromForm] CreatePageFileRequest request)
    {
        if (request.IsPagePrivate && !TokenValidator.ValidateTokenToPage(HttpContext, request.PageId, _jwtKey))
            return CustomResponseUnathorized();

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return CustomResponse(validationResult);
        }

        var result = await _service.CreatePageFile(request);
        if (!result)
            return CustomResponse(success:false);

        return CustomResponse(success:true);
    }

    [HttpGet("page/{pageId}")]
    public async Task<IActionResult> GetAllByPageId(string pageId)
    {
        var page = await _pageRepository.GetByIdAsync(pageId);
        if (page == null)
            return CustomResponse(success:false);

        if (page.IsPrivate && !TokenValidator.ValidateTokenToPage(HttpContext, pageId, _jwtKey))
            return CustomResponseUnathorized();

        var result = await _service.GetAllByPageId(pageId);
        if (result==null)
            return CustomResponse(success:false);

        return CustomResponse(result,success:true);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    { 
        var result = await _service.DeletePageFile(id); 
        if (!result) return  CustomResponse(success:false); 
        return CustomResponse(success:true); 
    }
    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(string id)
    {
        var fileMeta = await _service.GetById(id);
        if (fileMeta == null)
            return CustomResponse(success: false);

        var data = await _service.DowloadPageFile(id);
        if (data == null || data.Length == 0)
            return CustomResponse(success: false);

        // Retorna diretamente o arquivo
        return File(data, fileMeta.ContentType ?? "application/octet-stream", fileMeta.FileName ?? "file");
    }

}
