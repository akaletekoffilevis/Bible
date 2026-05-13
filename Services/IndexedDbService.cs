using Microsoft.JSInterop;

namespace BibleApp.Services;

public class IndexedDbService
{
    private readonly IJSRuntime _js;
    private const string DbName = "BibleAppDB";
    private const int DbVersion = 2;
    private bool _initialized;

    public IndexedDbService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        await _js.InvokeVoidAsync("bibleDb.initialize", DbName, DbVersion);
        _initialized = true;
    }

    public async Task<List<T>> GetAllAsync<T>(string storeName)
    {
        return await _js.InvokeAsync<List<T>>("bibleDb.getAll", storeName);
    }

    public async Task PutAsync<T>(string storeName, string id, T value)
    {
        await _js.InvokeVoidAsync("bibleDb.put", storeName, id, value);
    }

    public async Task DeleteAsync(string storeName, string id)
    {
        await _js.InvokeVoidAsync("bibleDb.delete", storeName, id);
    }

    public async Task<List<Models.Highlight>> GetHighlightsAsync()
    {
        try { return await GetAllAsync<Models.Highlight>("highlights"); }
        catch { return new List<Models.Highlight>(); }
    }

    public async Task<Models.Highlight?> GetHighlightAsync(string verseId)
    {
        try
        {
            return await _js.InvokeAsync<Models.Highlight?>("bibleUtils.getHighlight", verseId);
        }
        catch { return null; }
    }

    public async Task SaveHighlightAsync(Models.Highlight highlight)
    {
        await PutAsync("highlights", highlight.Id, highlight);
    }

    public async Task DeleteHighlightAsync(string verseId)
    {
        await DeleteAsync("highlights", verseId);
    }
}
