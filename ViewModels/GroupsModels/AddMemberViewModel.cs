using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.GroupsModels;
public partial class AddMemberViewModel
{
    [Required]
    public int GroupId { get; set; }

    [Required]
    public int UserId { get; set; }
}
