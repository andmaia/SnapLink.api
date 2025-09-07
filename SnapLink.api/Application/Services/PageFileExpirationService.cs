using Microsoft.EntityFrameworkCore;
using SnapLink.api.Infra;

namespace SnapLink.api.Application.Services
{
    public class PageFileExpirationService : BackgroundService
    {
        private readonly ILogger<PageFileExpirationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PageFileExpirationService(ILogger<PageFileExpirationService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PageFileExpirationService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var expiredFiles = await context.PageFiles
                        .Where(pf => pf.IsActive && pf.ExpiresAt != null && pf.ExpiresAt <= DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    if (expiredFiles.Any())
                    {
                        foreach (var file in expiredFiles)
                        {
                            file.Disable(); 
                        }

                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"{expiredFiles.Count} PageFiles desativados.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar PageFileExpirationService.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("PageFileExpirationService stopped.");
        }
    }
}
