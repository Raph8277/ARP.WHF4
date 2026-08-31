using Wfrp4.Shared.Models;

namespace Wfrp4.Infrastructure.Entities;

public class Personnage
{
    public int Id { get; set; }
    public string KeycloakId { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public GenrePersonnage Genre { get; set; } = GenrePersonnage.Masculin;

    public int EspeceId { get; set; }
    public Espece Espece { get; set; } = null!;

    public int? CarriereCouranteId { get; set; }
    public NiveauCarriere? CarriereCourante { get; set; }

    public int XpTotal { get; set; }
    public int XpDepense { get; set; }
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
    public TitreBaseReference? TitreBaseReference { get; set; }
    public int? TitreQualificatifReferenceId { get; set; }
    public TitreQualificatifReference? TitreQualificatifReference { get; set; }
    public string? AmbitionCourtTerme { get; set; }
    public string? AmbitionLongTerme { get; set; }
    public string? GroupeNom { get; set; }
    public string? GroupeMembres { get; set; }
    public string? Psychologie { get; set; }
    public string? CorruptionMutations { get; set; }
    [Obsolete("Utiliser CarriereCourante.Statut + CarriereCourante.StatutNumerique")]
    public string? StatutSocial { get; set; }
    public int? Age { get; set; }
    public string? CouleurYeux { get; set; }
    public string? CouleurCheveux { get; set; }
    public int? TailleCm { get; set; }
    public bool EstActif { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PersonnageCaracteristique> Caracteristiques { get; set; } = [];
    public ICollection<PersonnageCompetence> Competences { get; set; } = [];
    public ICollection<PersonnageTalent> Talents { get; set; } = [];
    public ICollection<HistoriqueXP> HistoriqueXP { get; set; } = [];
    public ICollection<PersonnageCarriere> Carrieres { get; set; } = [];
    public ICollection<PersonnagePartage> Partages { get; set; } = [];
    public ICollection<PersonnagePossession> Possessions { get; set; } = [];
    public ICollection<PersonnageSort> Sorts { get; set; } = [];
    public ICollection<PersonnageParchemin> Parchemins { get; set; } = [];
}
