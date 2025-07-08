using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

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

        // Convert to DTOs to avoid circular references
        var employeeASDtos = employeeASs.Select(e => new EmployeeASDto
        {
            Eid = e.Eid,
            AcademicRank = e.AcademicRank,
            Degree = e.Degree,
            PlaceIssue = e.PlaceIssue,
            IssueDay = e.IssueDay,
            DegreeImg1 = e.DegreeImg1,
            DegreeImg2 = e.DegreeImg2
        }).ToList();

        return Ok(new
        {
            Success = true,
            Data = employeeASDtos, // Return DTOs directly, not serialized JSON
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

        // Convert to DTO to avoid circular references
        var employeeASDto = new EmployeeASDto
        {
            Eid = employeeAS.Eid,
            AcademicRank = employeeAS.AcademicRank,
            Degree = employeeAS.Degree,
            PlaceIssue = employeeAS.PlaceIssue,
            IssueDay = employeeAS.IssueDay,
            DegreeImg1 = employeeAS.DegreeImg1,
            DegreeImg2 = employeeAS.DegreeImg2
        };

        return Ok(new { Success = true, Data = employeeASDto }); // Return DTO directly
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
    public async Task<IActionResult> Create([FromForm] string employeeAS, IFormFile? image1, IFormFile? image2)
    {
        try
        {
            // Deserialize the EmployeeASDto from JSON string
            var employeeASDto = JsonSerializer.Deserialize<EmployeeASDto>(employeeAS, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (employeeASDto == null)
            {
                return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ" });
            }

            // Validate required fields manually since we're using DTO
            if (string.IsNullOrWhiteSpace(employeeASDto.AcademicRank))
            {
                return BadRequest(new { Success = false, Message = "Học hàm là bắt buộc" });
            }
            if (string.IsNullOrWhiteSpace(employeeASDto.Degree))
            {
                return BadRequest(new { Success = false, Message = "Bằng cấp là bắt buộc" });
            }
            if (string.IsNullOrWhiteSpace(employeeASDto.PlaceIssue))
            {
                return BadRequest(new { Success = false, Message = "Nơi cấp là bắt buộc" });
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

            // Convert DTO to Entity
            var employeeASEntity = new Employee_AS
            {
                Eid = employeeASDto.Eid,
                AcademicRank = employeeASDto.AcademicRank,
                Degree = employeeASDto.Degree,
                PlaceIssue = employeeASDto.PlaceIssue,
                IssueDay = employeeASDto.IssueDay
            };

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
                employeeASEntity.DegreeImg1 = $"/Img/Employee_AS/{randomFileName}";
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
                employeeASEntity.DegreeImg2 = $"/Img/Employee_AS/{randomFileName}";
            }

            _context.Employee_ASs.Add(employeeASEntity);
            await _context.SaveChangesAsync();

            // Convert back to DTO for response
            var responseDto = new EmployeeASDto
            {
                Eid = employeeASEntity.Eid,
                AcademicRank = employeeASEntity.AcademicRank,
                Degree = employeeASEntity.Degree,
                PlaceIssue = employeeASEntity.PlaceIssue,
                IssueDay = employeeASEntity.IssueDay,
                DegreeImg1 = employeeASEntity.DegreeImg1,
                DegreeImg2 = employeeASEntity.DegreeImg2
            };

            return CreatedAtAction(nameof(GetByEid), new { eid = employeeASEntity.Eid },
                new { Success = true, Data = responseDto, Message = "Tạo thông tin học hàm học vị thành công" });
        }
        catch (JsonException)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu JSON không hợp lệ" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{eid}")]
    [SessionAuthorize(RequiredRole = new[] { "HR" })]
    public async Task<IActionResult> Update(int eid, [FromForm] string employeeAS, IFormFile? image1, IFormFile? image2)
    {
        try
        {
            // Deserialize the EmployeeASDto from JSON string
            var employeeASDto = JsonSerializer.Deserialize<EmployeeASDto>(employeeAS, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (employeeASDto == null)
            {
                return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ" });
            }

            if (eid != employeeASDto.Eid)
            {
                return BadRequest(new { Success = false, Message = "ID không khớp" });
            }

            // Validate required fields manually
            if (string.IsNullOrWhiteSpace(employeeASDto.AcademicRank))
            {
                return BadRequest(new { Success = false, Message = "Học hàm là bắt buộc" });
            }
            if (string.IsNullOrWhiteSpace(employeeASDto.Degree))
            {
                return BadRequest(new { Success = false, Message = "Bằng cấp là bắt buộc" });
            }
            if (string.IsNullOrWhiteSpace(employeeASDto.PlaceIssue))
            {
                return BadRequest(new { Success = false, Message = "Nơi cấp là bắt buộc" });
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

            // Get existing entity
            var existingEntity = await _context.Employee_ASs.FindAsync(eid);
            if (existingEntity == null)
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy thông tin học hàm học vị" });
            }

            // Update entity properties
            existingEntity.AcademicRank = employeeASDto.AcademicRank;
            existingEntity.Degree = employeeASDto.Degree;
            existingEntity.PlaceIssue = employeeASDto.PlaceIssue;
            existingEntity.IssueDay = employeeASDto.IssueDay;

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
                existingEntity.DegreeImg1 = $"/Img/Employee_AS/{randomFileName}";
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
                existingEntity.DegreeImg2 = $"/Img/Employee_AS/{randomFileName}";
            }

            await _context.SaveChangesAsync();

            // Convert back to DTO for response
            var responseDto = new EmployeeASDto
            {
                Eid = existingEntity.Eid,
                AcademicRank = existingEntity.AcademicRank,
                Degree = existingEntity.Degree,
                PlaceIssue = existingEntity.PlaceIssue,
                IssueDay = existingEntity.IssueDay,
                DegreeImg1 = existingEntity.DegreeImg1,
                DegreeImg2 = existingEntity.DegreeImg2
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Cập nhật thông tin học hàm học vị thành công" });
        }
        catch (JsonException)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu JSON không hợp lệ" });
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