using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    public int Uid { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [StringLength(255)]
    public string Password { get; set; }

    public int? Eid { get; set; } // New nullable field to link to Employee

    // Navigation property
    [ForeignKey("Eid")]
    public Employee Employee { get; set; }
}