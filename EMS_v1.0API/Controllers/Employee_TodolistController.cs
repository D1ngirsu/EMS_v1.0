using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

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
    public async Task<IActionResult> GetAll([FromQuery] int? eid, [FromQuery] byte? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // Validate pagination parameters
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { Success = false, Message = "Tham số phân trang không hợp lệ" });
        }

        try
        {
            // Get current user's employee info
            var currentUserEid = int.Parse(User.FindFirst("Eid")?.Value ?? "0");
            var currentEmployee = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.OrganizationUnit)
                .FirstOrDefaultAsync(e => e.Eid == currentUserEid);

            if (currentEmployee == null)
            {
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin nhân viên" });
            }

            var query = _context.Employee_Todolists
                .Include(e => e.Employee)
                .AsQueryable();

            // Apply role-based access control
            if (currentEmployee.Position.PositionName == "Giám Đốc" || User.IsInRole("Admin"))
            {
                // Giám đốc or Admin: Can view todos of Trưởng phòng (PositionId = 2)
                query = query.Where(t => t.Employee.PositionId == 2);
            }
            else if (currentEmployee.Position.PositionName == "Trưởng phòng")
            {
                // Trưởng phòng: Can view todos of Trưởng nhóm (PositionId = 3) in their department
                var departmentId = currentEmployee.UnitId;
                var groupUnitIds = await _context.OrganizationUnits
                    .Where(u => u.ParentId == departmentId && u.UnitType == "Nhom")
                    .Select(u => u.UnitId)
                    .ToListAsync();

                query = query.Where(t => t.Employee.PositionId == 3 && groupUnitIds.Contains(t.Employee.UnitId));
            }
            else if (currentEmployee.Position.PositionName == "Trưởng nhóm")
            {
                // Trưởng nhóm: Can view todos of members in their group
                var groupId = currentEmployee.UnitId;
                query = query.Where(t => t.Employee.UnitId == groupId);
            }
            else
            {
                // Other roles: Can only view their own todos
                query = query.Where(t => t.Eid == currentUserEid);
            }

            // Apply additional filters
            if (eid.HasValue)
            {
                query = query.Where(e => e.Eid == eid.Value);
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
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
        }
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

        // Authorization check for GetById
        var currentUserEid = int.Parse(User.FindFirst("Eid")?.Value ?? "0");
        var currentEmployee = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.OrganizationUnit)
            .FirstOrDefaultAsync(e => e.Eid == currentUserEid);

        if (currentEmployee == null)
        {
            return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin nhân viên" });
        }

        bool canAccess = false;
        if (currentEmployee.Position.PositionName == "Giám Đốc" || User.IsInRole("Admin"))
        {
            canAccess = todo.Employee.PositionId == 2; // Trưởng phòng
        }
        else if (currentEmployee.Position.PositionName == "Trưởng phòng")
        {
            var departmentId = currentEmployee.UnitId;
            var groupUnitIds = await _context.OrganizationUnits
                .Where(u => u.ParentId == departmentId && u.UnitType == "Nhom")
                .Select(u => u.UnitId)
                .ToListAsync();
            canAccess = todo.Employee.PositionId == 3 && groupUnitIds.Contains(todo.Employee.UnitId);
        }
        else if (currentEmployee.Position.PositionName == "Trưởng nhóm")
        {
            canAccess = todo.Employee.UnitId == currentEmployee.UnitId;
        }
        else
        {
            canAccess = todo.Eid == currentUserEid;
        }

        if (!canAccess)
        {
            return StatusCode(403, new { Success = false, Message = "Không có quyền xem công việc này" });
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

        // Get current user's employee info
        var currentUserEid = int.Parse(User.FindFirst("Eid")?.Value ?? "0");
        var currentEmployee = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.OrganizationUnit)
            .FirstOrDefaultAsync(e => e.Eid == currentUserEid);

        if (currentEmployee == null)
        {
            return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin nhân viên" });
        }

        // Verify target employee
        var targetEmployee = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.OrganizationUnit)
            .FirstOrDefaultAsync(e => e.Eid == todo.Eid);

        if (targetEmployee == null)
        {
            return BadRequest(new { Success = false, Message = "Nhân viên được giao việc không tồn tại" });
        }

        // Authorization check for Create
        bool canCreate = false;
        string errorMessage = "Không có quyền giao công việc này";

        if (currentEmployee.Position.PositionName == "Giám Đốc" || User.IsInRole("Admin"))
        {
            // Giám đốc or Admin can create todos for Trưởng phòng
            canCreate = targetEmployee.PositionId == 2; // Trưởng phòng
            errorMessage = "Chỉ có thể giao việc cho Trưởng phòng";
        }
        else if (currentEmployee.Position.PositionName == "Trưởng phòng")
        {
            // Trưởng phòng can create todos for Trưởng nhóm in their department
            var departmentId = currentEmployee.UnitId;
            var groupUnitIds = await _context.OrganizationUnits
                .Where(u => u.ParentId == departmentId && u.UnitType == "Nhom")
                .Select(u => u.UnitId)
                .ToListAsync();
            canCreate = targetEmployee.PositionId == 3 && groupUnitIds.Contains(targetEmployee.UnitId);
            errorMessage = "Chỉ có thể giao việc cho Trưởng nhóm trong phòng ban của bạn";
        }
        else if (currentEmployee.Position.PositionName == "Trưởng nhóm")
        {
            // Trưởng nhóm can create todos for Nhân viên in their group
            canCreate = targetEmployee.PositionId == 4 && targetEmployee.UnitId == currentEmployee.UnitId;
            errorMessage = "Chỉ có thể giao việc cho Nhân viên trong nhóm của bạn";
        }

        if (!canCreate)
        {
            return StatusCode(403, new { Success = false, Message = "Không có quyền tạo công việc này" });
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

        // Authorization check for Update
        var currentUserEid = int.Parse(User.FindFirst("Eid")?.Value ?? "0");
        var currentEmployee = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.OrganizationUnit)
            .FirstOrDefaultAsync(e => e.Eid == currentUserEid);

        if (currentEmployee == null)
        {
            return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin nhân viên" });
        }

        var existingTodo = await _context.Employee_Todolists
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.Tid == tid);
        if (existingTodo == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy công việc" });
        }

        bool canAccess = false;
        if (currentEmployee.Position.PositionName == "Giám Đốc" || User.IsInRole("Admin"))
        {
            canAccess = existingTodo.Employee.PositionId == 2; // Trưởng phòng
        }
        else
        {
            canAccess = existingTodo.Eid == currentUserEid; // Only allow updating own todos
        }

        if (!canAccess)
        {
            return StatusCode(403, new { Success = false, Message = "Không có quyền cập nhật công việc này" });
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