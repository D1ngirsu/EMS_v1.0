using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/employee-cd")]
[SessionAuthorize]
public class EmployeeCDController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeCDController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_CDs
            .Include(e => e.Employee)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var employeeCDs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeCDDto
            {
                Eid = e.Eid,
                IdNumber = e.IdNumber,
                IssueDay = e.IssueDay,
                IssuePlace = e.IssuePlace,
                Country = e.Country,
                Employee = e.Employee != null ? new EmployeeDto
                {
                    Eid = e.Employee.Eid,
                    Name = e.Employee.Name,
                    Email = e.Employee.Email,
                    Phone = e.Employee.Phone
                    // Chỉ include các field cần thiết, không include EmployeeCD
                } : null
            })
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = employeeCDs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("{eid}")]
    public async Task<IActionResult> GetByEid(int eid)
    {
        var employeeCD = await _context.Employee_CDs
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.Eid == eid);

        if (employeeCD == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin CCCD" });
        }

        var employeeCDDto = new EmployeeCDDto
        {
            Eid = employeeCD.Eid,
            IdNumber = employeeCD.IdNumber,
            IssueDay = employeeCD.IssueDay,
            IssuePlace = employeeCD.IssuePlace,
            Country = employeeCD.Country,
            Employee = employeeCD.Employee != null ? new EmployeeDto
            {
                Eid = employeeCD.Employee.Eid,
                Name = employeeCD.Employee.Name,
                Email = employeeCD.Employee.Email,
                Phone = employeeCD.Employee.Phone
            } : null
        };

        return Ok(new { Success = true, Data = employeeCDDto });
    }

    [HttpPost]
    [SessionAuthorize(RequiredRole = new[] { "HR" })]
    public async Task<IActionResult> Create([FromBody] EmployeeCDDto employeeCDDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            // Kiểm tra xem Employee có tồn tại không
            var employee = await _context.Employees.FindAsync(employeeCDDto.Eid);
            if (employee == null)
            {
                return BadRequest(new { Success = false, Message = "Không tìm thấy nhân viên với ID này" });
            }

            // Kiểm tra xem CCCD đã tồn tại cho nhân viên này chưa
            var existingCD = await _context.Employee_CDs.AnyAsync(e => e.Eid == employeeCDDto.Eid);
            if (existingCD)
            {
                return BadRequest(new { Success = false, Message = "Nhân viên này đã có thông tin CCCD" });
            }

            // Convert DTO to Entity
            var employeeCD = new Employee_CD
            {
                Eid = employeeCDDto.Eid,
                IdNumber = employeeCDDto.IdNumber,
                IssueDay = employeeCDDto.IssueDay,
                IssuePlace = employeeCDDto.IssuePlace,
                Country = employeeCDDto.Country
            };

            _context.Employee_CDs.Add(employeeCD);
            await _context.SaveChangesAsync();

            // Tạo DTO response để tránh circular reference
            var responseDto = new EmployeeCDDto
            {
                Eid = employeeCD.Eid,
                IdNumber = employeeCD.IdNumber,
                IssueDay = employeeCD.IssueDay,
                IssuePlace = employeeCD.IssuePlace,
                Country = employeeCD.Country,
                Employee = new EmployeeDto
                {
                    Eid = employee.Eid,
                    Name = employee.Name,
                    Email = employee.Email,
                    Phone = employee.Phone
                }
            };

            return CreatedAtAction(nameof(GetByEid), new { eid = employeeCD.Eid },
                new { Success = true, Data = responseDto, Message = "Tạo thông tin CCCD thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{eid}")]
    public async Task<IActionResult> Update(int eid, [FromBody] EmployeeCDDto employeeCDDto)
    {
        if (eid != employeeCDDto.Eid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            var existingCD = await _context.Employee_CDs
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(e => e.Eid == eid);

            if (existingCD == null)
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy thông tin CCCD" });
            }

            // Update properties
            existingCD.IdNumber = employeeCDDto.IdNumber;
            existingCD.IssueDay = employeeCDDto.IssueDay;
            existingCD.IssuePlace = employeeCDDto.IssuePlace;
            existingCD.Country = employeeCDDto.Country;

            await _context.SaveChangesAsync();

            // Tạo DTO response
            var responseDto = new EmployeeCDDto
            {
                Eid = existingCD.Eid,
                IdNumber = existingCD.IdNumber,
                IssueDay = existingCD.IssueDay,
                IssuePlace = existingCD.IssuePlace,
                Country = existingCD.Country,
                Employee = existingCD.Employee != null ? new EmployeeDto
                {
                    Eid = existingCD.Employee.Eid,
                    Name = existingCD.Employee.Name,
                    Email = existingCD.Employee.Email,
                    Phone = existingCD.Employee.Phone
                } : null
            };

            return Ok(new { Success = true, Data = responseDto, Message = "Cập nhật thông tin CCCD thành công" });
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
        var employeeCD = await _context.Employee_CDs.FindAsync(eid);
        if (employeeCD == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin CCCD" });
        }

        try
        {
            _context.Employee_CDs.Remove(employeeCD);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Xóa thông tin CCCD thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}

