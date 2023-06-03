using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.ListsViewModels;
public class UpdateListViewModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = null!;

    public string? Color { get; set; }
}