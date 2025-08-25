using Microsoft.AspNetCore.Mvc;
using SnapLink.api.Application.Services;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace SnapLink.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PageController : ControllerBase
    {
        private readonly IPageService _service;
        private readonly IValidator<CreatePageRequest> _validator;
        private readonly ITokenService _tokenService;

        public PageController(IPageService service, IValidator<CreatePageRequest> validator, ITokenService tokenService)
        {
            _service = service;
            _validator = validator;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePageRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(Result<string>.Fail(string.Join(" | ", errors)));
            }

            var result = await _service.CreatePageWithUniqueName(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(Result<string>.Ok("Página criada com sucesso."));
        }


        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _service.GetPageByName(name);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("access")]
        public async Task<IActionResult> AccessPage([FromBody] ValideAcessCodeRequest request)
        {
            var page = await _service.GetPageByName(request.Name);
            if (page == null)
                return NotFound("Page not found.");

            if (!page.Data.IsPrivate)
                return BadRequest("Page is not private.");

            var isValid =await _service.ValideAcessCode(request);
            if (!isValid.Success)
                return Unauthorized("Invalid access code.");

            var token = _tokenService.GeneratePageToken(page.Data.Id);
            return Ok(new { token });
        }

        [HttpGet("private/{name}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetPage(string name)
        {
            var page = await _service.GetPageByName(name);
            if (page == null)
                return NotFound("Page not found.");
            if (!page.Data.IsPrivate)
                return BadRequest("Page is not private.");

            var userClaims = HttpContext.User.Claims;
            var pageIdClaim = userClaims.FirstOrDefault(c => c.Type == "pageId")?.Value;

            if (page.Data.IsPrivate && pageIdClaim != page.Data.Id)
                return Unauthorized("Token does not grant access to this page.");

            return Ok(page);
        }
    }
}
