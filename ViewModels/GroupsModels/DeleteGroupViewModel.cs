using System.ComponentModel.DataAnnotations;

namespace OwListy.ViewModels.GroupsModels
{
    public class DeleteGroupViewModel
    {
        [Required]
        public int GroupId { get; set; }
    }
}