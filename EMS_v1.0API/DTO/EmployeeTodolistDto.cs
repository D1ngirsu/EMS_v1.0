public class EmployeeTodolistDto
{
    public int Tid { get; set; }
    public int Eid { get; set; }
    public DateTime DayIssue { get; set; }
    public DateTime Deadline { get; set; }
    public string AssignedBy { get; set; }
    public string Content { get; set; }
    public byte Status { get; set; }
    public EmployeeDto Employee { get; set; }
}