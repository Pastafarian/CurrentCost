namespace CurrentCost.Consumers.SignalR
{
    public interface IMonitorMessageReceivedCommand
    {
        Task MessageReceivedAsync(SignalRMonitorMessage message);
    }
}
