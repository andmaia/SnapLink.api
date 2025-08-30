using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SnapLink.Api.Domain;
using SnapLink.api.Infra.SnapLink.Api.Infra;
using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;

namespace SnapLink.Tests.Configuration
{
    public class AppFixture<TStartup> : IDisposable where TStartup : class
    {
        public HttpClient Client { get; }
        private readonly WebApplicationFactory<TStartup> _factory;

        public AppFixture()
        {
            _factory = new WebApplicationFactory<TStartup>()
                .WithWebHostBuilder(builder =>
                {
                    // Define o ambiente de teste
                    builder.UseEnvironment("Test");

                    builder.ConfigureServices(services =>
                    {
                        // Remove qualquer DbContext já registrado
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Adiciona InMemoryDatabase
                        services.AddDbContext<AppDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("TestDb");
                        });

                        // Seed inicial dentro do mesmo ServiceProvider
                        using var scope = services.BuildServiceProvider().CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        if (!db.Pages.Any())
                        {
                            db.Pages.Add(new Page
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "PrivatePage",
                                IsPrivate = true,
                                AccessCode = "1234"
                            });

                            db.Pages.Add(new Page
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "PublicPage",
                                IsPrivate = false,
                                AccessCode = null
                            });

                            db.SaveChanges();
                        }
                    });
                });

            Client = _factory.CreateClient();
        }

        public void Dispose()
        {
            Client.Dispose();
            _factory.Dispose();
        }
    }
}
