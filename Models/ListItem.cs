namespace OwListy.Models;

public partial class ListItem
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public bool Completed { get; set; }

    public int? ListId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual List? List { get; set; }
}
