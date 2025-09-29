using System.Net;
using System.Text.Json;
using SnapLink.Api.Crosscutting.Events;

namespace SnapLink.api.Crosscutting.Middlewares
{
    public class ErrorHandlerMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                MessageService.AddMessage(ex.Message);

                var result = new Result<object>(
                    data: null,
                    success: false,
                    erros: MessageService.GetAllDescriptions().ToList()
                );

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(result));
            }
        }
    }
}
