using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Employee_Todolist
{
    [Key]
    public int Tid { get; set; }

    [Required]
    public int Eid { get; set; } // Changed from Uid to Eid

    [Required]
    public DateTime DayIssue { get; set; }

    [Required]
    public DateTime Deadline { get; set; }

    [Required]
    [StringLength(100)]
    public string AssignedBy { get; set; }

    [Required]
    public string Content { get; set; }

    [Required]
    public byte Status { get; set; } // 0 or 1

    // Navigation property
    [ForeignKey("Eid")]
    public Employee Employee { get; set; }
}