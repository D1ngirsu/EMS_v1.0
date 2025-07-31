using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class NotificationHub : Hub
{
    public async Task SendNotification(string content)
    {
        await Clients.All.SendAsync("ReceiveNotification", content);
    }
}