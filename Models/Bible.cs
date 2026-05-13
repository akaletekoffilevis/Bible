using System.Text.Json.Serialization;

namespace BibleApp.Models;

public class BibleData
{
    [JsonPropertyName("Traduction")]
    public string Traduction { get; set; } = string.Empty;

    [JsonPropertyName("ContenuBible")]
    public List<Testament> ContenuBible { get; set; } = [];
}

public class Testament
{
    [JsonPropertyName("Titre")]
    public string Titre { get; set; } = string.Empty;

    [JsonPropertyName("Livres")]
    public List<Livre> Livres { get; set; } = [];
}

public class Livre
{
    [JsonPropertyName("Abreviation")]
    public string? Abreviation { get; set; }

    [JsonPropertyName("NomLivre")]
    public string NomLivre { get; set; } = string.Empty;

    [JsonPropertyName("ContenuChapitre")]
    public List<Chapitre> ContenuChapitre { get; set; } = [];

    public string Slug => Slugifier(NomLivre);

    public string? AbreviationCourante => Abreviation ?? Slug;

    private static string Slugifier(string nom)
    {
        var normalized = nom.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in normalized)
        {
            var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                var lower = char.ToLowerInvariant(c);
                if (lower == '\'')
                    sb.Append('-');
                else if (lower == ' ')
                    sb.Append('-');
                else if (lower == '-' && sb.Length > 0 && sb[^1] == '-')
                    continue;
                else if ((lower >= 'a' && lower <= 'z') || (lower >= '0' && lower <= '9') || lower == '-')
                    sb.Append(lower);
            }
        }
        while (sb.Length > 0 && sb[^1] == '-')
            sb.Remove(sb.Length - 1, 1);
        if (sb.Length > 0 && sb[0] == '-')
            sb.Remove(0, 1);
        return sb.ToString();
    }

    public static readonly Dictionary<string, string> Abreviations = new()
    {
        { "gn", "Genèse" }, { "ge", "Genèse" },
        { "ex", "Exode" },
        { "le", "Lévitique" }, { "lv", "Lévitique" },
        { "nb", "Nombres" }, { "nm", "Nombres" },
        { "dt", "Deutéronome" },
        { "js", "Josué" },
        { "jg", "Juges" },
        { "rt", "Ruth" },
        { "1 s", "1 Samuel" }, { "1sa", "1 Samuel" },
        { "2 s", "2 Samuel" }, { "2sa", "2 Samuel" },
        { "1 r", "1 Rois" }, { "1ro", "1 Rois" },
        { "2 r", "2 Rois" }, { "2ro", "2 Rois" },
        { "1 ch", "1 Chroniques" }, { "1ch", "1 Chroniques" },
        { "2 ch", "2 Chroniques" }, { "2ch", "2 Chroniques" },
        { "esd", "Esdras" },
        { "né", "Néhémie" }, { "ne", "Néhémie" },
        { "est", "Esther" },
        { "jb", "Job" }, { "j ob", "Job" },
        { "ps", "Psaumes" },
        { "pr", "Proverbes" },
        { "ec", "Ecclésiaste" },
        { "ca", "Cantique des cantiques" }, { "ct", "Cantique des cantiques" },
        { "es", "Esaïe" },
        { "jr", "Jérémie" }, { "je", "Jérémie" },
        { "lm", "Lamentations de Jérémie" },
        { "éz", "Ezéchiel" }, { "ez", "Ezéchiel" },
        { "dn", "Daniel" }, { "da", "Daniel" },
        { "os", "Osée" },
        { "jl", "Joël" },
        { "am", "Amos" },
        { "ab", "Abdias" },
        { "jon", "Jonas" },
        { "mi", "Michée" },
        { "na", "Nahum" },
        { "ha", "Habacuc" },
        { "so", "Sophonie" },
        { "ag", "Aggée" },
        { "za", "Zacharie" },
        { "ml", "Malachie" },
        { "mt", "Matthieu" },
        { "mc", "Marc" },
        { "lc", "Luc" },
        { "jn", "Jean" },
        { "ac", "Actes des Apôtres" },
        { "rm", "Romains" }, { "ro", "Romains" },
        { "1 co", "1 Corinthiens" }, { "1cor", "1 Corinthiens" },
        { "2 co", "2 Corinthiens" }, { "2cor", "2 Corinthiens" },
        { "ga", "Galates" },
        { "ep", "Ephésiens" },
        { "ph", "Philippiens" },
        { "col", "Colossiens" },
        { "1 th", "1 Thessaloniciens" }, { "1th", "1 Thessaloniciens" },
        { "2 th", "2 Thessaloniciens" }, { "2th", "2 Thessaloniciens" },
        { "1 tm", "1 Timothée" }, { "1ti", "1 Timothée" },
        { "2 tm", "2 Timothée" }, { "2ti", "2 Timothée" },
        { "tt", "Tite" }, { "tit", "Tite" },
        { "phm", "Philémon" },
        { "hb", "Hébreux" }, { "he", "Hébreux" },
        { "jc", "Jacques" }, { "ja", "Jacques" },
        { "1 p", "1 Pierre" }, { "1pi", "1 Pierre" },
        { "2 p", "2 Pierre" }, { "2pi", "2 Pierre" },
        { "1 jn", "1 Jean" }, { "1j n", "1 Jean" },
        { "2 jn", "2 Jean" }, { "2j n", "2 Jean" },
        { "3 jn", "3 Jean" }, { "3j n", "3 Jean" },
        { "jd", "Jude" },
        { "ap", "Apocalypse" },
    };
}

public class Chapitre
{
    [JsonPropertyName("NumeroChapitre")]
    public string NumeroChapitre { get; set; } = string.Empty;

    [JsonPropertyName("ContenuVersets")]
    public List<Verset> ContenuVersets { get; set; } = [];

    public int Numero => int.TryParse(NumeroChapitre, out var n) ? n : 0;
}

public class Verset
{
    [JsonPropertyName("NumeroVerset")]
    public string NumeroVerset { get; set; } = string.Empty;

    [JsonPropertyName("Verset")]
    public string Texte { get; set; } = string.Empty;

    public int Numero => int.TryParse(NumeroVerset, out var n) ? n : 0;

    public string TextePropre => Texte.Trim().Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
    public string Reference => $"{NumeroVerset}";
}

public class LivreIndex
{
    public string Slug { get; set; } = string.Empty;
    public string NomLivre { get; set; } = string.Empty;
    public string? Abreviation { get; set; }
    public string Testament { get; set; } = string.Empty;
    public int NombreChapitres { get; set; }
}
