using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
    public async Task<IActionResult> GetInsurances(
     [FromQuery] string? name = null,
     [FromQuery] string? unitName = null,
     [FromQuery] string? sortOrder = null, // "asc", "desc", "no-insurance"
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10)
    {
        var query = from e in _context.Employees
                    join ei in _context.Employee_Insurances
                        on e.Eid equals ei.Eid into insuranceGroup
                    from ei in insuranceGroup.DefaultIfEmpty()
                    join p in _context.Positions
                        on e.PositionId equals p.PositionId
                    join ou in _context.OrganizationUnits
                        on e.UnitId equals ou.UnitId
                    join parentUnit in _context.OrganizationUnits
                        on ou.ParentId equals parentUnit.UnitId into parentGroup
                    from parentUnit in parentGroup.DefaultIfEmpty()
                    select new
                    {
                        Iid = ei != null ? ei.Iid : (int?)null,
                        Eid = e.Eid,
                        EmployeeName = e.Name,
                        PositionName = p.PositionName,
                        UnitName = ou.ParentId != null ? parentUnit.UnitName : ou.UnitName,
                        InsuranceContent = ei != null ? ei.InsuranceContent : null,
                        FromDate = ei != null ? ei.FromDate : (DateTime?)null,
                        ToDate = ei != null ? ei.ToDate : (DateTime?)null,
                        ContributePercent = ei != null ? ei.ContributePercent : (decimal?)null
                    };

        // Tìm kiếm theo tên nhân viên
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(e => e.EmployeeName.Contains(name));
        }

        // Lọc theo phòng ban
        if (!string.IsNullOrEmpty(unitName))
        {
            query = query.Where(e => e.UnitName == unitName);
        }

        // Sắp xếp
        switch (sortOrder?.ToLower())
        {
            case "asc":
                query = query.OrderBy(e => e.ContributePercent ?? decimal.MaxValue);
                break;
            case "desc":
                query = query.OrderByDescending(e => e.ContributePercent ?? decimal.MinValue);
                break;
            case "no-insurance":
                query = query.OrderBy(e => e.ContributePercent == null ? 0 : 1).ThenBy(e => e.EmployeeName);
                break;
            default:
                query = query.OrderBy(e => e.EmployeeName);
                break;
        }

        var totalCount = await query.CountAsync();
        var employeeInsurances = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = employeeInsurances,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
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
    public async Task<IActionResult> GetByEid(int eid, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_Insurances
            .Include(e => e.Employee)
            .Where(e => e.Eid == eid)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var employeeInsurances = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (employeeInsurances == null || !employeeInsurances.Any())
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy thông tin bảo hiểm cho nhân viên này" });
        }

        return Ok(new
        {
            Success = true,
            Data = employeeInsurances,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
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