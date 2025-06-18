using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class OrganizationUnit
{
    [Key]
    public int UnitId { get; set; }

    [Required]
    [StringLength(100)]
    public string UnitName { get; set; }

    [Required]
    [StringLength(20)]
    public string UnitType { get; set; } // PhongBan, Nhom

    public int? ParentId { get; set; }

    // Navigation properties
    [ForeignKey("ParentId")]
    public OrganizationUnit Parent { get; set; }
    public ICollection<OrganizationUnit> Children { get; set; }
    public ICollection<Employee> Employees { get; set; }
}