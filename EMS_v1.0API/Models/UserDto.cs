using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class UserDto
{
    public int Uid { get; set; }
    public string Username { get; set; }
    public string Role { get; set; } = "User";
    public EmployeeDto Employee { get; set; }
}