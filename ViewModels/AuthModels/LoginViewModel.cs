using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.AuthModels;
public class LoginViewModel
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}