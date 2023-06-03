using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.GroupsModels;
public partial class RemoveMemberViewModel
{
    [Required]
    public int MemberId { get; set; }
}
