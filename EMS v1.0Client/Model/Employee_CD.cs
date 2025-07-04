using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Employee_CD
{
    [Key]
    public int Eid { get; set; } // Changed from Uid to Eid

    [Required]
    [StringLength(50)]
    public string IdNumber { get; set; }

    [Required]
    public DateTime IssueDay { get; set; }

    [Required]
    [StringLength(255)]
    public string IssuePlace { get; set; }

    [Required]
    [StringLength(100)]
    public string Country { get; set; }

    // Navigation property
    [ForeignKey("Eid")]
    public Employee? Employee { get; set; }
}