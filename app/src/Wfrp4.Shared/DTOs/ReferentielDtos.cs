using Wfrp4.Shared.Models;

namespace Wfrp4.Shared.DTOs;

public class EspeceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public int MouvementBase { get; set; }
    public Dictionary<string, int> CaracInitiales { get; set; } = [];
    public string? Description { get; set; }
    public EspeceTraitsDto? TraitsPhysiques { get; set; }
}

public class EspeceTraitsDto
{
    public FormulaDto Age { get; set; } = null!;
    public FormuleTailleDto Taille { get; set; } = null!;
    public List<TableEntryDto> CouleursYeux { get; set; } = [];
    public List<TableEntryDto> CouleursCheveux { get; set; } = [];
    public NomConfigDto Noms { get; set; } = null!;
    public bool DoubleTirageYeux { get; set; }
}

public class FormulaDto
{
    public int Base { get; set; }
    public int NbDes { get; set; }
    public int Multiplicateur { get; set; } = 1;
}

public class FormuleTailleDto
{
    public int BaseCm { get; set; }
    public int NbDes { get; set; }
    public double FacteurCm { get; set; }
    public bool DesSupSur10 { get; set; }
}

public class TableEntryDto
{
    public int Min { get; set; }
    public int Max { get; set; }
    public string Valeur { get; set; } = null!;
}

public class NomConfigDto
{
    public List<string> Prenoms { get; set; } = [];
    public List<string>? NomsFamille { get; set; }
    public List<string>? Epithetes { get; set; }
    public List<string>? SuffixesPatronymiques { get; set; }
    public List<string>? Clans { get; set; }
    public NomStructure Structure { get; set; }
    public List<string>? Elements1 { get; set; }
    public List<string>? Elements2 { get; set; }
    public List<string>? Terminaisons { get; set; }
}

public class ClasseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public List<CarriereDto> Carrieres { get; set; } = [];
}

public class CarriereDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public int ClasseId { get; set; }
    public string ClasseNom { get; set; } = null!;
    public List<string> EspecesAutorisees { get; set; } = [];
    public List<NiveauCarriereDto> Niveaux { get; set; } = [];
}

public class NiveauCarriereDto
{
    public int Id { get; set; }
    public int Niveau { get; set; }
    public string Intitule { get; set; } = null!;
    public StatutTier Statut { get; set; }
    public int StatutNumerique { get; set; }
    public List<string> CompetenceCodes { get; set; } = [];
    public List<string> TalentCodes { get; set; } = [];
    public List<string> Dotations { get; set; } = [];
}

public class CompetenceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public string Caracteristique { get; set; } = null!;
    public bool EstAvancee { get; set; }
    public bool EstGroupee { get; set; }
    public string? Description { get; set; }
}

public class TalentDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public int? MaxFois { get; set; }
    public bool Empilable { get; set; }
    public string? Description { get; set; }
    public string? Effet { get; set; }
}

public class ArmeReferenceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public string Groupe { get; set; } = null!;
    public string TypeArme { get; set; } = null!;
    public string? Prix { get; set; }
    public int Encombrement { get; set; }
    public string Dommage { get; set; } = null!;
    public string Disponibilite { get; set; } = null!;
    public string? Longueur { get; set; }
    public string? Portee { get; set; }
    public string? Qualites { get; set; }
    public string? Defauts { get; set; }
    public bool DeuxMains { get; set; }
}

public class SortReferenceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public string? Domaine { get; set; }
    public int? Cn { get; set; }
    public string? Portee { get; set; }
    public string? Cible { get; set; }
    public string? Duree { get; set; }
    public string? Resume { get; set; }
}

public class CreatureReferenceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Nom { get; set; } = null!;
    public string Categorie { get; set; } = null!;
    public int M { get; set; }
    public int CC { get; set; }
    public int CT { get; set; }
    public int F { get; set; }
    public int E { get; set; }
    public int I { get; set; }
    public int Ag { get; set; }
    public int Dex { get; set; }
    public int Int { get; set; }
    public int FM { get; set; }
    public int Soc { get; set; }
    public int B { get; set; }
    public string Traits { get; set; } = null!;
    public string? TraitsOptionnels { get; set; }
    public int Page { get; set; }
}

public class TitreBaseReferenceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Libelle { get; set; } = null!;
    public int Ordre { get; set; }
    public int NiveauMaitrise { get; set; }
}

public class TitreQualificatifReferenceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Libelle { get; set; } = null!;
    public int Ordre { get; set; }
    public int NiveauMaitrise { get; set; }
}
