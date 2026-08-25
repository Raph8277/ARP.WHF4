using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ReferentielsController : ControllerBase
{
    private readonly Wfrp4DbContext _db;

    public ReferentielsController(Wfrp4DbContext db)
    {
        _db = db;
    }

    [HttpGet("especes")]
    public async Task<ActionResult<List<EspeceDto>>> GetEspeces()
    {
        var especes = await _db.Especes
            .AsNoTracking()
            .OrderBy(e => e.Nom)
            .ToListAsync();

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        return Ok(especes.Select(e => new EspeceDto
        {
            Id = e.Id,
            Code = e.Code,
            Nom = e.Nom,
            MouvementBase = e.MouvementBase,
            CaracInitiales = !string.IsNullOrWhiteSpace(e.CaracInitiales)
                ? JsonSerializer.Deserialize<Dictionary<string, int>>(e.CaracInitiales) ?? new Dictionary<string, int>()
                : new Dictionary<string, int>(),
            Description = e.Description,
            TraitsPhysiques = !string.IsNullOrWhiteSpace(e.TraitsPhysiques)
                ? JsonSerializer.Deserialize<EspeceTraitsDto>(e.TraitsPhysiques, jsonOptions)
                : null,
        }).ToList());
    }

    [HttpGet("classes")]
    public async Task<ActionResult<List<ClasseDto>>> GetClasses()
    {
        var classes = await _db.Classes
            .AsNoTracking()
            .Include(c => c.Carrieres)
            .Select(c => new ClasseDto
            {
                Id = c.Id,
                Code = c.Code,
                Nom = c.Nom,
                Carrieres = c.Carrieres.Select(ca => new CarriereDto
                {
                    Id = ca.Id,
                    Code = ca.Code,
                    Nom = ca.Nom,
                    ClasseId = ca.ClasseId,
                    ClasseNom = c.Nom,
                    EspecesAutorisees = ParseCodes(ca.EspecesAutorisees),
                }).OrderBy(ca => ca.Nom).ToList(),
            })
            .OrderBy(c => c.Nom)
            .ToListAsync();

        return Ok(classes);
    }

    [HttpGet("carrieres")]
    public async Task<ActionResult<List<CarriereDto>>> GetCarrieres()
    {
        var carrieres = await _db.Carrieres
            .AsNoTracking()
            .Include(c => c.Classe)
            .Include(c => c.Niveaux)
            .OrderBy(c => c.Classe.Nom).ThenBy(c => c.Nom)
            .ToListAsync();

        return Ok(carrieres.Select(c => new CarriereDto
        {
            Id = c.Id,
            Code = c.Code,
            Nom = c.Nom,
            ClasseId = c.ClasseId,
            ClasseNom = c.Classe.Nom,
            EspecesAutorisees = ParseCodes(c.EspecesAutorisees),
            Niveaux = c.Niveaux.OrderBy(n => n.Niveau).Select(n => new NiveauCarriereDto
            {
                Id = n.Id,
                Niveau = n.Niveau,
                Intitule = n.Intitule,
                Statut = n.Statut,
                StatutNumerique = n.StatutNumerique,
                CompetenceCodes = ParseCodes(n.CompetenceRevenu),
                TalentCodes = ParseCodes(n.TalentsRevenu),
                Dotations = ParseDotations(n.Dotations),
            }).ToList(),
        }).ToList());
    }

    private static List<string> ParseCodes(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? []
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static List<string> ParseDotations(string? pipe) =>
        string.IsNullOrWhiteSpace(pipe)
            ? []
            : pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    [HttpGet("competences")]
    public async Task<ActionResult<List<CompetenceDto>>> GetCompetences()
    {
        var competences = await _db.Competences
            .AsNoTracking()
            .Select(c => new CompetenceDto
            {
                Id = c.Id,
                Code = c.Code,
                Nom = c.Nom,
                Caracteristique = c.Caracteristique,
                EstAvancee = c.EstAvancee,
                EstGroupee = c.EstGroupee,
                Description = c.Description,
            })
            .OrderBy(c => c.Nom)
            .ToListAsync();

        return Ok(competences);
    }

    [HttpGet("talents")]
    public async Task<ActionResult<List<TalentDto>>> GetTalents()
    {
        var talents = await _db.Talents
            .AsNoTracking()
            .Select(t => new TalentDto
            {
                Id = t.Id,
                Code = t.Code,
                Nom = t.Nom,
                MaxFois = t.MaxFois,
                Empilable = t.Empilable,
                Description = t.Description,
                Effet = t.Effet,
            })
            .OrderBy(t => t.Nom)
            .ToListAsync();

        return Ok(talents);
    }

    [HttpGet("armes")]
    public async Task<ActionResult<List<ArmeReferenceDto>>> GetArmes()
    {
        var armes = await _db.ArmesReference
            .AsNoTracking()
            .Select(a => new ArmeReferenceDto
            {
                Id = a.Id,
                Code = a.Code,
                Nom = a.Nom,
                Groupe = a.Groupe,
                TypeArme = a.TypeArme,
                Prix = a.Prix,
                Encombrement = a.Encombrement,
                Dommage = a.Dommage,
                Disponibilite = a.Disponibilite,
                Longueur = a.Longueur,
                Portee = a.Portee,
                Qualites = a.Qualites,
                Defauts = a.Defauts,
                DeuxMains = a.DeuxMains,
            })
            .OrderBy(a => a.TypeArme).ThenBy(a => a.Groupe).ThenBy(a => a.Nom)
            .ToListAsync();

        return Ok(armes);
    }

    [HttpGet("sorts")]
    public async Task<ActionResult<List<SortReferenceDto>>> GetSorts()
    {
        var sorts = await _db.SortsReference
            .AsNoTracking()
            .Select(s => new SortReferenceDto
            {
                Id = s.Id,
                Code = s.Code,
                Nom = s.Nom,
                Categorie = s.Categorie,
                Domaine = s.Domaine,
                Cn = s.Cn,
                Portee = s.Portee,
                Cible = s.Cible,
                Duree = s.Duree,
                Resume = s.Resume,
            })
            .OrderBy(s => s.Categorie)
            .ThenBy(s => s.Domaine)
            .ThenBy(s => s.Nom)
            .ToListAsync();

        return Ok(sorts);
    }

    [HttpGet("titres/bases")]
    public async Task<ActionResult<List<TitreBaseReferenceDto>>> GetTitresBase()
    {
        var titres = await _db.TitresBaseReference
            .AsNoTracking()
            .Select(t => new TitreBaseReferenceDto
            {
                Id = t.Id,
                Code = t.Code,
                Libelle = t.Libelle,
                Ordre = t.Ordre,
                NiveauMaitrise = t.NiveauMaitrise,
            })
            .OrderBy(t => t.NiveauMaitrise)
            .ThenBy(t => t.Ordre)
            .ThenBy(t => t.Libelle)
            .ToListAsync();

        return Ok(titres);
    }

    [HttpGet("titres/qualificatifs")]
    public async Task<ActionResult<List<TitreQualificatifReferenceDto>>> GetTitresQualificatifs()
    {
        var titres = await _db.TitresQualificatifReference
            .AsNoTracking()
            .Select(t => new TitreQualificatifReferenceDto
            {
                Id = t.Id,
                Code = t.Code,
                Libelle = t.Libelle,
                Ordre = t.Ordre,
                NiveauMaitrise = t.NiveauMaitrise,
            })
            .OrderBy(t => t.NiveauMaitrise)
            .ThenBy(t => t.Ordre)
            .ThenBy(t => t.Libelle)
            .ToListAsync();

        return Ok(titres);
    }
}
