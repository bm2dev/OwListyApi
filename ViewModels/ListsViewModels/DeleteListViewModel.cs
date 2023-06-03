using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.ListsViewModels;
public class DeleteListViewModel
{
    [Required]
    public int ListId { get; set; }
}