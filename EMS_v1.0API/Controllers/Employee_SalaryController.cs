// Employee_Salary Controller
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

[ApiController]
[Route("api/employee-salary")]
[SessionAuthorize]
public class EmployeeSalaryController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeSalaryController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [SessionAuthorize(RequiredRole = new[] { "PayrollOfficer" })]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchName = null,
        [FromQuery] string? unitName = null,
        [FromQuery] string? sortOrder = null) // "asc", "desc", "no-salary"
    {
        var query = from e in _context.Employees
                    join es in _context.Employee_Salaries
                        on e.Eid equals es.Eid into salaryGroup
                    from es in salaryGroup.DefaultIfEmpty()
                    join p in _context.Positions
                        on e.PositionId equals p.PositionId
                    join ou in _context.OrganizationUnits
                        on e.UnitId equals ou.UnitId
                    join parentUnit in _context.OrganizationUnits
                        on ou.ParentId equals parentUnit.UnitId into parentGroup
                    from parentUnit in parentGroup.DefaultIfEmpty()
                    select new
                    {
                        EmployeeId = e.Eid,
                        EmployeeName = e.Name,
                        PositionName = p.PositionName,
                        // Nếu là nhóm (có ParentId), hiển thị phòng ban cha, nếu là phòng ban thì hiển thị chính nó
                        UnitName = ou.ParentId != null ? parentUnit.UnitName : ou.UnitName,
                        Salary = es != null ? es.Salary : (decimal?)null
                    };

        // Tìm kiếm theo tên nhân viên
        if (!string.IsNullOrEmpty(searchName))
        {
            query = query.Where(e => e.EmployeeName.Contains(searchName));
        }

        // Tìm kiếm theo phòng ban
        if (unitName != null)
        {
            query = query.Where(e => e.UnitName == unitName);
        }

        // Sắp xếp
        switch (sortOrder?.ToLower())
        {
            case "asc":
                query = query.OrderBy(e => e.Salary ?? decimal.MaxValue);
                break;
            case "desc":
                query = query.OrderByDescending(e => e.Salary ?? decimal.MinValue);
                break;
            case "no-salary":
                query = query.OrderBy(e => e.Salary == null ? 0 : 1).ThenBy(e => e.EmployeeName);
                break;
            default:
                query = query.OrderBy(e => e.EmployeeName);
                break;
        }

        var totalCount = await query.CountAsync();
        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

    // Thêm method để lấy danh sách phòng ban cho filter
    [HttpGet("departments")]
    [SessionAuthorize(RequiredRole = new[] { "PayrollOfficer" })]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _context.OrganizationUnits
            .Where(ou => ou.UnitType == "PhongBan") // Chỉ lấy phòng ban
            .Select(ou => ou.UnitName)
            .OrderBy(name => name)
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = departments
        });
    }

    [HttpGet("{eid}")]
    [SessionAuthorize(RequiredRole = new[] { "PayrollOfficer" })]
    public async Task<IActionResult> GetByEid(int eid)
    {
        // Check if user can access this salary info
        var userIdStr = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userRole != "Admin" && userIdStr != eid.ToString())
        {
            return Forbid();
        }

        var salary = await _context.Employee_Salaries
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.Eid == eid);

        if (salary == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin lương" });
        }

        return Ok(new { Success = true, Data = salary });
    }

    [HttpPost]
    [SessionAuthorize(RequiredRole = new[] { "PayrollOfficer" })]
    public async Task<IActionResult> Create([FromBody] Employee_Salary salary)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            _context.Employee_Salaries.Add(salary);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEid), new { eid = salary.Eid },
                new { Success = true, Data = salary, Message = "Tạo thông tin lương thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{eid}")]
    [SessionAuthorize(RequiredRole = new[] { "PayrollOfficer" })]
    public async Task<IActionResult> Update(int eid, [FromBody] Employee_Salary salary)
    {
        if (eid != salary.Eid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        _context.Entry(salary).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = salary, Message = "Cập nhật thông tin lương thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employee_Salaries.AnyAsync(e => e.Eid == eid))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy thông tin lương" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }
}