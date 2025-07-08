public class EmployeeCDDto
{
    public int Eid { get; set; }
    public string IdNumber { get; set; }
    public DateTime IssueDay { get; set; }
    public string IssuePlace { get; set; }
    public string Country { get; set; }
    public EmployeeDto? Employee { get; set; }
}