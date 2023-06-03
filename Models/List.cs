namespace OwListy.Models;

public partial class List
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Color { get; set; }

    public int? GroupId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Group? Group { get; set; }

    public virtual ICollection<ListItem> ListItems { get; set; } = new List<ListItem>();
}
