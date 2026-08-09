namespace Wfrp4.Infrastructure.Entities;

public class PersonnageCompetence
{
    public int PersonnageId { get; set; }
    public int CompetenceId { get; set; }
    public Competence Competence { get; set; } = null!;
    public int Avances { get; set; }
}
