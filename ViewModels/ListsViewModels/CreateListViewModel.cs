using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.ListsViewModels;
public class CreateListViewModel
{
    [Required]
    public string Title { get; set; } = null!;

    public string? Color { get; set; }

    [Required]
    public int GroupId { get; set; }
}