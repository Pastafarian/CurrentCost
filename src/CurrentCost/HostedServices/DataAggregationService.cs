namespace CurrentCost.HostedServices;

public class DataAggregationService : BackgroundService
{
    //private readonly MonitorMessageConsumer _monitorMessageConsumer;
    //private readonly NotificationCreatedConsumer _notificationCreatedConsumer;

    //public DataAggregationService(CurrentCost.Consumers.MonitorMessageConsumer monitorMessageConsumer, CurrentCost.Consumers.NotificationCreatedConsumer notificationCreatedConsumer)
    //{
    //    _monitorMessageConsumer = monitorMessageConsumer;
    //    _notificationCreatedConsumer = notificationCreatedConsumer;
    //}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {


           await Task.Delay(10000, stoppingToken);
           await Task.Yield();
        }


        //return Task.FromResult(Task.CompletedTask);
    }
}
