using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net; 

[ApiController]
[Route("api/[controller]")]
[SessionAuthorize]
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    //[SessionAuthorize(RequiredRole = "Admin")]
    public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Khởi tạo các collection nếu null
            employee.Applications ??= new List<Employee_Application>();
            employee.Relatives ??= new List<Employee_Relatives>();
            employee.TodoList ??= new List<Employee_Todolist>();

            // Thêm Employee vào cơ sở dữ liệu
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Commit giao dịch
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetEmployee), new { eid = employee.Eid },
                new { Success = true, Data = employee, Message = "Tạo nhân viên thành công" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { Success = false, Message = "Lỗi khi tạo nhân viên: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Employees
            .Include(e => e.OrganizationUnit)
                .ThenInclude(o => o.Parent) // Bao gồm phòng ban cha
            .Include(e => e.Position)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeDto
            {
                Eid = e.Eid,
                Name = e.Name,
                DoB = e.DoB,
                Email = e.Email,
                Phone = e.Phone,
                Address = e.Address,
                Gender = e.Gender,
                Experience = e.Experience,
                Source = e.Source,
                BankNumber = e.BankNumber,
                Bank = e.Bank,
                Img = e.Img,
                OrganizationUnit = new OrganizationUnitDto
                {
                    UnitId = e.OrganizationUnit.UnitId,
                    UnitName = e.OrganizationUnit.UnitName,
                    UnitType = e.OrganizationUnit.UnitType
                },
                ParentOrganizationUnit = e.OrganizationUnit.Parent != null ? new OrganizationUnitDto
                {
                    UnitId = e.OrganizationUnit.Parent.UnitId,
                    UnitName = e.OrganizationUnit.Parent.UnitName,
                    UnitType = e.OrganizationUnit.Parent.UnitType
                } : null,
                Position = new PositionDto
                {
                    PositionId = e.Position.PositionId,
                    PositionName = e.Position.PositionName
                }
            })
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = employees,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("{eid}")]
    public async Task<IActionResult> GetEmployee(int eid)
    {
        var employee = await _context.Employees
            .Include(e => e.OrganizationUnit)
                .ThenInclude(o => o.Parent) // Bao gồm phòng ban cha
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Eid == eid);

        if (employee == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy nhân viên" });
        }

        var employeeDto = new EmployeeDto
        {
            Eid = employee.Eid,
            Name = employee.Name,
            DoB = employee.DoB,
            Email = employee.Email,
            Phone = employee.Phone,
            Address = employee.Address,
            Gender = employee.Gender,
            Experience = employee.Experience,
            Source = employee.Source,
            BankNumber = employee.BankNumber,
            Bank = employee.Bank,
            Img = employee.Img,
            OrganizationUnit = employee.OrganizationUnit != null ? new OrganizationUnitDto
            {
                UnitId = employee.OrganizationUnit.UnitId,
                UnitName = employee.OrganizationUnit.UnitName,
                UnitType = employee.OrganizationUnit.UnitType
            } : null,
            ParentOrganizationUnit = employee.OrganizationUnit?.Parent != null ? new OrganizationUnitDto
            {
                UnitId = employee.OrganizationUnit.Parent.UnitId,
                UnitName = employee.OrganizationUnit.Parent.UnitName,
                UnitType = employee.OrganizationUnit.Parent.UnitType
            } : null,
            Position = employee.Position != null ? new PositionDto
            {
                PositionId = employee.Position.PositionId,
                PositionName = employee.Position.PositionName
            } : null
        };

        return Ok(new { Success = true, Data = employeeDto });
    }

    [HttpPut("{eid}")]
    [SessionAuthorize(RequiredRole = "Admin")]
    public async Task<IActionResult> UpdateEmployee(int eid, [FromBody] Employee employee)
    {
        if (eid != employee.Eid)
        {
            return BadRequest(new { Success = false, Message = "ID không khớp" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ", Errors = ModelState });
        }

        _context.Entry(employee).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Data = employee, Message = "Cập nhật nhân viên thành công" });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employees.AnyAsync(e => e.Eid == eid))
            {
                return NotFound(new { Success = false, Message = "Không tìm thấy nhân viên" });
            }
            throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = "Lỗi khi cập nhật: " + ex.Message });
        }
    }

    [HttpDelete("{eid}")]
    //[SessionAuthorize(RequiredRole = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int eid)
    {
        var employee = await _context.Employees.FindAsync(eid);
        if (employee == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy nhân viên" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Success = true, Message = "Xóa nhân viên thành công" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { Success = false, Message = "Lỗi khi xóa: " + ex.Message });
        }
    }
}