using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

// Employee_Insurance Controller
[ApiController]
[Route("api/employee-insurance")]
[SessionAuthorize]
public class EmployeeInsuranceController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeInsuranceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetInsurances([FromQuery] string? name, [FromQuery] int? eid, [FromQuery] int? iid)
    {
        var query = _context.Employee_Insurances
            .Include(e => e.Employee)
            .AsQueryable();

        if (iid.HasValue)
        {
            query = query.Where(e => e.Iid == iid.Value);
        }
        else if (eid.HasValue)
        {
            query = query.Where(e => e.Eid == eid.Value);
        }
        else if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(e => e.Employee != null && e.Employee.Name.Contains(name));
        }

        var employeeInsurances = await query.ToListAsync();

        if (employeeInsurances == null || !employeeInsurances.Any())
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin bảo hiểm" });
        }

        return Ok(new { Success = true, Data = employeeInsurances });
    }

    [HttpGet("{iid}")]
    public async Task<IActionResult> GetByIid(int iid)
    {
        var employeeInsurance = await _context.Employee_Insurances
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.Iid == iid);

        if (employeeInsurance == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin bảo hiểm" });
        }

        return Ok(new { Success = true, Data = employeeInsurance });
    }

    [HttpGet("eid/{eid}")]
    public async Task<IActionResult> GetByEid(int eid)
    {
        var employeeInsurances = await _context.Employee_Insurances
            .Include(e => e.Employee)
            .Where(e => e.Eid == eid)
            .ToListAsync();

        if (employeeInsurances == null || !employeeInsurances.Any())
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin bảo hiểm cho nhân viên này" });
        }

        return Ok(new { Success = true, Data = employeeInsurances });
    }

    [HttpPost]
    [SessionAuthorize(RequiredRole = new[] { "InsuranceOfficer" })]
    public async Task<IActionResult> Create([FromBody] Employee_Insurance employeeInsurance)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            _context.Employee_Insurances.Add(employeeInsurance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByIid), new { iid = employeeInsurance.Iid },
                new { Success = true, Data = employeeInsurance, Message = "Tạo thông tin bảo hiểm thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{iid}")]
    [SessionAuthorize(RequiredRole = new[] { "InsuranceOfficer" })]
    public async Task<IActionResult> Update(int iid, [FromBody] Employee_Insurance employeeInsurance)
    {
        if (iid != employeeInsurance.Iid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        _context.Entry(employeeInsurance).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = employeeInsurance, Message = "Cập nhật thông tin bảo hiểm thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employee_Insurances.AnyAsync(e => e.Iid == iid))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy thông tin bảo hiểm" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{iid}")]
    [SessionAuthorize(RequiredRole = new[] { "InsuranceOfficer" })]
    public async Task<IActionResult> Delete(int iid)
    {
        var employeeInsurance = await _context.Employee_Insurances.FindAsync(iid);
        if (employeeInsurance == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin bảo hiểm" });
        }

        try
        {
            _context.Employee_Insurances.Remove(employeeInsurance);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Xóa thông tin bảo hiểm thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}