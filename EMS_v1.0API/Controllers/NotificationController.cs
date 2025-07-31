using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(NotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationRequest request)
    {
        if (string.IsNullOrEmpty(request.Content))
        {
            return BadRequest(new { Success = false, Message = "Nội dung thông báo không được để trống" });
        }

        try
        {
            await _notificationService.CreateAndSendNotificationAsync(request.Content);
            _logger.LogInformation("Notification created and sent: {Content}", request.Content);
            return Ok(new { Success = true, Message = "Thông báo đã được tạo và gửi thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification: {Content}", request.Content);
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo thông báo: " + ex.Message });
        }
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentNotifications()
    {
        try
        {
            await _notificationService.GetRecentNotificationsAsync();
            return Ok(new { Success = true, Message = "Lấy danh sách thông báo gần đây thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent notifications");
            return BadRequest(new { Success = false, Message = "Lỗi khi lấy thông báo: " + ex.Message });
        }
    }
}

public class NotificationRequest
{
    public string Content { get; set; }
}