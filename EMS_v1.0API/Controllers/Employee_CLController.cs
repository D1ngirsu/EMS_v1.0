using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Employee_CL Controller
[ApiController]
[Route("api/employee-cl")]
[SessionAuthorize]
public class EmployeeCLController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeCLController(AppDbContext context)
    {
        _context = context;
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
    public async Task<IActionResult> Create([FromBody] Employee_CL employeeCL)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            _context.Employee_CLs.Add(employeeCL);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByCid), new { cid = employeeCL.Cid },
                new { Success = true, Data = employeeCL, Message = "Tạo thông tin hợp đồng lao động thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{cid}")]
    [SessionAuthorize(RequiredRole = new[] { "HR" })]
    public async Task<IActionResult> Update(int cid, [FromBody] Employee_CL employeeCL)
    {
        if (cid != employeeCL.Cid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        _context.Entry(employeeCL).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
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

        try
        {
            _context.Employee_CLs.Remove(employeeCL);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Xóa thông tin hợp đồng lao động thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}