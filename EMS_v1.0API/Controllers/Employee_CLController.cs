using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

// Employee_CL Controller
[ApiController]
[Route("api/employee-cl")]
[SessionAuthorize]
public class EmployeeCLController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeeCLController> _logger;
    private readonly string _imageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Img", "EmployeeCL");
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public EmployeeCLController(AppDbContext context, ILogger<EmployeeCLController> logger)
    {
        _context = context;
        _logger = logger;
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
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_CLs
            .Include(e => e.Employee)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var employeeCLs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = employeeCLs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("{cid}")]
    public async Task<IActionResult> GetByCid(int cid)
    {
        var employeeCL = await _context.Employee_CLs
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.Cid == cid);

        if (employeeCL == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin hợp đồng lao động" });
        }

        return Ok(new { Success = true, Data = employeeCL });
    }

    [HttpGet("eid/{eid}")]
    public async Task<IActionResult> GetByEid(int eid, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_CLs
            .Include(e => e.Employee)
            .Where(e => e.Eid == eid)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var employeeCLs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeCLDto
            {
                Cid = e.Cid,
                Eid = e.Eid,
                EmployeeUser = e.EmployeeUser,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                ExpectedEndDate = e.ExpectedEndDate,
                Status = e.Status,
                Img = e.Img,
                SignDate = e.SignDate
            })
            .ToListAsync();

        if (employeeCLs == null || !employeeCLs.Any())
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin hợp đồng lao động cho nhân viên này" });
        }

        return Ok(new
        {
            Success = true,
            Data = employeeCLs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpPost]
    [SessionAuthorize(RequiredRole = new[] { "HR" })]
    public async Task<IActionResult> Create([FromForm] Employee_CL employeeCL, IFormFile? image)
    {
        _logger.LogInformation("CreateEmployeeCL called with employeeCL: {Cid}, Image provided: {ImageProvided}, Image Length: {ImageLength}, Img Path: {ImgPath}",
            employeeCL.Cid, image != null, image?.Length, employeeCL.Img);

        // Loại bỏ yêu cầu bắt buộc cho Img trong ModelState
        ModelState.Remove("Img");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList());
            _logger.LogWarning("Invalid model state: {Errors}", JsonSerializer.Serialize(errors));
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = errors });
        }

        // Validate image
        if (image != null && !IsValidImage(image))
        {
            _logger.LogWarning("Invalid image format or size. File: {FileName}, Size: {FileSize}",
                image.FileName, image.Length);
            return BadRequest(new { Success = false, Message = "Ảnh không hợp lệ. Chỉ chấp nhận định dạng .jpg, .jpeg, .png và kích thước tối đa 5MB." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Handle image upload
            if (image != null)
            {
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_imageBasePath, randomFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                employeeCL.Img = $"/Img/EmployeeCL/{randomFileName}";
                _logger.LogInformation("Image uploaded successfully: {FilePath}", employeeCL.Img);
            }
            else
            {
                employeeCL.Img = null; // Ensure Img is null if no image is provided
                _logger.LogInformation("No image provided for EmployeeCL: {Cid}", employeeCL.Cid);
            }

            // Add Employee_CL to database
            _context.Employee_CLs.Add(employeeCL);
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetByCid), new { cid = employeeCL.Cid },
                new { Success = true, Data = employeeCL, Message = "Tạo thông tin hợp đồng lao động thành công" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating EmployeeCL: {Cid}", employeeCL.Cid);
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{cid}")]
    [SessionAuthorize(RequiredRole = new[] { "HR" })]
    public async Task<IActionResult> Update(int cid, [FromForm] Employee_CL employeeCL, IFormFile? image)
    {
        if (cid != employeeCL.Cid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        // Loại bỏ yêu cầu bắt buộc cho Img trong ModelState
        ModelState.Remove("Img");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList());
            _logger.LogWarning("Invalid model state: {Errors}", JsonSerializer.Serialize(errors));
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = errors });
        }

        // Validate image
        if (image != null && !IsValidImage(image))
        {
            _logger.LogWarning("Invalid image format or size. File: {FileName}, Size: {FileSize}",
                image.FileName, image.Length);
            return BadRequest(new { Success = false, Message = "Ảnh không hợp lệ. Chỉ chấp nhận định dạng .jpg, .jpeg, .png và kích thước tối đa 5MB." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Fetch existing Employee_CL
            var existingEmployeeCL = await _context.Employee_CLs.FindAsync(cid);
            if (existingEmployeeCL == null)
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy thông tin hợp đồng lao động" });
            }

            // Handle image upload
            if (image != null)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(existingEmployeeCL.Img))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingEmployeeCL.Img.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_imageBasePath, randomFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                employeeCL.Img = $"/Img/EmployeeCL/{randomFileName}";
                _logger.LogInformation("Image uploaded successfully: {FilePath}", employeeCL.Img);
            }
            else
            {
                // Keep existing image path if no new image is provided
                employeeCL.Img = existingEmployeeCL.Img;
            }

            // Update Employee_CL properties
            _context.Entry(existingEmployeeCL).CurrentValues.SetValues(employeeCL);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Success = true, Data = employeeCL, Message = "Cập nhật thông tin hợp đồng lao động thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employee_CLs.AnyAsync(e => e.Cid == cid))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy thông tin hợp đồng lao động" });
            }
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating EmployeeCL: {Cid}", employeeCL.Cid);
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{cid}")]
    [SessionAuthorize(RequiredRole = new[] { "HR" })]
    public async Task<IActionResult> Delete(int cid)
    {
        var employeeCL = await _context.Employee_CLs.FindAsync(cid);
        if (employeeCL == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin hợp đồng lao động" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Delete associated image file
            if (!string.IsNullOrEmpty(employeeCL.Img))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employeeCL.Img.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Employee_CLs.Remove(employeeCL);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Success = true, Message = "Xóa thông tin hợp đồng lao động thành công" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }

    [HttpGet("image/{cid}")]
    public async Task<IActionResult> GetEmployeeCLImage(int cid)
    {
        var employeeCL = await _context.Employee_CLs
            .FirstOrDefaultAsync(e => e.Cid == cid);

        if (employeeCL == null || string.IsNullOrEmpty(employeeCL.Img))
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy ảnh hợp đồng lao động" });
        }

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employeeCL.Img.TrimStart('/'));
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
}