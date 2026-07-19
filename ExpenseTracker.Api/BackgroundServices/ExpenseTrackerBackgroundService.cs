using ExpenseTracker.Application.Interfaces;

namespace ExpenseTracker.Api.BackgroundServices;

public class ExpenseTrackerBackgroundService
    : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpenseTrackerBackgroundService> _logger;

    public ExpenseTrackerBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ExpenseTrackerBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Expense Tracker Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _serviceProvider.CreateScope();

                var processor =
                    scope.ServiceProvider
                        .GetRequiredService<
                            ISystemBackgroundProcessor>();

                _logger.LogInformation(
                    "Running system background processing...");

                await processor.ProcessAsync(
                    stoppingToken);

                _logger.LogInformation(
                    "Background processing completed at {Time}.",
                    DateTimeOffset.Now);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Background service execution failed.");
            }

            try
            {
#if DEBUG
                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
#else
                await Task.Delay(
                    TimeSpan.FromHours(1),
                    stoppingToken);
#endif
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Expense Tracker Background Service stopped.");
    }
}