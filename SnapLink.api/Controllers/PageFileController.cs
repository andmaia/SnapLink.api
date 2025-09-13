using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapLink.api.Application.Services;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting;
using SnapLink.api.Infra;

[ApiController]
[Route("[controller]")]
public class PageFileController : ControllerBase
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
            return Unauthorized("Token does not grant access to this page.");

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(Result<string>.Fail(string.Join(" | ", errors)));
        }

        var result = await _service.CreatePageFile(request);
        if (!result.Success)
            return BadRequest(result);

        return Ok(Result<string>.Ok("File uploaded successfully."));
    }

    [HttpGet("page/{pageId}")]
    public async Task<IActionResult> GetAllByPageId(string pageId)
    {
        var page = await _pageRepository.GetByIdAsync(pageId);
        if (page == null)
            return NotFound("Page not found");

        if (page.IsPrivate && !TokenValidator.ValidateTokenToPage(HttpContext, pageId, _jwtKey))
            return Unauthorized("Token does not grant access to this page.");

        var result = await _service.GetAllByPageId(pageId);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    { 
        var result = await _service.DeletePageFile(id); 
        if (!result.Success) return BadRequest(result); 
        return Ok(result); 
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(string id) 
    { 
        var fileMetaResult = await _service.GetById(id); 
        if (!fileMetaResult.Success)
            return NotFound(fileMetaResult); 
        var fileMeta = fileMetaResult.Data; 
        var dataResult = await _service.DowloadPageFile(id); 
        if (!dataResult.Success)
            return NotFound(dataResult); 
        var data = dataResult.Data;
        
        return File(data, fileMeta.ContentType ?? "application/octet-stream", fileMeta.FileName ?? "file");
    }

}
