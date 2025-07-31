using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

public class NotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;

    public NotificationService(IHubContext<NotificationHub> hubContext, IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
    }

    public async Task CreateAndSendNotificationAsync(string content)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var notification = new HR_Notification
                {
                    Content = content,
                    DateTime = DateTime.UtcNow
                };

                context.HR_Notifications.Add(notification);
                await context.SaveChangesAsync();
            }

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", content);
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating and sending notification: " + ex.Message);
        }
    }

    public async Task GetRecentNotificationsAsync()
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var notifications = await context.HR_Notifications
                    .OrderByDescending(n => n.DateTime)
                    .Take(10)
                    .Select(n => new { n.Id, n.Content, n.DateTime })
                    .ToListAsync();

                await _hubContext.Clients.All.SendAsync("ReceiveRecentNotifications", notifications);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error getting recent notifications: " + ex.Message);
        }
    }
}