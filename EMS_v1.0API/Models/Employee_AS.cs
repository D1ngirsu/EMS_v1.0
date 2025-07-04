using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class Employee_AS
{
    [Key]
    public int Eid { get; set; } // Changed from Uid to Eid

    [Required]
    [StringLength(100)]
    public string AcademicRank { get; set; }

    [Required]
    [StringLength(100)]
    public string Degree { get; set; }

    [Required]
    [StringLength(255)]
    public string PlaceIssue { get; set; }

    [Required]
    public DateTime IssueDay { get; set; }

    [StringLength(255)]
    public string? DegreeImg1 { get; set; }

    [StringLength(255)]
    public string? DegreeImg2 { get; set; }

    // Navigation property
    [ForeignKey("Eid")]
    public Employee Employee { get; set; }
}