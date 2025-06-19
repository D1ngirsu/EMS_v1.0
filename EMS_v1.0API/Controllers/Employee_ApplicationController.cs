using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Employee_Application Controller
[ApiController]
[Route("api/employee-application")]
[SessionAuthorize]
public class EmployeeApplicationController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeApplicationController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? eid, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_Applications
            .Include(e => e.Employee)
            .AsQueryable();

        if (eid.HasValue)
        {
            query = query.Where(e => e.Eid == eid.Value);
        }

        var totalCount = await query.CountAsync();
        var applications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = applications,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("{appId}")]
    public async Task<IActionResult> GetById(int appId)
    {
        var application = await _context.Employee_Applications
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.AppId == appId);

        if (application == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy đơn từ" });
        }

        return Ok(new { Success = true, Data = application });
    }

    [HttpGet("employee/{eid}")]
    public async Task<IActionResult> GetByEmployeeId(int eid, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_Applications
            .Where(e => e.Eid == eid)
            .Include(e => e.Employee)
            .OrderByDescending(e => e.Date)
            .AsQueryable();
        var totalCount = await query.CountAsync();
        var applications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new
        {
            Success = true,
            Data = applications,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpPost]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> Create([FromBody] Employee_Application application)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            _context.Employee_Applications.Add(application);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { appId = application.AppId },
                new { Success = true, Data = application, Message = "Tạo đơn từ thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{appId}")]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> Update(int appId, [FromBody] Employee_Application application)
    {
        if (appId != application.AppId)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        _context.Entry(application).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = application, Message = "Cập nhật đơn từ thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employee_Applications.AnyAsync(e => e.AppId == appId))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy đơn từ" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{appId}")]
    [SessionAuthorize(RequiredRole = new[] { "Admin", "HR" })]
    public async Task<IActionResult> Delete(int appId)
    {
        var application = await _context.Employee_Applications.FindAsync(appId);
        if (application == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy đơn từ" });
        }

        try
        {
            _context.Employee_Applications.Remove(application);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Xóa đơn từ thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}