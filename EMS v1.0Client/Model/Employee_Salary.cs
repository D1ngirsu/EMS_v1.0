using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class Employee_Salary
{
    [Key]
    public int Eid { get; set; } // Changed from Uid to Eid

    [Required]
    [Column(TypeName = "decimal(15,2)")]
    public decimal Salary { get; set; }

    // Navigation property
    [ForeignKey("Eid")]
    public Employee? Employee { get; set; }
}