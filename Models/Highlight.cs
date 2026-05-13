namespace BibleApp.Models;

public class Highlight
{
    public string Id { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Chapitre { get; set; }
    public int Verset { get; set; }
    public string Couleur { get; set; } = "yellow";

    public static readonly Dictionary<string, string> Couleurs = new()
    {
        { "yellow", "#fdd835" },
        { "green", "#4caf50" },
        { "blue", "#2196f3" },
        { "pink", "#e91e63" },
        { "orange", "#ff9800" },
    };
}
