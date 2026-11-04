using BibleApp.Models;

namespace BibleApp.Services;

public class MemorisationService
{
    private readonly IndexedDbService _db;
    private const string Store = "memorisation";

    public MemorisationService(IndexedDbService db) => _db = db;

    public async Task<List<MemorisationCarte>> GetAllAsync()
    {
        try
        {
            await _db.InitializeAsync();
            var all = await _db.GetAllAsync<MemorisationCarte>(Store);
            return all ?? new();
        }
        catch { return new(); }
    }

    public async Task<MemorisationCarte?> GetAsync(string id)
    {
        try
        {
            await _db.InitializeAsync();
            return await _db.GetAsync<MemorisationCarte>(Store, id);
        }
        catch { return null; }
    }

    public async Task AjouterAsync(MemorisationCarte carte)
    {
        await _db.InitializeAsync();
        if (carte.ProchaineRevision == null) carte.ProchaineRevision = DateTime.UtcNow;
        await _db.PutAsync(Store, carte.Id, carte);
    }

    public async Task SupprimerAsync(string id)
    {
        await _db.InitializeAsync();
        await _db.DeleteAsync(Store, id);
    }

    /// <summary>Applique l'algorithme SM-2. quality in [0..5].</summary>
    public async Task ReviserAsync(MemorisationCarte carte, int quality)
    {
        quality = Math.Clamp(quality, 0, 5);
        if (quality < 3)
        {
            carte.Repetitions = 0;
            carte.Intervalle = 1;
        }
        else
        {
            if (carte.Repetitions == 0) carte.Intervalle = 1;
            else if (carte.Repetitions == 1) carte.Intervalle = 6;
            else carte.Intervalle = (int)Math.Round(carte.Intervalle * carte.Ease);
            carte.Repetitions++;
        }
        carte.Ease = carte.Ease + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
        if (carte.Ease < 1.3) carte.Ease = 1.3;
        carte.ProchaineRevision = DateTime.UtcNow.AddDays(Math.Max(1, carte.Intervalle));
        carte.DerniereRevision = DateTime.UtcNow;
        carte.TotalRevisions++;
        if (quality >= 3) carte.Reussites++;
        await _db.InitializeAsync();
        await _db.PutAsync(Store, carte.Id, carte);
    }

    public async Task<int> CountDueAsync()
    {
        var all = await GetAllAsync();
        return all.Count(c => c.EstDue);
    }
}