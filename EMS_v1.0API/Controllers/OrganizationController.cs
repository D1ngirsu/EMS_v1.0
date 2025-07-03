using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrganizationController(AppDbContext context)
    {
        _context = context;
    }

    // Position Endpoints

    [HttpGet("positions")]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetPositions()
    {
        var positions = await _context.Positions
            .Select(p => new PositionDto
            {
                PositionId = p.PositionId,
                PositionName = p.PositionName
            })
            .ToListAsync();

        return Ok(positions);
    }

    [HttpGet("positions/{id}")]
    public async Task<ActionResult<PositionDto>> GetPosition(int id)
    {
        var position = await _context.Positions
            .Where(p => p.PositionId == id)
            .Select(p => new PositionDto
            {
                PositionId = p.PositionId,
                PositionName = p.PositionName
            })
            .FirstOrDefaultAsync();

        if (position == null)
        {
            return NotFound();
        }

        return Ok(position);
    }

    [HttpPost("positions")]
    public async Task<ActionResult<PositionDto>> CreatePosition(PositionDto positionDto)
    {
        var position = new Position
        {
            PositionName = positionDto.PositionName
        };

        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        positionDto.PositionId = position.PositionId;
        return CreatedAtAction(nameof(GetPosition), new { id = position.PositionId }, positionDto);
    }

    [HttpPut("positions/{id}")]
    public async Task<IActionResult> UpdatePosition(int id, PositionDto positionDto)
    {
        if (id != positionDto.PositionId)
        {
            return BadRequest();
        }

        var position = await _context.Positions.FindAsync(id);
        if (position == null)
        {
            return NotFound();
        }

        position.PositionName = positionDto.PositionName;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("positions/{id}")]
    public async Task<IActionResult> DeletePosition(int id)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null)
        {
            return NotFound();
        }

        _context.Positions.Remove(position);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // OrganizationUnit Endpoints

    [HttpGet("departments")]
    public async Task<ActionResult<IEnumerable<OrganizationUnitDto>>> GetDepartments()
    {
        var departments = await _context.OrganizationUnits
            .Where(o => o.UnitType == "PhongBan")
            .Select(o => new OrganizationUnitDto
            {
                UnitId = o.UnitId,
                UnitName = o.UnitName,
                UnitType = o.UnitType
            })
            .ToListAsync();

        return Ok(departments);
    }

    [HttpGet("departments/{departmentId}/groups")]
    public async Task<ActionResult<IEnumerable<OrganizationUnitDto>>> GetGroupsByDepartment(int departmentId)
    {
        var department = await _context.OrganizationUnits
            .Where(o => o.UnitId == departmentId && o.UnitType == "PhongBan")
            .FirstOrDefaultAsync();

        if (department == null)
        {
            return NotFound("Department not found");
        }

        var groups = await _context.OrganizationUnits
            .Where(o => o.ParentId == departmentId && o.UnitType == "Nhom")
            .Select(o => new OrganizationUnitDto
            {
                UnitId = o.UnitId,
                UnitName = o.UnitName,
                UnitType = o.UnitType
            })
            .ToListAsync();

        return Ok(groups);
    }

    [HttpGet("organization-units/{id}")]
    public async Task<ActionResult<OrganizationUnitDto>> GetOrganizationUnit(int id)
    {
        var unit = await _context.OrganizationUnits
            .Where(o => o.UnitId == id)
            .Select(o => new OrganizationUnitDto
            {
                UnitId = o.UnitId,
                UnitName = o.UnitName,
                UnitType = o.UnitType
            })
            .FirstOrDefaultAsync();

        if (unit == null)
        {
            return NotFound();
        }

        return Ok(unit);
    }

    [HttpPost("organization-units")]
    public async Task<ActionResult<OrganizationUnitDto>> CreateOrganizationUnit(OrganizationUnitDto unitDto)
    {
        var unit = new OrganizationUnit
        {
            UnitName = unitDto.UnitName,
            UnitType = unitDto.UnitType,
            ParentId = unitDto.UnitId > 0 ? unitDto.UnitId : null
        };

        _context.OrganizationUnits.Add(unit);
        await _context.SaveChangesAsync();

        unitDto.UnitId = unit.UnitId;
        return CreatedAtAction(nameof(GetOrganizationUnit), new { id = unit.UnitId }, unitDto);
    }

    [HttpPut("organization-units/{id}")]
    public async Task<IActionResult> UpdateOrganizationUnit(int id, OrganizationUnitDto unitDto)
    {
        if (id != unitDto.UnitId)
        {
            return BadRequest();
        }

        var unit = await _context.OrganizationUnits.FindAsync(id);
        if (unit == null)
        {
            return NotFound();
        }

        unit.UnitName = unitDto.UnitName;
        unit.UnitType = unitDto.UnitType;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("organization-units/{id}")]
    public async Task<IActionResult> DeleteOrganizationUnit(int id)
    {
        var unit = await _context.OrganizationUnits.FindAsync(id);
        if (unit == null)
        {
            return NotFound();
        }

        _context.OrganizationUnits.Remove(unit);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}