using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.ListsViewModels;
public class DeleteListItemViewModel
{
    [Required]
    public int ListItemId { get; set; }
}