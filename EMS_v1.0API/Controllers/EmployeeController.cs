using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[SessionAuthorize]
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeeController> _logger;
    private readonly string _imageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Img", "Employee");
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public EmployeeController(AppDbContext context)
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

    [HttpPost]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> CreateEmployee([FromForm] Employee employee, IFormFile? image)
    {
        _logger.LogInformation("CreateEmployee called with employee: {EmployeeName}, Image provided: {ImageProvided}, Image Length: {ImageLength}, Img Path: {ImgPath}",
            employee.Name, image != null, image?.Length, employee.Img);

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
            // Handle image upload using provided Img path
            if (image != null && !string.IsNullOrEmpty(employee.Img))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employee.Img.TrimStart('/'));
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                _logger.LogInformation("Image uploaded successfully: {FilePath}", employee.Img);
            }
            else if (image != null && string.IsNullOrEmpty(employee.Img))
            {
                // Fallback in case Img path is not provided
                var fileExtension = Path.GetExtension(image.FileName);
                var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_imageBasePath, randomFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                employee.Img = $"/Img/Employee/{randomFileName}";
                _logger.LogInformation("Image uploaded with fallback path: {FilePath}", employee.Img);
            }

            // Initialize collections if null
            employee.Applications ??= new List<Employee_Application>();
            employee.Relatives ??= new List<Employee_Relatives>();
            employee.TodoList ??= new List<Employee_Todolist>();

            // Add Employee to database
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            var response = new { Success = true, Data = employee, Message = "Tạo nhân viên thành công" };
            _logger.LogInformation("Employee created successfully: {EmployeeId}, Response: {Response}",
                employee.Eid, JsonSerializer.Serialize(response));
            return CreatedAtAction(nameof(GetEmployee), new { eid = employee.Eid }, response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating employee: {EmployeeName}", employee.Name);
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo nhân viên: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] int? eid = null,
        [FromQuery] int? unitId = null)
    {
        var query = _context.Employees
            .Include(e => e.OrganizationUnit)
                .ThenInclude(o => o.Parent)
            .Include(e => e.Position)
            .AsQueryable();

        // Apply search filters
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(e => e.Name.Contains(name));
        }

        if (eid.HasValue)
        {
            query = query.Where(e => e.Eid == eid.Value);
        }

        if (unitId.HasValue)
        {
            query = query.Where(e => e.UnitId == unitId.Value);
        }

        var totalCount = await query.CountAsync();
        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeDto
            {
                Eid = e.Eid,
                Name = e.Name,
                DoB = e.DoB,
                Email = e.Email,
                Phone = e.Phone,
                Address = e.Address,
                Gender = e.Gender,
                Experience = e.Experience,
                Source = e.Source,
                BankNumber = e.BankNumber,
                Bank = e.Bank,
                Img = e.Img,
                OrganizationUnit = new OrganizationUnitDto
                {
                    UnitId = e.OrganizationUnit.UnitId,
                    UnitName = e.OrganizationUnit.UnitName,
                    UnitType = e.OrganizationUnit.UnitType
                },
                ParentOrganizationUnit = e.OrganizationUnit.Parent != null ? new OrganizationUnitDto
                {
                    UnitId = e.OrganizationUnit.Parent.UnitId,
                    UnitName = e.OrganizationUnit.Parent.UnitName,
                    UnitType = e.OrganizationUnit.Parent.UnitType
                } : null,
                Position = new PositionDto
                {
                    PositionId = e.Position.PositionId,
                    PositionName = e.Position.PositionName
                }
            })
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = employees,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("{eid}")]
    public async Task<IActionResult> GetEmployee(int eid)
    {
        var employee = await _context.Employees
            .Include(e => e.OrganizationUnit)
                .ThenInclude(o => o.Parent)
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Eid == eid);

        if (employee == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy nhân viên" });
        }

        var employeeDto = new EmployeeDto
        {
            Eid = employee.Eid,
            Name = employee.Name,
            DoB = employee.DoB,
            Email = employee.Email,
            Phone = employee.Phone,
            Address = employee.Address,
            Gender = employee.Gender,
            Experience = employee.Experience,
            Source = employee.Source,
            BankNumber = employee.BankNumber,
            Bank = employee.Bank,
            Img = employee.Img,
            OrganizationUnit = employee.OrganizationUnit != null ? new OrganizationUnitDto
            {
                UnitId = employee.OrganizationUnit.UnitId,
                UnitName = employee.OrganizationUnit.UnitName,
                UnitType = employee.OrganizationUnit.UnitType
            } : null,
            ParentOrganizationUnit = employee.OrganizationUnit?.Parent != null ? new OrganizationUnitDto
            {
                UnitId = employee.OrganizationUnit.Parent.UnitId,
                UnitName = employee.OrganizationUnit.Parent.UnitName,
                UnitType = employee.OrganizationUnit.Parent.UnitType
            } : null,
            Position = employee.Position != null ? new PositionDto
            {
                PositionId = employee.Position.PositionId,
                PositionName = employee.Position.PositionName
            } : null
        };

        return Ok(new { Success = true, Data = employeeDto });
    }

    [HttpGet("image/{eid}")]
    public async Task<IActionResult> GetEmployeeImage(int eid)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Eid == eid);

        if (employee == null || string.IsNullOrEmpty(employee.Img))
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy ảnh nhân viên" });
        }

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employee.Img.TrimStart('/'));
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

    [HttpPut("{eid}")]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> UpdateEmployee(int eid, [FromForm] Employee employee, IFormFile? image)
    {
        if (eid != employee.Eid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        // Loại bỏ yêu cầu bắt buộc cho Img trong ModelState
        ModelState.Remove("Img");

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        // Validate image
        if (image != null && !IsValidImage(image))
        {
            return BadRequest(new { Success = false, Message = "Ảnh không hợp lệ. Chỉ chấp nhận định dạng .jpg, .jpeg, .png và kích thước tối đa 5MB." });
        }

        // Handle image upload using provided Img path
        if (image != null && !string.IsNullOrEmpty(employee.Img))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employee.Img.TrimStart('/'));
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            _logger.LogInformation("Image uploaded successfully: {FilePath}", employee.Img);
        }
        else if (image != null && string.IsNullOrEmpty(employee.Img))
        {
            // Fallback in case Img path is not provided
            var fileExtension = Path.GetExtension(image.FileName);
            var randomFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_imageBasePath, randomFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            employee.Img = $"/Img/Employee/{randomFileName}";
            _logger.LogInformation("Image uploaded with fallback path: {FilePath}", employee.Img);
        }

        _context.Entry(employee).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = employee, Message = "Cập nhật nhân viên thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employees.AnyAsync(e => e.Eid == eid))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy nhân viên" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{eid}")]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> DeleteEmployee(int eid)
    {
        var employee = await _context.Employees.FindAsync(eid);
        if (employee == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy nhân viên" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Delete associated image file
            if (!string.IsNullOrEmpty(employee.Img))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", employee.Img.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Success = true, Message = "Xóa nhân viên thành công" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}