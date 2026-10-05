using System.ComponentModel.DataAnnotations;
using Wfrp4.Shared.Models;

namespace Wfrp4.Shared.DTOs;

public class PartieSummaryDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = null!;
    public string? Description { get; set; }
    public TypePartie Type { get; set; }
    public StatutPartie Statut { get; set; }
    public string MjKeycloakId { get; set; } = null!;
    public string MjNom { get; set; } = null!;
    public int NombreMembres { get; set; }
    public int NombrePersonnagesChoisis { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PartieDetailDto : PartieSummaryDto
{
    public List<PartieMembreDto> Membres { get; set; } = new();
}

public class PartieMembreDto
{
    public int Id { get; set; }
    public string JoueurKeycloakId { get; set; } = null!;
    public string JoueurNom { get; set; } = null!;
    public int? PersonnageId { get; set; }
    public string? PersonnageNom { get; set; }
    public string? PersonnageEspece { get; set; }
    public string? PersonnageCarriere { get; set; }
    public bool PersonnageActif { get; set; }
    public DateTime AjouteLe { get; set; }
}

public class EnregistrerPartieRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Nom { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public TypePartie Type { get; set; } = TypePartie.Aventure;
    public StatutPartie Statut { get; set; } = StatutPartie.Preparation;
}

public class AjouterMembreRequest
{
    [Required]
    public string JoueurKeycloakId { get; set; } = null!;
}

public class ChoisirPersonnageRequest
{
    /// <summary>null retire le personnage choisi.</summary>
    public int? PersonnageId { get; set; }
}

/// <summary>Participation vue par le joueur.</summary>
public class ParticipationDto
{
    public int PartieId { get; set; }
    public string PartieNom { get; set; } = null!;
    public string? Description { get; set; }
    public TypePartie Type { get; set; }
    public StatutPartie Statut { get; set; }
    public string MjNom { get; set; } = null!;
    public int? PersonnageId { get; set; }
    public string? PersonnageNom { get; set; }
    public DateTime AjouteLe { get; set; }
}

/// <summary>Résumé d'un compte visible par un MJ pour composer une partie.</summary>
public class UtilisateurResumeDto
{
    public string Id { get; set; } = null!;
    public string NomUtilisateur { get; set; } = null!;
    public string? Email { get; set; }
}
