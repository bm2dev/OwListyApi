using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.GroupsModels;

public partial class AddMemberViewModel
{
    [Required]
    public int GroupId { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    public string UserEmail { get; set; } = null!;
}
