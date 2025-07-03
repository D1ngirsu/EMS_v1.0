using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Employee
{
    [Key]
    public int Eid { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    public DateTime DoB { get; set; }

    [Required]
    [StringLength(100)]
    public string Email { get; set; }

    [StringLength(20)]
    public string Phone { get; set; }

    [StringLength(255)]
    public string Address { get; set; }

    [StringLength(10)]
    public string Gender { get; set; } // Nam, Nữ, Khác

    public int Experience { get; set; } = 0;

    [Required]
    [StringLength(20)]
    public string Source { get; set; } // Nội tuyến, Ngoại tuyến

    [Required]
    public int UnitId { get; set; }

    [Required]
    public int PositionId { get; set; }

    [StringLength(50)]
    public string BankNumber { get; set; }

    [StringLength(100)]
    public string Bank { get; set; }

    [StringLength(1000)]
    public string Img { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public OrganizationUnit? OrganizationUnit { get; set; }
    public Position? Position { get; set; }
    public Employee_CD? EmployeeCD { get; set; } // Tùy chọn
    public Employee_AS? EmployeeAS { get; set; } // Tùy chọn
    public Employee_Salary? EmployeeSalary { get; set; } // Tùy chọn
    public Employee_Insurance? EmployeeInsurance { get; set; } // Tùy chọn
    public ICollection<Employee_Application>? Applications { get; set; } // Tùy chọn
    public ICollection<Employee_Relatives>? Relatives { get; set; } // Tùy chọn
    public ICollection<Employee_Todolist>? TodoList { get; set; } // Tùy chọn
    public Employee_CL? EmployeeCL { get; set; } //Optional
}