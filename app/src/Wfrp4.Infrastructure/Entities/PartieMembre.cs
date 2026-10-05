namespace Wfrp4.Infrastructure.Entities;

public class PartieMembre
{
    public int Id { get; set; }
    public int PartieId { get; set; }
    public Partie Partie { get; set; } = null!;
    public string JoueurKeycloakId { get; set; } = null!;
    public string JoueurNom { get; set; } = null!;
    public int? PersonnageId { get; set; }
    public Personnage? Personnage { get; set; }
    public DateTime AjouteLe { get; set; }
}
