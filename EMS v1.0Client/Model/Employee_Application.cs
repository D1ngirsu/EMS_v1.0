using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class Employee_Application
{
    [Key]
    public int AppId { get; set; }

    [Required]
    public int Eid { get; set; } // Changed from Uid to Eid

    [Required]
    [StringLength(200)]
    public string ApplicationName { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [StringLength(255)]
    public string Img { get; set; }

    [Required]
    [StringLength(50)]
    public string ApplicationType { get; set; } // xác nhận, đơn, hợp đồng lao động

    // Navigation property
    [ForeignKey("Eid")]
    public Employee Employee { get; set; }
}