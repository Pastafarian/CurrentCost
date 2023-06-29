using CurrentCost.Consumers;
using CurrentCost.Messages.Messages;
using MassTransit;

namespace CurrentCost.HostedServices;

public class DataAggregationService : BackgroundService
{
    private readonly MonitorMessageConsumer _monitorMessageConsumer;
    private readonly NotificationCreatedConsumer _notificationCreatedConsumer;

    public DataAggregationService(CurrentCost.Consumers.MonitorMessageConsumer monitorMessageConsumer, CurrentCost.Consumers.NotificationCreatedConsumer notificationCreatedConsumer)
    {
        _monitorMessageConsumer = monitorMessageConsumer;
        _notificationCreatedConsumer = notificationCreatedConsumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
