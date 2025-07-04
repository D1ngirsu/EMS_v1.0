using Microsoft.AspNetCore.Mvc;

public class EmployeeDto
{
    public int Eid { get; set; }
    public string Name { get; set; }
    public DateTime DoB { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Gender { get; set; }
    public int Experience { get; set; }
    public string Source { get; set; }
    public string BankNumber { get; set; }
    public string Bank { get; set; }
    public string Img { get; set; }
    public OrganizationUnitDto OrganizationUnit { get; set; }
    public OrganizationUnitDto ParentOrganizationUnit { get; set; }
    public PositionDto Position { get; set; }
}