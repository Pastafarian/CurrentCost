using Microsoft.AspNetCore.SignalR;

namespace CurrentCost.Consumers.SignalR
{
    public class MessagingHub : Hub<IMonitorMessageReceivedCommand>
    {
        public async Task MessageReceivedAsync(SignalRMonitorMessage monitorMessage) => await Clients.All.MessageReceivedAsync(monitorMessage);
    }
}
