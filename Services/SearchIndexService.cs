using BibleApp.Models;

namespace BibleApp.Services;

public class SearchIndexService
{
    private readonly BibleService _bibleService;
    private Dictionary<string, List<SearchResult>>? _index;
    private readonly HashSet<string> _indexedSlugs = new();

    public SearchIndexService(BibleService bibleService)
    {
        _bibleService = bibleService;
    }

    public bool IsIndexed(string slug) => _indexedSlugs.Contains(slug);

    public async Task IndexLivreAsync(string slug)
    {
        var livre = await _bibleService.GetLivreAsync(slug);
        if (livre == null || _indexedSlugs.Contains(slug)) return;

        _index ??= new Dictionary<string, List<SearchResult>>();

        foreach (var chapitre in livre.ContenuChapitre)
        {
            foreach (var verset in chapitre.ContenuVersets)
            {
                var mots = Tokenize(verset.TextePropre);
                foreach (var mot in mots)
                {
                    if (!_index.ContainsKey(mot))
                        _index[mot] = new List<SearchResult>();

                    _index[mot].Add(new SearchResult
                    {
                        Slug = slug,
                        NomLivre = livre.NomLivre,
                        Chapitre = chapitre.Numero,
                        Verset = verset.Numero,
                        Texte = verset.TextePropre
                    });
                }
            }
        }

        _indexedSlugs.Add(slug);
    }

    public async Task IndexAllLoadedAsync()
    {
        var index = await _bibleService.GetIndexAsync();
        foreach (var livre in index)
        {
            if (!_indexedSlugs.Contains(livre.Slug))
                await IndexLivreAsync(livre.Slug);
        }
    }

    public List<SearchResult> Rechercher(string query, string? testament = null, string? livreSlug = null)
    {
        if (_index == null || string.IsNullOrWhiteSpace(query))
            return [];

        var mots = Tokenize(query);
        if (mots.Count == 0) return [];

        IEnumerable<SearchResult>? results = null;

        foreach (var mot in mots)
        {
            var matches = _index
                .Where(kv => kv.Key.StartsWith(mot) || kv.Key.Contains(mot))
                .SelectMany(kv => kv.Value)
                .ToList();

            if (results == null)
            {
                results = matches;
            }
            else
            {
                var verseKeys = matches.Select(m => (m.Slug, m.Chapitre, m.Verset)).ToHashSet();
                results = results.Where(r => verseKeys.Contains((r.Slug, r.Chapitre, r.Verset))).ToList();
            }
        }

        var final = results?.ToList() ?? new List<SearchResult>();

        if (!string.IsNullOrEmpty(testament))
        {
            var testaments = testament == "AT"
                ? new[] { "Genèse", "Exode", "Lévitique", "Nombres", "Deutéronome", "Josué", "Juges", "Ruth",
                    "1 Samuel", "2 Samuel", "1 Rois", "2 Rois", "1 Chroniques", "2 Chroniques", "Esdras",
                    "Néhémie", "Esther", "Job", "Psaumes", "Proverbes", "Ecclésiaste", "Cantique des cantiques",
                    "Esaïe", "Jérémie", "Lamentations de Jérémie", "Ezéchiel", "Daniel", "Osée", "Joël",
                    "Amos", "Abdias", "Jonas", "Michée", "Nahum", "Habacuc", "Sophonie", "Aggée",
                    "Zacharie", "Malachie" }
                : new[] { "Matthieu", "Marc", "Luc", "Jean", "Actes des Apôtres", "Romains",
                    "1 Corinthiens", "2 Corinthiens", "Galates", "Ephésiens", "Philippiens", "Colossiens",
                    "1 Thessaloniciens", "2 Thessaloniciens", "1 Timothée", "2 Timothée", "Tite",
                    "Philémon", "Hébreux", "Jacques", "1 Pierre", "2 Pierre", "1 Jean", "2 Jean",
                    "3 Jean", "Jude", "Apocalypse" };

            final = final.Where(r => testaments.Contains(r.NomLivre)).ToList();
        }

        if (!string.IsNullOrEmpty(livreSlug))
            final = final.Where(r => r.Slug == livreSlug).ToList();

        return final
            .GroupBy(r => (r.Slug, r.Chapitre, r.Verset))
            .Select(g => g.First())
            .OrderBy(r => r.Slug)
            .ThenBy(r => r.Chapitre)
            .ThenBy(r => r.Verset)
            .Take(100)
            .ToList();
    }

    private static List<string> Tokenize(string text)
    {
        var normalized = text.ToLowerInvariant()
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("-", " ")
            .Replace("'", " ")
            .Replace("’", " ");

        var sb = new System.Text.StringBuilder();
        foreach (var c in normalized.Normalize(System.Text.NormalizationForm.FormD))
        {
            var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1)
            .Distinct()
            .ToList();
    }
}

public class SearchResult
{
    public string Slug { get; set; } = string.Empty;
    public string NomLivre { get; set; } = string.Empty;
    public int Chapitre { get; set; }
    public int Verset { get; set; }
    public string Texte { get; set; } = string.Empty;
    public string Reference => $"{NomLivre} {Chapitre}:{Verset}";
}
