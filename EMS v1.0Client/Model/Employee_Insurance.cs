using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class Employee_Insurance
{
    [Key]
    public int Eid { get; set; } // Changed from Uid to Eid

    [Required]
    [StringLength(500)]
    public string InsuranceContent { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal ContributePercent { get; set; }

    // Navigation property
    [ForeignKey("Eid")]
    public Employee Employee { get; set; }
}