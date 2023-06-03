using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.ListsViewModels;
public class CreateListItemViewModel
{
    [Required]
    public int? ListId { get; set; }

    [Required(AllowEmptyStrings = true)]
    public string? Content { get; set; }

    [Required]
    public bool Completed { get; set; }
}