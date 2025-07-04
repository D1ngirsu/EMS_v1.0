public class EmployeeApplicationDto
{
    public int AppId { get; set; }
    public int Eid { get; set; }
    public string? ApplicationName { get; set; }
    public DateTime Date { get; set; }
    public string? Img { get; set; }
    public string? ApplicationType { get; set; }
    public EmployeeDto? Employee { get; set; }
}