using BibleApp.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace BibleApp.Services;

public class SearchIndexService
{
    private readonly BibleService _bibleService;
    private readonly HttpClient _http;
    private Dictionary<string, List<string>>? _index; // word -> list of "b:c:v"
    private List<LivreIndex>? _bookIndex;
    private readonly Dictionary<(string, int, int), string> _verseTextCache = new();
    private readonly HashSet<string> _indexedSlugs = new();

    public SearchIndexService(BibleService bibleService, HttpClient http)
    {
        _bibleService = bibleService;
        _http = http;
    }

    public bool IsIndexed(string slug) => _indexedSlugs.Contains(slug) || _index != null;

    public async Task IndexLivreAsync(string slug)
    {
        if (_indexedSlugs.Contains(slug)) return;
        var livre = await _bibleService.GetLivreAsync(slug);
        if (livre == null) return;
        foreach (var chapitre in livre.ContenuChapitre)
            foreach (var verset in chapitre.ContenuVersets)
                _verseTextCache.TryAdd((slug, chapitre.Numero, verset.Numero), verset.TextePropre);
        _indexedSlugs.Add(slug);
    }

    public async Task IndexAllLoadedAsync()
    {
        // Load pre-built search index (compact, ~4MB raw / ~1.1MB gzip) once.
        if (_index != null) return;
        try
        {
            _bookIndex = await _bibleService.GetIndexAsync();
            var doc = await _http.GetFromJsonAsync<Dictionary<string, List<string>>>("data/search-index.json");
            _index = doc ?? new();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[SearchIndex] Échec chargement index: {ex.Message}");
            _index = new();
        }
    }

    public List<SearchResult> Rechercher(string query, string? testament = null, string? livreSlug = null, bool modeEt = true)
    {
        if (_index == null || string.IsNullOrWhiteSpace(query))
            return [];

        var (phrasesExactes, motsSimples) = ParseQuery(query);
        if (phrasesExactes.Count == 0 && motsSimples.Count == 0) return [];

        var versesParMot = new List<List<(int b, int c, int v)>>();

        foreach (var mot in motsSimples)
        {
            var matches = new List<(int, int, int)>();
            // prefix/contains match on keys
            foreach (var kv in _index)
            {
                if (kv.Key.StartsWith(mot) || kv.Key.Contains(mot))
                {
                    foreach (var refStr in kv.Value)
                    {
                        var parts = refStr.Split(':');
                        if (parts.Length != 3) continue;
                        if (!int.TryParse(parts[0], out var b)) continue;
                        if (!int.TryParse(parts[1], out var c)) continue;
                        if (!int.TryParse(parts[2], out var v)) continue;
                        matches.Add((b, c, v));
                    }
                }
            }
            if (matches.Count > 0)
                versesParMot.Add(matches);
        }

        List<(int b, int c, int v)>? results = null;

        if (versesParMot.Count > 0)
        {
            if (modeEt)
            {
                results = versesParMot[0].ToList();
                for (int i = 1; i < versesParMot.Count; i++)
                {
                    var keys = versesParMot[i].ToHashSet();
                    results = results.Where(r => keys.Contains(r)).ToList();
                }
            }
            else
            {
                var seen = new HashSet<(int, int, int)>();
                results = new List<(int, int, int)>();
                foreach (var list in versesParMot)
                    foreach (var r in list)
                        if (seen.Add(r)) results.Add(r);
            }
        }

        var final = results?.ToList() ?? new List<(int, int, int)>();

        // phrase exacte filter
        foreach (var phrase in phrasesExactes)
        {
            var phraseLower = phrase.ToLowerInvariant();
            // if no results yet, scan all refs
            var candidates = final.Count > 0 ? final : _index.SelectMany(kv => kv.Value)
                .Select(refStr => { var p = refStr.Split(':'); return (b: int.Parse(p[0]), c: int.Parse(p[1]), v: int.Parse(p[2])); }).Distinct().ToList();
            final = candidates.Where(r =>
            {
                var slug = _bookIndex != null && r.b < _bookIndex.Count ? _bookIndex[r.b].Slug : "";
                if (!_verseTextCache.TryGetValue((slug, r.c, r.v), out var t))
                {
                    // lazy load text not available; skip filter
                    return true;
                }
                return t.ToLowerInvariant().Contains(phraseLower);
            }).ToList();
        }

        // testament / livre filter
        if (!string.IsNullOrEmpty(testament) && _bookIndex != null)
        {
            var slugs = testament == "AT"
                ? _bookIndex.Where(i => i.Testament.StartsWith("Ancien")).Select(i => i.Slug).ToHashSet()
                : _bookIndex.Where(i => i.Testament.StartsWith("Nouveau")).Select(i => i.Slug).ToHashSet();
            final = final.Where(r => r.b < _bookIndex.Count && slugs.Contains(_bookIndex[r.b].Slug)).ToList();
        }
        if (!string.IsNullOrEmpty(livreSlug) && _bookIndex != null)
        {
            final = final.Where(r => r.b < _bookIndex.Count && _bookIndex[r.b].Slug == livreSlug).ToList();
        }

        return final
            .GroupBy(r => r)
            .Select(g => g.First())
            .OrderBy(r => r.b).ThenBy(r => r.c).ThenBy(r => r.v)
            .Take(100)
            .Select(r =>
            {
                var slug = _bookIndex != null && r.b < _bookIndex.Count ? _bookIndex[r.b].Slug : "";
                var nom = _bookIndex != null && r.b < _bookIndex.Count ? _bookIndex[r.b].NomLivre : slug;
                // ensure text cached
                if (!_verseTextCache.TryGetValue((slug, r.c, r.v), out var txt))
                {
                    _ = EnsureTextCachedAsync(slug, r.c, r.v);
                    txt = "";
                }
                return new SearchResult
                {
                    Slug = slug,
                    NomLivre = nom,
                    Chapitre = r.c,
                    Verset = r.v,
                    Texte = txt
                };
            })
            .ToList();
    }

    private async Task EnsureTextCachedAsync(string slug, int chap, int vers)
    {
        try
        {
            var c = await _bibleService.GetChapitreAsync(slug, chap);
            var v = c?.ContenuVersets.FirstOrDefault(x => x.Numero == vers);
            if (v != null) _verseTextCache.TryAdd((slug, chap, vers), v.TextePropre);
        }
        catch { }
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
                    if (phrase.Length >= 2) phrases.Add(phrase);
                    i = end + 1;
                    continue;
                }
                remaining.Append(query[i]);
            }
            else remaining.Append(query[i]);
            i++;
        }
        var remainingStr = remaining.ToString();
        if (!string.IsNullOrWhiteSpace(remainingStr))
            mots = Tokenize(remainingStr);
        return (phrases, mots);
    }

    private static List<string> Tokenize(string text)
    {
        var normalized = text.ToLowerInvariant().Replace("\r", " ").Replace("\n", " ").Replace("-", " ").Replace("'", " ").Replace("’", " ");
        return normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length > 2).Distinct().ToList();
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
