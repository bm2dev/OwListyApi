using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.AuthModels;
public class ResetPasswordViewModel
{
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;

    [Required]
    public string ValidationToken { get; set; } = null!;
}