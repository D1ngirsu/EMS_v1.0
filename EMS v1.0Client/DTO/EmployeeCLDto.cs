﻿public class EmployeeCLDto
{
    public int Cid { get; set; }
    public int Eid { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; } // "Thử việc", "Có thời hạn", "Vô thời hạn"
    public string? EmployeeUser { get; set; }
    public DateTime SignDate { get; set; }
    public string? Img { get; set; }
}