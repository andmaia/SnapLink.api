using System.ComponentModel.DataAnnotations;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SnapLink.api.Crosscutting;
using SnapLink.Api.Crosscutting;
using SnapLink.Api.Crosscutting.Events;

namespace SnapLink.Api.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {

        protected IActionResult CustomResponse(FluentValidation.Results.ValidationResult result)
        {

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                foreach(var error in errors)
                {
                    MessageService.AddMessage(error);
                }

                var messages = MessageService.GetAllDescriptions().ToList();
                MessageService.ClearMessages();

                return BadRequest(new Result<object>(null, false, messages));
            }
            return CustomResponse();

        }
        protected IActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                var errors = modelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                foreach (var error in errors)
                {
                    MessageService.AddMessage(error);
                }

                var messages = MessageService.GetAllDescriptions().ToList();
                MessageService.ClearMessages();

                return BadRequest(new Result<object>(null, false, messages));
            }

            return CustomResponse();
        }


       protected IActionResult CustomResponseUnathorized()
        {
           
                var messages = MessageService.GetAllDescriptions().ToList();
                MessageService.ClearMessages();
                return Unauthorized(new Result<object>(null, false, messages));

        }

        protected IActionResult CustomResponse(object? obj = null,bool success=false)
        {
            if (obj == null && !success)
            {
                var messages = MessageService.GetAllDescriptions().ToList();
                MessageService.ClearMessages();

                return NotFound(new Result<object>(null, success, messages));
            }

            if (MessageService.HasMessage())
            {
                var messages = MessageService.GetAllDescriptions().ToList();
                MessageService.ClearMessages();

                return BadRequest(new Result<object>(null, success, messages));
            }

            return Ok(new Result<object>(obj, success, "Operação realizada com sucesso."));
        }
    }
}
