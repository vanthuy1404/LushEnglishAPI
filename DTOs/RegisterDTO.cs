// 2025
// DANGTHUY

using System.ComponentModel.DataAnnotations;

namespace LushEnglishAPI.DTOs;

public class RegisterRequestDTO
{
    [Required, MaxLength(255)] public string FullName { get; set; }

    [Required, MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; }

    [Required, MaxLength(255)] public string Password { get; set; }
    [Required, MaxLength(255)] public string ConfirmPassword { get; set; }

    
}