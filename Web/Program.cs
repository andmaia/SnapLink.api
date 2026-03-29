using System.Net;
using System.Text.Json;
using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Web.Services;


var builder = WebApplication.CreateBuilder(args);
Env.Load();

const long MaxFileSizeBytes = 209715200; // 200 MB

// Kestrel: permite requisições de até 200 MB
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = MaxFileSizeBytes;
});

// IIS (caso usado em produção)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = MaxFileSizeBytes;
});

// Limite de formulários multipart no pipeline do ASP.NET
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxFileSizeBytes;
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
var defaultJsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
builder.Services.AddSingleton(defaultJsonOptions);
// Separar em dois pipelines: um para uploads, outro para chamadas normais

// pipeline normal — adicionar circuit breaker
builder.Services.AddHttpClient<ISnapLinkApiClient, SnapLinkApiClient>(ConfigureBase)
    .AddResilienceHandler("snaplink-pipeline", pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(10));

        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: HttpRequestException } => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.ServiceUnavailable } => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.TooManyRequests } => PredicateResult.True(),
                _ => PredicateResult.False()
            }
        });

        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: HttpRequestException } => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.ServiceUnavailable } => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            OnOpened = args =>
            {
                return ValueTask.CompletedTask;
            }
        });

        pipeline.AddTimeout(TimeSpan.FromSeconds(30));
    });

builder.Services.AddHttpClient<ISnapLinkUploadClient, SnapLinkApiClient>(client =>
{
    ConfigureBase(client);
    client.Timeout = TimeSpan.FromMinutes(5);
})
    .AddResilienceHandler("snaplink-upload-pipeline", pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromMinutes(5));
    });

static void ConfigureBase(HttpClient client)
{
    var baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");
    if (string.IsNullOrEmpty(baseUrl))
        throw new InvalidOperationException("A variável de ambiente 'API_BASE_URL' não foi configurada.");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}



var app = builder.Build();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        var message = exception switch
        {
            BrokenCircuitException => "Serviço indisponível. Tente em instantes.",
            TimeoutRejectedException => "Requisição expirou. Tente novamente.",
            HttpRequestException => "Erro de comunicação com o serviço.",
            _ => "Erro inesperado."
        };

        // passa a mensagem pro TempData via cookie (truque para redirect)
        context.Response.Cookies.Append("ErrorMessage", message, new CookieOptions
        {
            MaxAge = TimeSpan.FromSeconds(30)
        });

        context.Response.Redirect("/error");
    });
});
if (!app.Environment.IsDevelopment())
{
   
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();