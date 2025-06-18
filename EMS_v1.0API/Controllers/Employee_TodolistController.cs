// Employee_Todolist Controller
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/employee-todolist")]
[SessionAuthorize]
public class EmployeeTodolistController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeTodolistController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? uid, [FromQuery] byte? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employee_Todolists
            .Include(e => e.Employee)
            .AsQueryable();

        if (uid.HasValue)
        {
            query = query.Where(e => e.Eid == uid.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var todos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(e => e.DayIssue)
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = todos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("{tid}")]
    public async Task<IActionResult> GetById(int tid)
    {
        var todo = await _context.Employee_Todolists
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.Tid == tid);

        if (todo == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy công việc" });
        }

        return Ok(new { Success = true, Data = todo });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Employee_Todolist todo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        try
        {
            _context.Employee_Todolists.Add(todo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { tid = todo.Tid },
                new { Success = true, Data = todo, Message = "Tạo công việc thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo: " + ex.Message });
        }
    }

    [HttpPut("{tid}")]
    public async Task<IActionResult> Update(int tid, [FromBody] Employee_Todolist todo)
    {
        if (tid != todo.Tid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        _context.Entry(todo).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = todo, Message = "Cập nhật công việc thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employee_Todolists.AnyAsync(e => e.Tid == tid))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy công việc" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{tid}")]
    [SessionAuthorize(RequiredRole = "Admin")]
    public async Task<IActionResult> Delete(int tid)
    {
        var todo = await _context.Employee_Todolists.FindAsync(tid);
        if (todo == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy công việc" });
        }

        try
        {
            _context.Employee_Todolists.Remove(todo);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Xóa công việc thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}