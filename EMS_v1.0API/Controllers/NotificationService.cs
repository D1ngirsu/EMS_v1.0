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

    public async Task<GenericResponse> GetRecentNotificationsAsync()
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

                // Trả về GenericResponse với danh sách thông báo (có thể rỗng)
                var response = new GenericResponse
                {
                    Success = true,
                    Message = "Lấy danh sách thông báo thành công",
                    Data = notifications // Danh sách rỗng nếu không có thông báo
                };

                // Gửi thông báo qua SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveRecentNotifications", notifications);

                return response;
            }
        }
        catch (Exception ex)
        {
            return new GenericResponse
            {
                Success = false,
                Message = "Lỗi khi lấy thông báo: " + ex.Message
            };
        }
    }

    public class GenericResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}