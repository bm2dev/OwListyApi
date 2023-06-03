using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.GroupsModels;
public class UpdateGroupViewModel
{
    [Required]
    public int GroupId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? Color { get; set; }

}
