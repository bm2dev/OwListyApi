using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.AuthModels;
public class RegisterViewModel
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}