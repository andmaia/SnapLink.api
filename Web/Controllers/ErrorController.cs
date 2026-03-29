using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    // Controllers/ErrorController.cs
    public class ErrorController : Controller
    {
        [HttpGet("/error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            var message = Request.Cookies["ErrorMessage"] ?? "Erro inesperado.";
            Response.Cookies.Delete("ErrorMessage");
            ViewBag.Message = message;
            return View();
        }
    }
}
