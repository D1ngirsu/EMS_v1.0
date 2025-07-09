using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Employee_CL
{
    [Key]
    public int Cid { get; set; }

    [Required]
    public int Eid { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? ExpectedEndDate { get; set; }

    [Required]
    [StringLength(20)]
    public string? Status { get; set; } // "Hiệu lực" hoặc "Không Hiệu lực"

    [Required]
    [StringLength(20)]
    public string? Type { get; set; } // "Thử việc", "Có thời hạn", "Vô thời hạn"

    [Required]
    [StringLength(100)]
    public string? EmployeeUser { get; set; }

    [Required]
    public DateTime SignDate { get; set; }

    [StringLength(500)]
    public string? Img { get; set; } // Đường dẫn ảnh hợp đồng lao động

    // Navigation property
    [ForeignKey("Eid")]
    public Employee? Employee { get; set; }
}