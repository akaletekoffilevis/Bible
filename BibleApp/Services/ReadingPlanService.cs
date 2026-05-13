using BibleApp.Models;

namespace BibleApp.Services;

public class LectureQuotidienne
{
    public int Jour { get; set; }
    public string Titre { get; set; } = "";
    public List<LectureRef> Lectures { get; set; } = new();
    public bool EstFait { get; set; }
}

public class LectureRef
{
    public string Slug { get; set; } = "";
    public string Livre { get; set; } = "";
    public int Chapitre { get; set; }
    public string Apercu { get; set; } = "";
    public string Url => $"/lecture/{Slug}/{Chapitre}";
}

public class ReadingPlanService
{
    private readonly BibleService _bible;
    private List<LectureQuotidienne>? _plan;

    public ReadingPlanService(BibleService bible)
    {
        _bible = bible;
    }

    public async Task<List<LectureQuotidienne>> GetPlanAsync()
    {
        if (_plan != null) return _plan;
        _plan = await GenererPlanAsync();
        return _plan;
    }

    public int JourActuel
    {
        get
        {
            var debut = new DateTime(DateTime.UtcNow.Year, 1, 1);
            return (DateTime.UtcNow - debut).Days + 1;
        }
    }

    public LectureQuotidienne? LectureDuJour => _plan?.FirstOrDefault(p => p.Jour == JourActuel);

    public string ProgressionTexte(int faits) => $"{faits}/365 jours";

    private async Task<List<LectureQuotidienne>> GenererPlanAsync()
    {
        var plan = new List<LectureQuotidienne>();
        var index = await _bible.GetIndexAsync();
        if (index.Count == 0) return plan;

        var slugToLivre = index.ToDictionary(i => i.Slug, i => i.NomLivre);

        var slugsOT = index.Where(i => i.Testament.StartsWith("Ancien")).Select(i => i.Slug).ToList();
        var slugsNT = index.Where(i => i.Testament.StartsWith("Nouveau")).Select(i => i.Slug).ToList();

        var chapitresOT = new List<(string Slug, string Livre, int Chap)>();
        var chapitresNT = new List<(string Slug, string Livre, int Chap)>();
        var chapitresPs = new List<(string Slug, string Livre, int Chap)>();

        foreach (var slug in slugsOT)
        {
            var nbChapitres = index.FirstOrDefault(i => i.Slug == slug)?.NombreChapitres ?? 0;
            if (nbChapitres == 0) continue;
            var nom = slugToLivre.GetValueOrDefault(slug, slug);
            for (int c = 1; c <= nbChapitres; c++)
            {
                if (slug == "psaumes")
                    chapitresPs.Add((slug, nom, c));
                else
                    chapitresOT.Add((slug, nom, c));
            }
        }

        foreach (var slug in slugsNT)
        {
            var nbChapitres = index.FirstOrDefault(i => i.Slug == slug)?.NombreChapitres ?? 0;
            if (nbChapitres == 0) continue;
            var nom = slugToLivre.GetValueOrDefault(slug, slug);
            for (int c = 1; c <= nbChapitres; c++)
                chapitresNT.Add((slug, nom, c));
        }

        var otIdx = 0;
        var ntIdx = 0;
        var psIdx = 0;

        for (int jour = 1; jour <= 365; jour++)
        {
            var lectures = new List<LectureRef>();

            if (jour % 3 == 0 && psIdx < chapitresPs.Count)
            {
                var p = chapitresPs[psIdx++];
                lectures.Add(new LectureRef { Slug = p.Slug, Livre = p.Livre, Chapitre = p.Chap });
            }

            if (otIdx < chapitresOT.Count)
            {
                var ot = chapitresOT[otIdx++];
                lectures.Add(new LectureRef { Slug = ot.Slug, Livre = ot.Livre, Chapitre = ot.Chap });
                if (jour % 2 == 0 && otIdx < chapitresOT.Count)
                {
                    var ot2 = chapitresOT[otIdx++];
                    lectures.Add(new LectureRef { Slug = ot2.Slug, Livre = ot2.Livre, Chapitre = ot2.Chap });
                }
            }

            if (ntIdx < chapitresNT.Count)
            {
                var nt = chapitresNT[ntIdx++];
                lectures.Add(new LectureRef { Slug = nt.Slug, Livre = nt.Livre, Chapitre = nt.Chap });
            }

            if (lectures.Count == 0)
            {
                if (psIdx < chapitresPs.Count)
                {
                    var p = chapitresPs[psIdx++];
                    lectures.Add(new LectureRef { Slug = p.Slug, Livre = p.Livre, Chapitre = p.Chap });
                }
            }

            var titre = jour == 1 ? "Commencement" : $"Jour {jour}";
            plan.Add(new LectureQuotidienne
            {
                Jour = jour,
                Titre = titre,
                Lectures = lectures
            });
        }

        return plan;
    }
}
