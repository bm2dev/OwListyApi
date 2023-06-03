using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.AuthModels;
public class ForgotPasswordViewModel
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;
}