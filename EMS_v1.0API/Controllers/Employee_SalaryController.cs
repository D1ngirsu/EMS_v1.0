// Employee_Salary Controller
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [SessionAuthorize(RequiredRole = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var salaries = await _context.Employee_Salaries
            .Include(e => e.Employee)
            .ToListAsync();

        return Ok(new { Success = true, Data = salaries });
    }

    [HttpGet("{eid}")]
    public async Task<IActionResult> GetByUid(int eid)
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
    [SessionAuthorize(RequiredRole = "Admin")]
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

            return CreatedAtAction(nameof(GetByUid), new { eid = salary.Eid },
                new { Success = true, Data = salary, Message = "Tạo thông tin lương thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{eid}")]
    [SessionAuthorize(RequiredRole = "Admin")]
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