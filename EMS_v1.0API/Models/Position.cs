using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class Position
{
    [Key]
    public int PositionId { get; set; }

    [Required]
    [StringLength(100)]
    public string PositionName { get; set; }

    // Navigation property
    public ICollection<Employee> Employees { get; set; }
}