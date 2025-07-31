using System;
using System.ComponentModel.DataAnnotations;

public class HR_Notification
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(1000)]
    public string Content { get; set; }

    [Required]
    public DateTime DateTime { get; set; }
}