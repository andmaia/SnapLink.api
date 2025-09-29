using Microsoft.AspNetCore.Mvc;
using SnapLink.api.Application.Services;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting;
using FluentValidation;
using SnapLink.Api.Controllers;
using SnapLink.Api.Crosscutting.Events;

namespace SnapLink.api.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class PageController : MainController
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
                return CustomResponse(validationResult);

            var result = await _service.CreatePageWithUniqueName(request);
            if (!result)
                return CustomResponse(result);

            return CustomResponse(success:true);
        }

        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _service.GetPageByName(name);

            if (result ==null)
                return CustomResponse(success:false);

            return CustomResponse(result,true);
        }

        [HttpPost("access")]
        public async Task<IActionResult> AccessPage([FromBody] ValideAcessCodeRequest request)
        {
            var page = await _service.GetPageByName(request.Name);
            if (page == null)
                return CustomResponse(success:false);

            if (!page.IsPrivate)
            {
                MessageService.AddMessage("Página não é privada");
                return CustomResponse(success: false);

            }

            var isValid = await _service.ValideAcessCode(request);
            if (!isValid)
                return CustomResponseUnathorized();

            var token = _tokenService.GeneratePageToken(page.Id);

            Response.Cookies.Append(
                $"PageToken_{page.Name}",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                }
            );

            return CustomResponse(new { token },success:true);
        }

        [HttpGet("private/{name}")]
        public async Task<IActionResult> GetPage(string name)
        {
            var page = await _service.GetPageByName(name);
            if (page == null)
                return CustomResponse(success:false);

            if (page.IsPrivate && !TokenValidator.ValidateTokenToPage(HttpContext, page.Id, _jwtKey))
                return CustomResponseUnathorized();

            return CustomResponse(page,true);
        }
    }
}
