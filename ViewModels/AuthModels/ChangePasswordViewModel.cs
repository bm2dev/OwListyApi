using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.AuthModels;
public class ChangePasswordViewModel
{
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}