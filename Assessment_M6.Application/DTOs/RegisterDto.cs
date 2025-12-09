using System.ComponentModel.DataAnnotations;
using Assessment_M6.Domain.Enums;

namespace Assessment_M6.Application.DTOs;

public class RegisterDto
{
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
    
    [Required]
    public UserRole Role { get; set; }
}