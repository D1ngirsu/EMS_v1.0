using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/employee-as")]
[SessionAuthorize]
public class EmployeeASController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _imageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Img", "Employee_AS");
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public EmployeeASController(AppDbContext context)
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
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_ASs
            .Include(e => e.Employee)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var employeeASs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = employeeASs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("{eid}")]
    public async Task<IActionResult> GetByEid(int eid)
    {
        var employeeAS = await _context.Employee_ASs
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.Eid == eid);

        if (employeeAS == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin học hàm học vị" });
        }

        return Ok(new { Success = true, Data = employeeAS });
    }

    [HttpGet("image1/{eid}")]
    public async Task<IActionResult> GetDegreeImage1(int eid)
    {
        var employeeAS = await _context.Employee_ASs
            .FirstOrDefaultAsync(e => e.Eid == eid);

        if (employeeAS == null || string.IsNullOrEmpty(employeeAS.DegreeImg1))
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy ảnh DegreeImg1" });
        }

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employeeAS.DegreeImg1.TrimStart('/'));
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

    [HttpGet("image2/{eid}")]
    public async Task<IActionResult> GetDegreeImage2(int eid)
    {
        var employeeAS = await _context.Employee_ASs
            .FirstOrDefaultAsync(e => e.Eid == eid);

        if (employeeAS == null || string.IsNullOrEmpty(employeeAS.DegreeImg2))
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy ảnh DegreeImg2" });
        }

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employeeAS.DegreeImg2.TrimStart('/'));
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

    [HttpPost]
    [SessionAuthorize(RequiredRole = new[] { "HR" })]
    public async Task<IActionResult> Create([FromForm] Employee_AS employeeAS, IFormFile? image1, IFormFile? image2)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        // Validate images
        if (image1 != null && !IsValidImage(image1))
        {
            return BadRequest(new { Success = false, Message = "Ảnh DegreeImg1 không hợp lệ. Chỉ chấp nhận định dạng .jpg, .jpeg, .png và kích thước tối đa 5MB." });
        }
        if (image2 != null && !IsValidImage(image2))
        {
            return BadRequest(new { Success = false, Message = "Ảnh DegreeImg2 không hợp lệ. Chỉ chấp nhận định dạng .jpg, .jpeg, .png và kích thước tối đa 5MB." });
        }

        try
        {
            // Handle image1 upload
            if (image1 != null)
            {
                var fileExtension = Path.GetExtension(image1.FileName);
                var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_imageBasePath, randomFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image1.CopyToAsync(stream);
                }
                employeeAS.DegreeImg1 = $"/Img/Employee_AS/{randomFileName}";
            }

            // Handle image2 upload
            if (image2 != null)
            {
                var fileExtension = Path.GetExtension(image2.FileName);
                var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_imageBasePath, randomFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image2.CopyToAsync(stream);
                }
                employeeAS.DegreeImg2 = $"/Img/Employee_AS/{randomFileName}";
            }

            _context.Employee_ASs.Add(employeeAS);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEid), new { eid = employeeAS.Eid },
                new { Success = true, Data = employeeAS, Message = "Tạo thông tin học hàm học vị thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{eid}")]
    [SessionAuthorize(RequiredRole = new[] { "HR" })]
    public async Task<IActionResult> Update(int eid, [FromForm] Employee_AS employeeAS, IFormFile? image1, IFormFile? image2)
    {
        if (eid != employeeAS.Eid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        // Validate images
        if (image1 != null && !IsValidImage(image1))
        {
            return BadRequest(new { Success = false, Message = "Ảnh DegreeImg1 không hợp lệ. Chỉ chấp nhận định dạng .jpg, .jpeg, .png và kích thước tối đa 5MB." });
        }
        if (image2 != null && !IsValidImage(image2))
        {
            return BadRequest(new { Success = false, Message = "Ảnh DegreeImg2 không hợp lệ. Chỉ chấp nhận định dạng .jpg, .jpeg, .png và kích thước tối đa 5MB." });
        }

        // Handle image1 upload
        if (image1 != null)
        {
            var fileExtension = Path.GetExtension(image1.FileName);
            var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_imageBasePath, randomFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image1.CopyToAsync(stream);
            }
            employeeAS.DegreeImg1 = $"/Img/Employee_AS/{randomFileName}";
        }

        // Handle image2 upload
        if (image2 != null)
        {
            var fileExtension = Path.GetExtension(image2.FileName);
            var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_imageBasePath, randomFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image2.CopyToAsync(stream);
            }
            employeeAS.DegreeImg2 = $"/Img/Employee_AS/{randomFileName}";
        }

        _context.Entry(employeeAS).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = employeeAS, Message = "Cập nhật thông tin học hàm học vị thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employee_ASs.AnyAsync(e => e.Eid == eid))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy thông tin học hàm học vị" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{eid}")]
    [SessionAuthorize(RequiredRole = new[] { "HR" })]
    public async Task<IActionResult> Delete(int eid)
    {
        var employeeAS = await _context.Employee_ASs.FindAsync(eid);
        if (employeeAS == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin học hàm học vị" });
        }

        try
        {
            // Delete associated image files
            if (!string.IsNullOrEmpty(employeeAS.DegreeImg1))
            {
                var imagePath1 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employeeAS.DegreeImg1.TrimStart('/'));
                if (System.IO.File.Exists(imagePath1))
                {
                    System.IO.File.Delete(imagePath1);
                }
            }
            if (!string.IsNullOrEmpty(employeeAS.DegreeImg2))
            {
                var imagePath2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employeeAS.DegreeImg2.TrimStart('/'));
                if (System.IO.File.Exists(imagePath2))
                {
                    System.IO.File.Delete(imagePath2);
                }
            }

            _context.Employee_ASs.Remove(employeeAS);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Xóa thông tin học hàm học vị thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}