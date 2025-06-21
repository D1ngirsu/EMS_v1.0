using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/employee-application")]
[SessionAuthorize]
public class EmployeeApplicationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _imageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Img", "Employee_Applications");
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public EmployeeApplicationController(AppDbContext context)
    {
        _context = context;
        // Ensure directory exists
        if (!Directory.Exists(_imageBasePath))
        {
            Directory.CreateDirectory(_imageBasePath);
        }
    }

    private bool IsValidImage(IFormFile image)
    {
        if (image == null) return true; // Image is optional
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return false;
        }
        if (image.Length > _maxFileSize)
        {
            return false;
        }
        return true;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? eid, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_Applications
            .Include(e => e.Employee)
            .AsQueryable();

        if (eid.HasValue)
        {
            query = query.Where(e => e.Eid == eid.Value);
        }

        var totalCount = await query.CountAsync();
        var applications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = applications,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("{appId}")]
    public async Task<IActionResult> GetById(int appId)
    {
        var application = await _context.Employee_Applications
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.AppId == appId);

        if (application == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy đơn từ" });
        }

        return Ok(new { Success = true, Data = application });
    }

    [HttpGet("image/{appId}")]
    public async Task<IActionResult> GetApplicationImage(int appId)
    {
        var application = await _context.Employee_Applications
            .FirstOrDefaultAsync(e => e.AppId == appId);

        if (application == null || string.IsNullOrEmpty(application.Img))
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy ảnh đơn từ" });
        }

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", application.Img.TrimStart('/'));
        if (!System.IO.File.Exists(imagePath))
        {
            return NotFound(new { Success = false, Message = "Tệp ảnh không tồn tại" });
        }

        var imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
        var extension = Path.GetExtension(imagePath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        return File(imageBytes, contentType);
    }

    [HttpGet("employee/{eid}")]
    public async Task<IActionResult> GetByEmployeeId(int eid, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_Applications
            .Where(e => e.Eid == eid)
            .Include(e => e.Employee)
            .OrderByDescending(e => e.Date)
            .AsQueryable();
        var totalCount = await query.CountAsync();
        var applications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new
        {
            Success = true,
            Data = applications,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpPost]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> Create([FromForm] Employee_Application application, IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        // Validate image
        if (image != null && !IsValidImage(image))
        {
            return BadRequest(new { Success = false, Message = "Ảnh không hợp lệ. Chỉ chấp nhận định dạng .jpg, .jpeg, .png và kích thước tối đa 5MB." });
        }

        try
        {
            // Handle image upload
            if (image != null)
            {
                var fileExtension = Path.GetExtension(image.FileName);
                var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_imageBasePath, randomFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                application.Img = $"/Img/Employee_Applications/{randomFileName}";
            }

            _context.Employee_Applications.Add(application);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { appId = application.AppId },
                new { Success = true, Data = application, Message = "Tạo đơn từ thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{appId}")]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> Update(int appId, [FromForm] Employee_Application application, IFormFile? image)
    {
        if (appId != application.AppId)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        // Validate image
        if (image != null && !IsValidImage(image))
        {
            return BadRequest(new { Success = false, Message = "Ảnh không hợp lệ. Chỉ chấp nhận định dạng .jpg, .jpeg, .png và kích thước tối đa 5MB." });
        }

        // Handle image upload
        if (image != null)
        {
            var fileExtension = Path.GetExtension(image.FileName);
            var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_imageBasePath, randomFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            application.Img = $"/Img/Employee_Applications/{randomFileName}";
        }

        _context.Entry(application).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = application, Message = "Cập nhật đơn từ thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employee_Applications.AnyAsync(e => e.AppId == appId))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy đơn từ" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{appId}")]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> Delete(int appId)
    {
        var application = await _context.Employee_Applications.FindAsync(appId);
        if (application == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy đơn từ" });
        }

        try
        {
            // Delete associated image file
            if (!string.IsNullOrEmpty(application.Img))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", application.Img.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Employee_Applications.Remove(application);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Xóa đơn từ thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}