using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.AuthModels;

public class ResetPasswordViewModel
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;

    [Required]
    public string ValidationCode { get; set; } = null!;
}
