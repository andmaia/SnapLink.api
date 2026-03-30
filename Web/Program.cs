using System.Net;
using System.Text.Json;
using DotNetEnv;
using Microsoft.AspNetCore.DataProtection;
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

const long MaxFileSizeBytes = 209715200;

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = MaxFileSizeBytes;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = MaxFileSizeBytes;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxFileSizeBytes;
});

// ✅ persiste chaves entre restarts do container
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/etc/snaplink-keys"))
    .SetApplicationName("SnapLink");

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var defaultJsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
builder.Services.AddSingleton(defaultJsonOptions);

builder.Services.AddHttpClient<ISnapLinkApiClient, SnapLinkApiClient>(ConfigureBase)
    .AddResilienceHandler("snaplink-pipeline", pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(35));

        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: HttpRequestException } => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.ServiceUnavailable } => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.TooManyRequests } => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            DelayGenerator = args =>
            {
                var retryAfter = args.Outcome.Result?.Headers.RetryAfter;
                if (retryAfter?.Delta != null)
                    return new ValueTask<TimeSpan?>(retryAfter.Delta);
                if (retryAfter?.Date != null)
                    return new ValueTask<TimeSpan?>(retryAfter.Date.Value - DateTimeOffset.UtcNow);

                return new ValueTask<TimeSpan?>(
                    TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber + 2)));
            }
        });

        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.8,
            MinimumThroughput = 5,
            SamplingDuration = TimeSpan.FromSeconds(60),
            BreakDuration = TimeSpan.FromSeconds(60),
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: HttpRequestException } => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.ServiceUnavailable } => PredicateResult.True(),
                _ => PredicateResult.False()
            }
        });

       
        pipeline.AddTimeout(TimeSpan.FromMinutes(3));
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

        context.Response.Cookies.Append("ErrorMessage", message, new CookieOptions
        {
            MaxAge = TimeSpan.FromSeconds(30),
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        });

        context.Response.Redirect("/error");
    });
});

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();