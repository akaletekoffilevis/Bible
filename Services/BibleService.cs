using System.Net.Http.Json;
using BibleApp.Models;

namespace BibleApp.Services;

public class BibleService
{
    private readonly HttpClient _http;
    private readonly IndexedDbService _db;
    private readonly Dictionary<string, Livre> _cache = new();
    private List<LivreIndex>? _index;

    public BibleService(HttpClient http, IndexedDbService db)
    {
        _http = http;
        _db = db;
    }

    public async Task<List<LivreIndex>> GetIndexAsync()
    {
        if (_index != null) return _index;
        try { _index = await _http.GetFromJsonAsync<List<LivreIndex>>("data/index.json"); }
        catch (Exception ex) { Console.Error.WriteLine($"[BibleService] Échec chargement index: {ex.Message}"); }
        return _index ?? [];
    }

    public async Task<Livre?> GetLivreAsync(string slug)
    {
        slug = slug.ToLowerInvariant();

        if (_cache.TryGetValue(slug, out var livre))
            return livre;

        // Try IndexedDB cache first (faster than HTTP)
        try
        {
            await _db.InitializeAsync();
            var cached = await _db.GetAsync<CachedBook>("books", slug);
            if (cached?.data != null)
            {
                livre = System.Text.Json.JsonSerializer.Deserialize<Livre>(cached.data);
                if (livre != null)
                {
                    _cache[slug] = livre;
                    return livre;
                }
            }
        }
        catch (Exception ex) { Console.Error.WriteLine($"[BibleService] Échec cache IndexedDB pour {slug}: {ex.Message}"); }

        try
        {
            livre = await _http.GetFromJsonAsync<Livre>($"data/books/{slug}.json");

            if (livre != null && string.IsNullOrEmpty(livre.Abreviation))
            {
                var idx = await GetIndexAsync();
                var entry = idx.FirstOrDefault(i => i.Slug == slug);
                if (entry != null)
                    livre.Abreviation = entry.Abreviation;
            }

            if (livre != null)
            {
                _cache[slug] = livre;
                _ = CacheEnIndexedDbAsync(slug, livre);
            }

            return livre;
        }
        catch
        {
            return null;
        }
    }

    private class CachedBook
    {
        public string id { get; set; } = "";
        public string data { get; set; } = "";
    }

    private async Task CacheEnIndexedDbAsync(string slug, Livre livre)
    {
        try
        {
            await _db.InitializeAsync();
            await _db.PutAsync("books", slug, new { id = slug, data = System.Text.Json.JsonSerializer.Serialize(livre) });
        }
        catch (Exception ex) { Console.Error.WriteLine($"[BibleService] Échec cache IndexedDB pour {slug}: {ex.Message}"); }
    }

    public async Task<Chapitre?> GetChapitreAsync(string slug, int chapitre)
    {
        var livre = await GetLivreAsync(slug);
        return livre?.ContenuChapitre.FirstOrDefault(c => c.Numero == chapitre);
    }

    public async Task<Verset?> GetVersetAsync(string slug, int chapitre, int verset)
    {
        var chap = await GetChapitreAsync(slug, chapitre);
        return chap?.ContenuVersets.FirstOrDefault(v => v.Numero == verset);
    }

    public async Task<string?> ResolveReferenceAsync(string reference)
    {
        reference = reference.Trim();

        var parts = reference.Split(':');
        if (parts.Length != 2) return null;

        var livrePart = parts[0].Trim();
        var chapitreVerset = parts[1].Trim();

        var slug = await ResolveSlugAsync(livrePart);

        // If livrePart contains the chapter number (e.g. "Matthieu 5" in "Matthieu 5:7")
        if (slug == null && livrePart.Contains(' '))
        {
            var words = livrePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1 && int.TryParse(words[^1], out var extraCh))
            {
                var bookName = string.Join(" ", words[..^1]);
                slug = await ResolveSlugAsync(bookName);
                if (slug != null)
                {
                    var vers = chapitreVerset.Split(',')[0].Split('.');
                    if (int.TryParse(vers[0], out var extraV))
                    {
                        return $"/lecture/{slug}/{extraCh}/{extraV}";
                    }
                    return $"/lecture/{slug}/{extraCh}";
                }
            }
        }

        if (slug == null) return null;

        var cv = chapitreVerset.Split(',')[0].Split('.');
        if (!int.TryParse(cv[0], out var chapitre)) return null;
        int? verset = cv.Length > 1 && int.TryParse(cv[1], out var v) ? v : null;

        return verset.HasValue
            ? $"/lecture/{slug}/{chapitre}/{verset}"
            : $"/lecture/{slug}/{chapitre}";
    }

    public async Task<string?> ResolveSlugAsync(string input)
    {
        input = input.Trim().ToLowerInvariant();

        var index = await GetIndexAsync();

        var exact = index.FirstOrDefault(i =>
            i.Slug == input ||
            i.NomLivre.ToLowerInvariant() == input);
        if (exact != null) return exact.Slug;

        if (Livre.Abreviations.TryGetValue(input, out var nomLivre))
        {
            var found = index.FirstOrDefault(i =>
                i.NomLivre.Equals(nomLivre, StringComparison.OrdinalIgnoreCase));
            if (found != null) return found.Slug;
        }

        var fuzzy = index.FirstOrDefault(i =>
            i.NomLivre.StartsWith(input, StringComparison.OrdinalIgnoreCase) ||
            (i.Abreviation ?? "").StartsWith(input, StringComparison.OrdinalIgnoreCase));
        if (fuzzy != null) return fuzzy.Slug;

        var leven = index
            .Select(i => new { Item = i, Dist = Levenshtein(input, i.Slug) })
            .OrderBy(x => x.Dist)
            .FirstOrDefault(x => x.Dist <= 3);
        if (leven != null) return leven.Item.Slug;

        return null;
    }

    private static int Levenshtein(string a, string b)
    {
        var m = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) m[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) m[0, j] = j;
        for (int i = 1; i <= a.Length; i++)
            for (int j = 1; j <= b.Length; j++)
                m[i, j] = Math.Min(
                    Math.Min(m[i - 1, j] + 1, m[i, j - 1] + 1),
                    m[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
        return m[a.Length, b.Length];
    }

    public async Task<(Livre Livre, Chapitre Chapitre)?> GetAdjacentAsync(string slug, int chapitre, int direction)
    {
        var livre = await GetLivreAsync(slug);
        if (livre == null) return null;

        var idx = livre.ContenuChapitre.FindIndex(c => c.Numero == chapitre);
        if (idx == -1) return null;

        var newIdx = idx + direction;
        if (newIdx >= 0 && newIdx < livre.ContenuChapitre.Count)
            return (livre, livre.ContenuChapitre[newIdx]);

        var index = await GetIndexAsync();
        var currentIdx = index.FindIndex(i => i.Slug == slug);
        if (currentIdx == -1) return null;

        var nextIdx = currentIdx + direction;
        if (nextIdx < 0 || nextIdx >= index.Count) return null;

        var nextLivre = await GetLivreAsync(index[nextIdx].Slug);
        if (nextLivre == null) return null;

        var targetChap = direction > 0
            ? nextLivre.ContenuChapitre.FirstOrDefault()
            : nextLivre.ContenuChapitre.LastOrDefault();

        return targetChap != null ? (nextLivre, targetChap) : null;
    }

    public async Task<(Livre Livre, Chapitre Chapitre, Verset Verset)?> GetVersetDuJourAsync()
    {
        var index = await GetIndexAsync();
        var seed = DateTime.UtcNow.Date.Ticks;
        var rng = new Random((int)(seed % int.MaxValue));

        var bookIdx = rng.Next(index.Count);
        var slug = index[bookIdx].Slug;

        var livre = await GetLivreAsync(slug);
        if (livre == null || livre.ContenuChapitre.Count == 0) return null;

        var chapIdx = rng.Next(livre.ContenuChapitre.Count);
        var chapitre = livre.ContenuChapitre[chapIdx];
        if (chapitre.ContenuVersets.Count == 0) return null;

        var versetIdx = rng.Next(chapitre.ContenuVersets.Count);
        return (livre, chapitre, chapitre.ContenuVersets[versetIdx]);
    }
}
