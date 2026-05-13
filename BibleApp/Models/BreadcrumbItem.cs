namespace BibleApp.Models;

public class BreadcrumbNode
{
    public string Label { get; set; } = string.Empty;
    public string? Href { get; set; }
    public string? Icon { get; set; }
}
