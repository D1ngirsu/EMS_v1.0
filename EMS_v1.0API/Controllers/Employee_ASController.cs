using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Employee_AS Controller
[ApiController]
[Route("api/employee-as")]
[SessionAuthorize]
public class EmployeeASController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeASController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employeeASs = await _context.Employee_ASs
            .Include(e => e.Employee)
            .ToListAsync();

        return Ok(new { Success = true, Data = employeeASs });
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

    [HttpPost]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> Create([FromBody] Employee_AS employeeAS)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            _context.Employee_ASs.Add(employeeAS);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEid), new { uid = employeeAS.Eid },
                new { Success = true, Data = employeeAS, Message = "Tạo thông tin học hàm học vị thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{eid}")]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> Update(int eid, [FromBody] Employee_AS employeeAS)
    {
        if (eid != employeeAS.Eid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
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
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> Delete(int eid)
    {
        var employeeAS = await _context.Employee_ASs.FindAsync(eid);
        if (employeeAS == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin học hàm học vị" });
        }

        try
        {
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