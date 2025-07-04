public class EmployeeRelativesDto
{
    public int RelId { get; set; }
    public int Eid { get; set; }
    public string RName { get; set; }
    public string RRelativity { get; set; }
    public string RContact { get; set; }
    public byte Type { get; set; }
    public EmployeeDto Employee { get; set; }
}