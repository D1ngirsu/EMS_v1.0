using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Employee_Relatives Controller
[ApiController]
[Route("api/employee-relatives")]
[SessionAuthorize]
public class EmployeeRelativesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeRelativesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? eid)
    {
        var query = _context.Employee_Relatives
            .Include(e => e.Employee)
            .AsQueryable();

        if (eid.HasValue)
        {
            query = query.Where(e => e.Eid == eid.Value);
        }

        var relatives = await query.ToListAsync();
        return Ok(new { Success = true, Data = relatives });
    }

    [HttpGet("{relId}")]
    public async Task<IActionResult> GetById(int relId)
    {
        var relative = await _context.Employee_Relatives
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.RelId == relId);

        if (relative == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin người thân" });
        }

        return Ok(new { Success = true, Data = relative });
    }

    [HttpGet("by-employee/{eid}")]
    public async Task<IActionResult> GetByEmployeeId(int eid)
    {
        var relatives = await _context.Employee_Relatives
            .Where(e => e.Eid == eid)
            .Include(e => e.Employee)
            .ToListAsync();
        if (relatives == null || !relatives.Any())
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin người thân cho nhân viên này" });
        }
        return Ok(new { Success = true, Data = relatives });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Employee_Relatives relative)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            _context.Employee_Relatives.Add(relative);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { relId = relative.RelId },
                new { Success = true, Data = relative, Message = "Tạo thông tin người thân thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{relId}")]
    public async Task<IActionResult> Update(int relId, [FromBody] Employee_Relatives relative)
    {
        if (relId != relative.RelId)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        _context.Entry(relative).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = relative, Message = "Cập nhật thông tin người thân thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employee_Relatives.AnyAsync(e => e.RelId == relId))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy thông tin người thân" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{relId}")]
    [SessionAuthorize(RequiredRole = new[] { "HR" } )]
    public async Task<IActionResult> Delete(int relId)
    {
        var relative = await _context.Employee_Relatives.FindAsync(relId);
        if (relative == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin người thân" });
        }

        try
        {
            _context.Employee_Relatives.Remove(relative);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Xóa thông tin người thân thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}