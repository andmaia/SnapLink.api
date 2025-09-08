using SnapLink.api.Application.Services;
using SnapLink.api.Infra;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv; // << Importante

var builder = WebApplication.CreateBuilder(args);

// Carrega variáveis do arquivo .env na raiz do projeto
Env.Load();

// Lê as variáveis do ambiente
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? throw new Exception("DB_CONNECTION_STRING não definida!");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new Exception("JWT_KEY não definida!");
var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
    ?? throw new Exception("API_BASE_URL não definida!");

// Configurações do .NET
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Injeção de dependências
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPageFileService, PageFileService>();
builder.Services.AddScoped<IPageFileRepository, PageFileRepository>();
builder.Services.AddScoped<IPageRepository, PageRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOFWork>();

// Configuração do PostgreSQL com Npgsql
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddHostedService<PageFileExpirationService>();

// Configuração JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.HttpContext.Request.Cookies["PageToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };

    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configuração Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SnapLink API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Digite 'Bearer {seu token}' para autenticação."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
