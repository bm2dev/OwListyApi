using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.GroupsModels;
public class CreateGroupViewModel
{
    [Required]
    public string Name { get; set; } = null!;

    public string? Color { get; set; }
}
