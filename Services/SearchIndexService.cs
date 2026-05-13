using BibleApp.Models;

namespace BibleApp.Services;

public class SearchIndexService
{
    private readonly BibleService _bibleService;
    private Dictionary<string, List<VerseRef>>? _index;
    private readonly Dictionary<(string, int, int), string> _verseTextCache = new();
    private readonly HashSet<string> _indexedSlugs = new();

    private struct VerseRef
    {
        public string Slug;
        public string NomLivre;
        public int Chapitre;
        public int Verset;
    }

    public SearchIndexService(BibleService bibleService)
    {
        _bibleService = bibleService;
    }

    public bool IsIndexed(string slug) => _indexedSlugs.Contains(slug);

    public async Task IndexLivreAsync(string slug)
    {
        var livre = await _bibleService.GetLivreAsync(slug);
        if (livre == null || _indexedSlugs.Contains(slug)) return;

        _index ??= new Dictionary<string, List<VerseRef>>();

        foreach (var chapitre in livre.ContenuChapitre)
        {
            foreach (var verset in chapitre.ContenuVersets)
            {
                var key = (slug, chapitre.Numero, verset.Numero);
                _verseTextCache.TryAdd(key, verset.TextePropre);

                var mots = Tokenize(verset.TextePropre);
                foreach (var mot in mots)
                {
                    if (!_index.ContainsKey(mot))
                        _index[mot] = new List<VerseRef>();

                    _index[mot].Add(new VerseRef
                    {
                        Slug = slug,
                        NomLivre = livre.NomLivre,
                        Chapitre = chapitre.Numero,
                        Verset = verset.Numero
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

    public List<SearchResult> Rechercher(string query, string? testament = null, string? livreSlug = null, bool modeEt = true)
    {
        if (_index == null || string.IsNullOrWhiteSpace(query))
            return [];

        var (phrasesExactes, motsSimples) = ParseQuery(query);
        if (phrasesExactes.Count == 0 && motsSimples.Count == 0) return [];

        var versesParMot = new List<List<VerseRef>>();

        foreach (var mot in motsSimples)
        {
            var matches = _index
                .Where(kv => kv.Key.StartsWith(mot) || kv.Key.Contains(mot))
                .SelectMany(kv => kv.Value)
                .ToList();
            if (matches.Count > 0)
                versesParMot.Add(matches);
        }

        List<VerseRef>? results = null;

        if (versesParMot.Count > 0)
        {
            if (modeEt)
            {
                results = versesParMot[0].ToList();
                for (int i = 1; i < versesParMot.Count; i++)
                {
                    var keys = versesParMot[i].Select(m => (m.Slug, m.Chapitre, m.Verset)).ToHashSet();
                    results = results.Where(r => keys.Contains((r.Slug, r.Chapitre, r.Verset))).ToList();
                }
            }
            else
            {
                var seen = new HashSet<(string, int, int)>();
                results = new List<VerseRef>();
                foreach (var list in versesParMot)
                {
                    foreach (var r in list)
                    {
                        var k = (r.Slug, r.Chapitre, r.Verset);
                        if (seen.Add(k)) results.Add(r);
                    }
                }
            }
        }

        var final = results?.ToList() ?? new List<VerseRef>();

        foreach (var phrase in phrasesExactes)
        {
            var phraseLower = phrase.ToLowerInvariant();
            var filtered = final.Count > 0
                ? final.Where(r => _verseTextCache.TryGetValue((r.Slug, r.Chapitre, r.Verset), out var t) && t.ToLowerInvariant().Contains(phraseLower)).ToList()
                : _index
                    .SelectMany(kv => kv.Value)
                    .Where(r => _verseTextCache.TryGetValue((r.Slug, r.Chapitre, r.Verset), out var t) && t.ToLowerInvariant().Contains(phraseLower))
                    .DistinctBy(r => (r.Slug, r.Chapitre, r.Verset))
                    .ToList();
            final = filtered;
        }

        if (!string.IsNullOrEmpty(testament))
        {
            final = testament == "AT"
                ? final.Where(r => _atBooks.Contains(r.NomLivre)).ToList()
                : final.Where(r => _ntBooks.Contains(r.NomLivre)).ToList();
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
            .Select(r => new SearchResult
            {
                Slug = r.Slug,
                NomLivre = r.NomLivre,
                Chapitre = r.Chapitre,
                Verset = r.Verset,
                Texte = _verseTextCache.TryGetValue((r.Slug, r.Chapitre, r.Verset), out var t) ? t : ""
            })
            .ToList();
    }

    private static (List<string> Phrases, List<string> Mots) ParseQuery(string query)
    {
        var phrases = new List<string>();
        var mots = new List<string>();
        var remaining = new System.Text.StringBuilder();
        int i = 0;
        while (i < query.Length)
        {
            if (query[i] == '"')
            {
                var end = query.IndexOf('"', i + 1);
                if (end > i + 1)
                {
                    var phrase = query[(i + 1)..end].Trim();
                    if (phrase.Length >= 2)
                        phrases.Add(phrase);
                    i = end + 1;
                    continue;
                }
                remaining.Append(query[i]);
            }
            else
            {
                remaining.Append(query[i]);
            }
            i++;
        }

        var remainingStr = remaining.ToString();
        if (!string.IsNullOrWhiteSpace(remainingStr))
            mots = Tokenize(remainingStr);

        return (phrases, mots);
    }

    private static readonly HashSet<string> _atBooks = new(StringComparer.OrdinalIgnoreCase)
    {
        "Genèse", "Exode", "Lévitique", "Nombres", "Deutéronome", "Josué", "Juges", "Ruth",
        "1 Samuel", "2 Samuel", "1 Rois", "2 Rois", "1 Chroniques", "2 Chroniques", "Esdras",
        "Néhémie", "Esther", "Job", "Psaumes", "Proverbes", "Ecclésiaste", "Cantique des cantiques",
        "Esaïe", "Jérémie", "Lamentations de Jérémie", "Ezéchiel", "Daniel", "Osée", "Joël",
        "Amos", "Abdias", "Jonas", "Michée", "Nahum", "Habacuc", "Sophonie", "Aggée",
        "Zacharie", "Malachie"
    };

    private static readonly HashSet<string> _ntBooks = new(StringComparer.OrdinalIgnoreCase)
    {
        "Matthieu", "Marc", "Luc", "Jean", "Actes des Apôtres", "Romains",
        "1 Corinthiens", "2 Corinthiens", "Galates", "Ephésiens", "Philippiens", "Colossiens",
        "1 Thessaloniciens", "2 Thessaloniciens", "1 Timothée", "2 Timothée", "Tite",
        "Philémon", "Hébreux", "Jacques", "1 Pierre", "2 Pierre", "1 Jean", "2 Jean",
        "3 Jean", "Jude", "Apocalypse"
    };

    private static List<string> Tokenize(string text)
    {
        var normalized = text.ToLowerInvariant()
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("-", " ")
            .Replace("'", " ")
            .Replace("’", " ");

        // Normalize(FormD) échoue avec InvariantGlobalization=true sur WASM
        try
        {
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized.Normalize(System.Text.NormalizationForm.FormD))
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            normalized = sb.ToString();
        }
        catch
        {
            // Fallback: replace common accented chars manually
            normalized = normalized
                .Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("ë", "e")
                .Replace("à", "a").Replace("â", "a").Replace("ä", "a")
                .Replace("ù", "u").Replace("û", "u").Replace("ü", "u")
                .Replace("ô", "o").Replace("ö", "o")
                .Replace("î", "i").Replace("ï", "i")
                .Replace("ç", "c");
        }

        return normalized
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
