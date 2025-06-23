using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
                .ThenInclude(e => e.OrganizationUnit)
            .Include(u => u.Employee)
                .ThenInclude(e => e.Position)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Ok(new LoginResponse
            {
                Success = false,
                Message = "Tên đăng nhập hoặc mật khẩu không đúng"
            });
        }

        // Determine user role
        string userRole = "User";
        if (user.Employee?.Position?.PositionName == "Giám Đốc")
            userRole = "Admin";
        else if (user.Employee?.Position?.PositionName == "Trưởng Phòng")
            userRole = "Manager";
        else if (user.Employee?.Position?.PositionName == "Trưởng nhóm")
            userRole = "TeamLeader";
        else if (user.Employee?.Position?.PositionName == "Cán bộ nhân sự")
            userRole = "HR";
        else if (user.Employee?.Position?.PositionName == "Cán bộ lương")
            userRole = "PayrollOfficer";
        else if (user.Employee?.Position?.PositionName == "Cán bộ bảo hiểm")
            userRole = "InsuranceOfficer";
        else if (user.Employee?.Position?.PositionName == "Nhân viên")
            userRole = "Staff";

        // Store user info in session
        HttpContext.Session.SetString("UserId", user.Uid.ToString());
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("UserRole", userRole);
        HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString());

        // Create user DTO
        var userDto = new UserDto
        {
            Uid = user.Uid,
            Username = user.Username,
            Role = userRole,
            Employee = user.Employee != null ? new EmployeeDto
            {
                Eid = user.Employee.Eid,
                Name = user.Employee.Name,
                Email = user.Employee.Email,
                Phone = user.Employee.Phone,
                Gender = user.Employee.Gender,
                Img = user.Employee.Img,
                OrganizationUnit = user.Employee.OrganizationUnit != null ? new OrganizationUnitDto
                {
                    UnitId = user.Employee.OrganizationUnit.UnitId,
                    UnitName = user.Employee.OrganizationUnit.UnitName,
                    UnitType = user.Employee.OrganizationUnit.UnitType
                } : null,
                Position = user.Employee.Position != null ? new PositionDto
                {
                    PositionId = user.Employee.Position.PositionId,
                    PositionName = user.Employee.Position.PositionName
                } : null
            } : null
        };

        // Store complete user object
        var userJson = JsonSerializer.Serialize(userDto);
        HttpContext.Session.SetString("User", userJson);

        return Ok(new LoginResponse
        {
            Success = true,
            Message = "Đăng nhập thành công",
            Token = HttpContext.Session.Id,
            User = userDto
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { Success = true, Message = "Đăng xuất thành công" });
    }

    [HttpGet("current-user")]
    public IActionResult GetCurrentUser()
    {
        var userJson = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(userJson))
        {
            return Unauthorized(new { Success = false, Message = "Chưa xác thực" });
        }

        var user = JsonSerializer.Deserialize<UserDto>(userJson);

        // Lấy thông tin nhân viên đầy đủ
        var employee = _context.Employees
            .Include(e => e.OrganizationUnit)
            .Include(e => e.Position)
            .FirstOrDefault(e => e.Eid == user.Employee.Eid);

        if (employee != null)
        {
            user.Employee = new EmployeeDto
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
                Position = employee.Position != null ? new PositionDto
                {
                    PositionId = employee.Position.PositionId,
                    PositionName = employee.Position.PositionName
                } : null
            };
        }

        // Cập nhật session
        var updatedUserJson = JsonSerializer.Serialize(user);
        HttpContext.Session.SetString("User", updatedUserJson);

        return Ok(new { Success = true, User = user });
    }



    [HttpGet("organization-units")]
    public async Task<IActionResult> GetOrganizationUnits()
    {
        var units = await _context.OrganizationUnits
            .Select(u => new OrganizationUnitDto
            {
                UnitId = u.UnitId,
                UnitName = u.UnitName,
                UnitType = u.UnitType
            })
            .ToListAsync();

        return Ok(new { Success = true, Data = units });
    }

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions()
    {
        var positions = await _context.Positions
            .Select(p => new PositionDto
            {
                PositionId = p.PositionId,
                PositionName = p.PositionName
            })
            .ToListAsync();

        return Ok(new { Success = true, Data = positions });
    }

    [HttpPost("change-password")]
    [SessionAuthorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized(new { Success = false, Message = "Chưa xác thực" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { Success = false, Message = "Không tìm thấy người dùng" });
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
        {
            return Ok(new { Success = false, Message = "Mật khẩu hiện tại không đúng" });
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new { Success = true, Message = "Đổi mật khẩu thành công" });
    }
}