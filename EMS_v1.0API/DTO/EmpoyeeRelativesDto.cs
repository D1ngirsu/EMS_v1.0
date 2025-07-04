using System.ComponentModel.DataAnnotations;

public class EmployeeRelativesDto
{
    public int? RelId { get; set; }
    [Required] public int Eid { get; set; }
    [Required][StringLength(100)] public string RName { get; set; }
    [Required][StringLength(50)] public string RRelativity { get; set; }
    [StringLength(100)] public string RContact { get; set; }
    [Required] public byte Type { get; set; }
}