using KariyerNet.Application.Services;

namespace KariyerNet.API.BackgroundServices
{
    public class EvaluationWorker : BackgroundService
    {
        private readonly EvaluationQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<EvaluationWorker> _logger;

        public EvaluationWorker(
            EvaluationQueue queue,
            IServiceScopeFactory scopeFactory,
            IHostEnvironment environment,
            ILogger<EvaluationWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _environment = environment;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var applicationId in _queue.DequeueAllAsync(stoppingToken))
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var service = scope.ServiceProvider.GetRequiredService<JobApplicationService>();
                        await service.EvaluateAsync(applicationId, _environment.ContentRootPath);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Başvuru {ApplicationId} için AI değerlendirmesi başarısız oldu.", applicationId);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Uygulama kapanırken normal çıkış
            }
        }
    }
}