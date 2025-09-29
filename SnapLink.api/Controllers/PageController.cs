using Microsoft.AspNetCore.Mvc;
using SnapLink.api.Application.Services;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting;
using FluentValidation;

namespace SnapLink.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PageController : ControllerBase
    {
        private readonly IPageService _service;
        private readonly IValidator<CreatePageRequest> _validator;
        private readonly ITokenService _tokenService;
        private readonly string _jwtKey;
        public PageController(
             IPageService service,
             IValidator<CreatePageRequest> validator,
             ITokenService tokenService,
             string jwtKey) 
        {
            _service = service;
            _validator = validator;
            _tokenService = tokenService;
            _jwtKey = jwtKey;
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
                return BadRequest(result);

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
            if (page.Data == null)
                return NotFound(page.Message);

            if (!page.Data.IsPrivate)
                return BadRequest(page.Message);

            var isValid = await _service.ValideAcessCode(request);
            if (!isValid.Success)
                return Unauthorized("Invalid access code.");

            var token = _tokenService.GeneratePageToken(page.Data.Id);

            Response.Cookies.Append(
                $"PageToken_{page.Data.Name}",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                }
            );

            return Ok(new { token });
        }

        [HttpGet("private/{name}")]
        public async Task<IActionResult> GetPage(string name)
        {
            var page = await _service.GetPageByName(name);
            if (page.Data == null)
                return NotFound(page.Message);

            if (page.Data.IsPrivate && !TokenValidator.ValidateTokenToPage(HttpContext, page.Data.Id, _jwtKey))
                return Unauthorized("Token does not grant access to this page.");

            return Ok(page);
        }
    }
}
