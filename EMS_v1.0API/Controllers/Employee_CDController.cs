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
    public async Task<IActionResult> GetAll()
    {
        var employeeCDs = await _context.Employee_CDs
            .Include(e => e.Employee)
            .ToListAsync();

        return Ok(new { Success = true, Data = employeeCDs });
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

        return Ok(new { Success = true, Data = employeeCD });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Employee_CD employeeCD)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            _context.Employee_CDs.Add(employeeCD);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEid), new { uid = employeeCD.Eid },
                new { Success = true, Data = employeeCD, Message = "Tạo thông tin CCCD thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{eid}")]
    public async Task<IActionResult> Update(int eid, [FromBody] Employee_CD employeeCD)
    {
        if (eid != employeeCD.Eid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        _context.Entry(employeeCD).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = employeeCD, Message = "Cập nhật thông tin CCCD thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employee_CDs.AnyAsync(e => e.Eid == eid))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy thông tin CCCD" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{eid}")]
    [SessionAuthorize(RequiredRole = "Admin")]
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