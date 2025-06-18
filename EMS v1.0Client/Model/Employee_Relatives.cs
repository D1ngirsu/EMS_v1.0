using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class Employee_Relatives
{
    [Key]
    public int RelId { get; set; }

    [Required]
    public int Eid { get; set; } // Changed from Uid to Eid

    [Required]
    [StringLength(100)]
    public string RName { get; set; }

    [Required]
    [StringLength(50)]
    public string RRelativity { get; set; }

    [StringLength(100)]
    public string RContact { get; set; }

    [Required]
    public byte Type { get; set; } // 0 or 1

    // Navigation property
    [ForeignKey("Eid")]
    public Employee Employee { get; set; }
}