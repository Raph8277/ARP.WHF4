using Wfrp4.Shared.Models;

namespace Wfrp4.Shared.DTOs;

public class PersonnageSummaryDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = null!;
    public GenrePersonnage Genre { get; set; }
    public string EspeceNom { get; set; } = null!;
    public string? CarriereCouranteIntitule { get; set; }
    public StatutTier? StatutTier { get; set; }
    public int? StatutNumerique { get; set; }
    public int XpTotal { get; set; }
    public int XpDepense { get; set; }
    public int XpRestant => XpTotal - XpDepense;
    public int AvancesCompetenceGratuitesRestantes { get; set; }
    public int TalentsGratuitsRestants { get; set; }
    public bool EstActif { get; set; }
    public bool EstPartage { get; set; }
    public PermissionPartage? PermissionPartage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PersonnageDetailDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = null!;
    public GenrePersonnage Genre { get; set; }
    public int EspeceId { get; set; }
    public string EspeceNom { get; set; } = null!;
    public int? CarriereCouranteId { get; set; }
    public string? CarriereCouranteIntitule { get; set; }
    public string? ClasseNom { get; set; }
    public List<string> Dotations { get; set; } = [];
    public int XpTotal { get; set; }
    public int XpDepense { get; set; }
    public int XpRestant => XpTotal - XpDepense;
    public int AvancesCompetenceGratuitesRestantes { get; set; }
    public int TalentsGratuitsRestants { get; set; }

    public int BlessuresMax { get; set; }
    public int Destin { get; set; }
    public int Fortune { get; set; }
    public int Resilience { get; set; }
    public int Resolution { get; set; }
    public int Mouvement { get; set; }

    public int CouronnesOr { get; set; }
    public int PistolesArgent { get; set; }
    public int SousCuivre { get; set; }

    public string? Motivation { get; set; }
    public int? TitreBaseReferenceId { get; set; }
    public string? TitreBaseLibelle { get; set; }
    public int? TitreQualificatifReferenceId { get; set; }
    public string? TitreQualificatifLibelle { get; set; }
    public string? TitreComplet =>
        string.IsNullOrWhiteSpace(TitreBaseLibelle) || string.IsNullOrWhiteSpace(TitreQualificatifLibelle)
            ? null
            : TitreGenreHelper.Assembler(TitreBaseLibelle, TitreQualificatifLibelle, Genre);
    public string? AmbitionCourtTerme { get; set; }
    public string? AmbitionLongTerme { get; set; }
    public string? GroupeNom { get; set; }
    public string? GroupeMembres { get; set; }
    public string? Psychologie { get; set; }
    public string? CorruptionMutations { get; set; }
    public StatutTier? StatutTier { get; set; }
    public int? StatutNumerique { get; set; }
    public int? Age { get; set; }
    public string? CouleurYeux { get; set; }
    public string? CouleurCheveux { get; set; }
    public int? TailleCm { get; set; }
    public bool EstActif { get; set; }
    public bool EstProprietaire { get; set; }
    public bool PeutModifier { get; set; }
    public bool PeutAvancer { get; set; }
    public bool PeutPartager { get; set; }
    public bool PeutOctroyerXp { get; set; }

    public List<CaracteristiqueDto> Caracteristiques { get; set; } = [];
    public List<PersonnageCompetenceDto> Competences { get; set; } = [];
    public List<PersonnageTalentDto> Talents { get; set; } = [];
    public List<PersonnageCarriereDto> Carrieres { get; set; } = [];
    public List<HistoriqueXPDto> HistoriqueXP { get; set; } = [];
    public List<PossessionDto> Possessions { get; set; } = [];
    public List<PersonnageSortDto> Sorts { get; set; } = [];
    public List<PersonnageParcheminDto> Parchemins { get; set; } = [];
}

public class CaracteristiqueDto
{
    public string Code { get; set; } = null!;
    public int ValeurInitiale { get; set; }
    public int Avances { get; set; }
    public int ValeurActuelle => ValeurInitiale + Avances;
}

public class PersonnageCompetenceDto
{
    public int CompetenceId { get; set; }
    public string CompetenceNom { get; set; } = null!;
    public string Caracteristique { get; set; } = null!;
    public bool EstAvancee { get; set; }
    public bool EstGroupee { get; set; }
    public int Avances { get; set; }
}

public class PersonnageTalentDto
{
    public int TalentId { get; set; }
    public string TalentNom { get; set; } = null!;
    public int Fois { get; set; }
}

public class PersonnageCarriereDto
{
    public int NiveauCarriereId { get; set; }
    public string CarriereNom { get; set; } = null!;
    public string Intitule { get; set; } = null!;
    public int Niveau { get; set; }
    public StatutTier Statut { get; set; }
    public int StatutNumerique { get; set; }
    public bool EstCourante { get; set; }
    public DateTime DateEntree { get; set; }
    public DateTime? DateSortie { get; set; }
}

public class PossessionDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = null!;
    public TypePossession Type { get; set; }
    public int Quantite { get; set; }
}

public class PersonnageSortDto
{
    public int Id { get; set; }
    public int SortReferenceId { get; set; }
    public string SortNom { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public string? Domaine { get; set; }
    public int? Cn { get; set; }
    public string? Portee { get; set; }
    public string? Cible { get; set; }
    public string? Duree { get; set; }
    public string? Resume { get; set; }
}

public class PersonnageParcheminDto
{
    public int Id { get; set; }
    public int SortReferenceId { get; set; }
    public string SortNom { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public string? Domaine { get; set; }
    public int? Cn { get; set; }
    public string? Portee { get; set; }
    public string? Cible { get; set; }
    public string? Duree { get; set; }
    public string? Resume { get; set; }
    public int Quantite { get; set; }
}

public class AjoutPossessionRequest
{
    public string Nom { get; set; } = null!;
    public TypePossession Type { get; set; }
    public int Quantite { get; set; } = 1;
}

public class AjoutCompetenceRequest
{
    public int CompetenceId { get; set; }
    public int NombrePoints { get; set; }
}

public class AjoutTalentRequest
{
    public int TalentId { get; set; }
    public int NombreFois { get; set; } = 1;
}

public class AjoutSortRequest
{
    public int SortReferenceId { get; set; }
}

public class AjoutParcheminRequest
{
    public int SortReferenceId { get; set; }
    public int Quantite { get; set; } = 1;
}

public class HistoriqueXPDto
{
    public int Id { get; set; }
    public string AuteurKeycloakId { get; set; } = null!;
    public int Montant { get; set; }
    public TypeXP Type { get; set; }
    public string? Cible { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
