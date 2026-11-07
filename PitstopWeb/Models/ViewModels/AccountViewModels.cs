using System.ComponentModel.DataAnnotations;

namespace PitstopWeb.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Имейлът е задължителен")]
    [EmailAddress]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Паролата е задължителна")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Името е задължително")]
    [Display(Name = "Име")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Имейлът е задължителен")]
    [EmailAddress]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Паролата е задължителна")]
    [MinLength(6, ErrorMessage = "Паролата трябва да е поне 6 символа")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
