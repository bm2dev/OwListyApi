using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.ListsViewModels;
public class UpdateListItemViewModel
{
    [Required]
    public int? Id { get; set; }

    [Required(AllowEmptyStrings = true)]
    public string? Content { get; set; }

    [Required]
    public bool Completed { get; set; }
}