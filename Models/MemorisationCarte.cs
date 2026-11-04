namespace BibleApp.Models;

public class MemorisationCarte
{
    public string Id { get; set; } = string.Empty;          // slug-chapitre-verset
    public string Slug { get; set; } = string.Empty;
    public string NomLivre { get; set; } = string.Empty;
    public int Chapitre { get; set; }
    public int Verset { get; set; }
    public string Texte { get; set; } = string.Empty;
    public string Reference => $"{NomLivre} {Chapitre}:{Verset}";

    // SM-2 state
    public int Repetitions { get; set; } = 0;
    public double Ease { get; set; } = 2.5;
    public int Intervalle { get; set; } = 0;                 // jours
    public DateTime? ProchaineRevision { get; set; } = null; // null = à réviser maintenant
    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
    public DateTime? DerniereRevision { get; set; }
    public int TotalRevisions { get; set; } = 0;
    public int Reussites { get; set; } = 0;

    public bool EstDue => ProchaineRevision == null || ProchaineRevision.Value <= DateTime.UtcNow;
}