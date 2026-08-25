using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;
using Wfrp4.Server.Filters;
using Wfrp4.Server.Services;
using Wfrp4.Shared.DTOs;
using Wfrp4.Shared.Models;

namespace Wfrp4.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Joueur")]
public class PersonnagesController : ControllerBase
{
    private readonly Wfrp4DbContext _db;
    private readonly PersonnageService _personnageService;

    public PersonnagesController(Wfrp4DbContext db, PersonnageService personnageService)
    {
        _db = db;
        _personnageService = personnageService;
    }

    private string GetKeycloakId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    private bool IsOwnerOrAdmin(string ownerKeycloakId)
    {
        var keycloakId = GetKeycloakId();
        return User.IsInRole("wfrp4-admin") || ownerKeycloakId == keycloakId;
    }

    [HttpGet]
    public async Task<ActionResult<List<PersonnageSummaryDto>>> GetMesPersonnages()
    {
        var keycloakId = GetKeycloakId();

        var personnages = await _db.Personnages
            .AsNoTracking()
            .Include(p => p.Espece)
            .Include(p => p.CarriereCourante)
            .Where(p => p.KeycloakId == keycloakId)
            .Select(p => new PersonnageSummaryDto
            {
                Id = p.Id,
                Nom = p.Nom,
                Genre = p.Genre,
                EspeceNom = p.Espece.Nom,
                CarriereCouranteIntitule = p.CarriereCourante != null ? p.CarriereCourante.Intitule : null,
                StatutTier = p.CarriereCourante != null ? p.CarriereCourante.Statut : null,
                StatutNumerique = p.CarriereCourante != null ? p.CarriereCourante.StatutNumerique : null,
                XpTotal = p.XpTotal,
                XpDepense = p.XpDepense,
                EstActif = p.EstActif,
                EstPartage = false,
                CreatedAt = p.CreatedAt,
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(personnages);
    }

    [HttpGet("partages-recues")]
    [Authorize(Policy = "MaitreJeu")]
    public async Task<ActionResult<List<PersonnageSummaryDto>>> GetPersonnagesPartages()
    {
        var keycloakId = GetKeycloakId();

        var personnages = await _db.PersonnagePartages
            .AsNoTracking()
            .Include(pp => pp.Personnage).ThenInclude(p => p.Espece)
            .Include(pp => pp.Personnage).ThenInclude(p => p.CarriereCourante)
            .Where(pp => pp.MjKeycloakId == keycloakId && (pp.ExpiresAt == null || pp.ExpiresAt > DateTime.UtcNow))
            .Select(pp => new PersonnageSummaryDto
            {
                Id = pp.Personnage.Id,
                Nom = pp.Personnage.Nom,
                Genre = pp.Personnage.Genre,
                EspeceNom = pp.Personnage.Espece.Nom,
                CarriereCouranteIntitule = pp.Personnage.CarriereCourante != null ? pp.Personnage.CarriereCourante.Intitule : null,
                StatutTier = pp.Personnage.CarriereCourante != null ? pp.Personnage.CarriereCourante.Statut : null,
                StatutNumerique = pp.Personnage.CarriereCourante != null ? pp.Personnage.CarriereCourante.StatutNumerique : null,
                XpTotal = pp.Personnage.XpTotal,
                XpDepense = pp.Personnage.XpDepense,
                EstActif = pp.Personnage.EstActif,
                EstPartage = true,
                PermissionPartage = pp.Permission,
                CreatedAt = pp.Personnage.CreatedAt,
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(personnages);
    }

    [HttpGet("{id}")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<ActionResult<PersonnageDetailDto>> GetPersonnage(int id)
    {
        var personnage = await _db.Personnages
            .AsNoTracking()
            .Include(p => p.Espece)
            .Include(p => p.CarriereCourante)
            .Include(p => p.TitreBaseReference)
            .Include(p => p.TitreQualificatifReference)
            .Include(p => p.Caracteristiques)
            .Include(p => p.Competences).ThenInclude(c => c.Competence)
            .Include(p => p.Talents).ThenInclude(t => t.Talent)
            .Include(p => p.Carrieres).ThenInclude(c => c.NiveauCarriere).ThenInclude(n => n.Carriere).ThenInclude(c => c.Classe)
            .Include(p => p.HistoriqueXP)
            .Include(p => p.Possessions)
            .Include(p => p.Sorts).ThenInclude(s => s.SortReference)
            .Include(p => p.Parchemins).ThenInclude(p => p.SortReference)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (personnage == null) return NotFound();

        var keycloakId = GetKeycloakId();
        var estProprietaire = personnage.KeycloakId == keycloakId;
        var estAdmin = User.IsInRole("wfrp4-admin");
        var partage = !estProprietaire && !estAdmin
            ? await _db.PersonnagePartages
                .AsNoTracking()
                .FirstOrDefaultAsync(pp => pp.PersonnageId == id
                    && pp.MjKeycloakId == keycloakId
                    && (pp.ExpiresAt == null || pp.ExpiresAt > DateTime.UtcNow))
            : null;

        var premiereCarriere = personnage.Carrieres
            .OrderBy(c => c.DateEntree)
            .FirstOrDefault();
        var dotationsRaw = premiereCarriere?.NiveauCarriere?.Dotations;
        var dotationsList = string.IsNullOrWhiteSpace(dotationsRaw)
            ? new List<string>()
            : dotationsRaw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var dto = new PersonnageDetailDto
        {
            Id = personnage.Id,
            Nom = personnage.Nom,
            Genre = personnage.Genre,
            EspeceId = personnage.EspeceId,
            EspeceNom = personnage.Espece.Nom,
            CarriereCouranteId = personnage.CarriereCouranteId,
            CarriereCouranteIntitule = personnage.CarriereCourante?.Intitule,
            ClasseNom = premiereCarriere?.NiveauCarriere?.Carriere?.Classe?.Nom,
            Dotations = dotationsList,
            XpTotal = personnage.XpTotal,
            XpDepense = personnage.XpDepense,
            BlessuresMax = personnage.BlessuresMax,
            Destin = personnage.Destin,
            Fortune = personnage.Fortune,
            Resilience = personnage.Resilience,
            Resolution = personnage.Resolution,
            Mouvement = personnage.Mouvement,
            CouronnesOr = personnage.CouronnesOr,
            PistolesArgent = personnage.PistolesArgent,
            SousCuivre = personnage.SousCuivre,
            Motivation = personnage.Motivation,
            TitreBaseReferenceId = personnage.TitreBaseReferenceId,
            TitreBaseLibelle = personnage.TitreBaseReference != null ? personnage.TitreBaseReference.Libelle : null,
            TitreQualificatifReferenceId = personnage.TitreQualificatifReferenceId,
            TitreQualificatifLibelle = personnage.TitreQualificatifReference != null ? personnage.TitreQualificatifReference.Libelle : null,
            AmbitionCourtTerme = personnage.AmbitionCourtTerme,
            AmbitionLongTerme = personnage.AmbitionLongTerme,
            GroupeNom = personnage.GroupeNom,
            GroupeMembres = personnage.GroupeMembres,
            Psychologie = personnage.Psychologie,
            CorruptionMutations = personnage.CorruptionMutations,
            StatutTier = personnage.CarriereCourante?.Statut,
            StatutNumerique = personnage.CarriereCourante?.StatutNumerique,
            Age = personnage.Age,
            CouleurYeux = personnage.CouleurYeux,
            CouleurCheveux = personnage.CouleurCheveux,
            TailleCm = personnage.TailleCm,
            EstActif = personnage.EstActif,
            EstProprietaire = estProprietaire || estAdmin,
            PeutModifier = estProprietaire || estAdmin,
            PeutAvancer = estProprietaire || estAdmin,
            PeutPartager = estProprietaire || estAdmin,
            PeutOctroyerXp = estAdmin || (User.IsInRole("wfrp4-maitre-jeu") && partage?.Permission == PermissionPartage.XP),
            Caracteristiques = personnage.Caracteristiques.Select(c => new CaracteristiqueDto
            {
                Code = c.Code,
                ValeurInitiale = c.ValeurInitiale,
                Avances = c.Avances,
            }).ToList(),
            Competences = personnage.Competences.Select(c => new PersonnageCompetenceDto
            {
                CompetenceId = c.CompetenceId,
                CompetenceNom = c.Competence.Nom,
                Caracteristique = c.Competence.Caracteristique,
                Avances = c.Avances,
            }).ToList(),
            Talents = personnage.Talents.Select(t => new PersonnageTalentDto
            {
                TalentId = t.TalentId,
                TalentNom = t.Talent.Nom,
                Fois = t.Fois,
            }).ToList(),
            Carrieres = personnage.Carrieres.Select(c => new PersonnageCarriereDto
            {
                NiveauCarriereId = c.NiveauCarriereId,
                CarriereNom = c.NiveauCarriere.Carriere.Nom,
                Intitule = c.NiveauCarriere.Intitule,
                Niveau = c.NiveauCarriere.Niveau,
                Statut = c.NiveauCarriere.Statut,
                StatutNumerique = c.NiveauCarriere.StatutNumerique,
                EstCourante = c.EstCourante,
                DateEntree = c.DateEntree,
                DateSortie = c.DateSortie,
            }).ToList(),
            HistoriqueXP = personnage.HistoriqueXP
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new HistoriqueXPDto
                {
                    Id = h.Id,
                    AuteurKeycloakId = h.AuteurKeycloakId,
                    Montant = h.Montant,
                    Type = h.Type,
                    Cible = h.Cible,
                    Notes = h.Notes,
                    CreatedAt = h.CreatedAt,
                }).ToList(),
            Possessions = personnage.Possessions.Select(p => new PossessionDto
            {
                Id = p.Id,
                Nom = p.Nom,
                Type = p.Type,
                Quantite = p.Quantite,
            }).ToList(),
            Sorts = personnage.Sorts
                .OrderBy(s => s.SortReference.Nom)
                .Select(s => new PersonnageSortDto
                {
                    Id = s.Id,
                    SortReferenceId = s.SortReferenceId,
                    SortNom = s.SortReference.Nom,
                    Categorie = s.SortReference.Categorie,
                    Domaine = s.SortReference.Domaine,
                    Cn = s.SortReference.Cn,
                    Portee = s.SortReference.Portee,
                    Cible = s.SortReference.Cible,
                    Duree = s.SortReference.Duree,
                    Resume = s.SortReference.Resume,
                }).ToList(),
            Parchemins = personnage.Parchemins
                .OrderBy(p => p.SortReference.Nom)
                .Select(p => new PersonnageParcheminDto
                {
                    Id = p.Id,
                    SortReferenceId = p.SortReferenceId,
                    SortNom = p.SortReference.Nom,
                    Categorie = p.SortReference.Categorie,
                    Domaine = p.SortReference.Domaine,
                    Cn = p.SortReference.Cn,
                    Portee = p.SortReference.Portee,
                    Cible = p.SortReference.Cible,
                    Duree = p.SortReference.Duree,
                    Resume = p.SortReference.Resume,
                    Quantite = p.Quantite,
                }).ToList(),
        };

        return Ok(dto);
    }

    [HttpGet("{id}/revenus")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<ActionResult<RevenusDto>> GetRevenus(int id, [FromServices] XPService xpService)
    {
        var personnage = await _db.Personnages
            .AsNoTracking()
            .Include(p => p.CarriereCourante)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (personnage == null) return NotFound();
        if (personnage.CarriereCourante == null)
            return Ok(new RevenusDto { Monnaie = "Aucun", Formule = "—" });

        var revenus = xpService.CalculerRevenus(
            personnage.CarriereCourante.Statut,
            personnage.CarriereCourante.StatutNumerique);

        return Ok(revenus);
    }

    [HttpGet("{id}/fiche-pdf")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> ExporterFichePdf(int id, [FromServices] CharacterSheetPdfService pdfService, CancellationToken ct)
    {
        var result = await pdfService.GenerateAsync(id, User, ct);
        return File(result.Content, "application/pdf", result.FileName);
    }

    [HttpPost("{id}/fiche-pdf/preview")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> PrevisualiserFichePdf(
        int id,
        PdfSheetLayoutDto layout,
        [FromServices] CharacterSheetPdfService pdfService,
        CancellationToken ct)
    {
        var result = await pdfService.GeneratePreviewAsync(id, User, layout, ct);
        return File(result.Content, "application/pdf", result.FileName);
    }

    [HttpGet("fiche-pdf/layout")]
    public async Task<ActionResult<PdfSheetLayoutDto>> GetFichePdfLayout([FromServices] CharacterSheetPdfService pdfService, CancellationToken ct)
    {
        return Ok(await pdfService.GetLayoutAsync(ct));
    }

    [HttpGet("fiche-pdf/template/{page:int}")]
    public async Task<IActionResult> GetFichePdfTemplate(
        int page,
        [FromServices] CharacterSheetPdfService pdfService,
        CancellationToken ct)
    {
        if (page is not 1 and not 2)
            return BadRequest(new { Error = "La page doit etre 1 ou 2." });

        return File(await pdfService.GetTemplatePageAsync(page, ct), "image/jpeg");
    }

    [HttpGet("fiche-pdf/layouts")]
    public async Task<ActionResult<IReadOnlyList<PdfSheetLayoutSummaryDto>>> GetFichePdfLayouts(
        [FromServices] CharacterSheetPdfService pdfService,
        CancellationToken ct)
    {
        return Ok(await pdfService.GetLayoutSummariesAsync(ct));
    }

    [HttpGet("fiche-pdf/layouts/{key}")]
    public async Task<ActionResult<PdfSheetLayoutDto>> GetFichePdfLayoutByKey(
        string key,
        [FromServices] CharacterSheetPdfService pdfService,
        CancellationToken ct)
    {
        return Ok(await pdfService.GetNamedLayoutAsync(key, ct));
    }

    [HttpPut("fiche-pdf/layout")]
    [Authorize(Roles = "wfrp4-admin")]
    public async Task<IActionResult> SaveFichePdfLayout(
        PdfSheetLayoutDto layout,
        [FromServices] CharacterSheetPdfService pdfService,
        CancellationToken ct)
    {
        await pdfService.SaveLayoutAsync(layout, ct);
        return NoContent();
    }

    [HttpPut("{id}")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<ActionResult<PersonnageDetailDto>> MettreAJourPersonnage(int id, UpdatePersonnageRequest request)
    {
        var personnage = await _db.Personnages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (personnage == null)
            return NotFound();

        if (!IsOwnerOrAdmin(personnage.KeycloakId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Nom))
            return BadRequest(new { Error = "Le nom est obligatoire." });

        if (request.TitreBaseReferenceId.HasValue != request.TitreQualificatifReferenceId.HasValue)
            return BadRequest(new { Error = "Le titre doit comporter une base et un qualificatif, ou rester vide." });

        var personnageAvecNiveau = await _db.Personnages
            .AsNoTracking()
            .Include(p => p.CarriereCourante)
            .FirstAsync(p => p.Id == id);
        var niveauMaitriseMax = personnageAvecNiveau.CarriereCourante?.Niveau ?? 1;

        if (request.TitreBaseReferenceId.HasValue)
        {
            var titreBaseValide = await _db.TitresBaseReference
                .AnyAsync(t => t.Id == request.TitreBaseReferenceId.Value && t.NiveauMaitrise <= niveauMaitriseMax);
            if (!titreBaseValide)
                return BadRequest(new { Error = "Titre de base inaccessible pour le niveau de maîtrise actuel." });
        }

        if (request.TitreQualificatifReferenceId.HasValue)
        {
            var titreQualificatifValide = await _db.TitresQualificatifReference
                .AnyAsync(t => t.Id == request.TitreQualificatifReferenceId.Value && t.NiveauMaitrise <= niveauMaitriseMax);
            if (!titreQualificatifValide)
                return BadRequest(new { Error = "Qualificatif de titre inaccessible pour le niveau de maîtrise actuel." });
        }

        await _personnageService.MettreAJourPersonnage(id, request);
        return await GetPersonnage(id);
    }

    [HttpPost]
    public async Task<ActionResult<PersonnageSummaryDto>> CreerPersonnage(CreatePersonnageRequest request)
    {
        var keycloakId = GetKeycloakId();
        var personnage = await _personnageService.CreerPersonnage(keycloakId, request);

        return CreatedAtAction(nameof(GetPersonnage), new { id = personnage.Id }, new PersonnageSummaryDto
        {
            Id = personnage.Id,
            Nom = personnage.Nom,
            Genre = personnage.Genre,
            EstPartage = false,
        });
    }

    [HttpDelete("{id}")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> SupprimerPersonnage(int id)
    {
        var keycloakId = GetKeycloakId();
        var personnage = await _db.Personnages.FirstOrDefaultAsync(p => p.Id == id && p.KeycloakId == keycloakId);

        if (personnage == null && !User.IsInRole("wfrp4-admin"))
            return Forbid();

        personnage ??= await _db.Personnages.FindAsync(id);
        if (personnage == null) return NotFound();

        _db.Personnages.Remove(personnage);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // --- Possessions ---

    [HttpPost("{id}/possessions")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<ActionResult<PossessionDto>> AjouterPossession(int id, AjoutPossessionRequest request)
    {
        var personnage = await _db.Personnages.FindAsync(id);
        if (personnage == null) return NotFound();
        if (!IsOwnerOrAdmin(personnage.KeycloakId)) return Forbid();

        var possession = new Infrastructure.Entities.PersonnagePossession
        {
            PersonnageId = id,
            Nom = request.Nom,
            Type = request.Type,
            Quantite = request.Quantite > 0 ? request.Quantite : 1,
        };

        _db.PersonnagePossessions.Add(possession);
        await _db.SaveChangesAsync();

        return Ok(new PossessionDto
        {
            Id = possession.Id,
            Nom = possession.Nom,
            Type = possession.Type,
            Quantite = possession.Quantite,
        });
    }

    [HttpDelete("{id}/possessions/{possessionId}")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> SupprimerPossession(int id, int possessionId)
    {
        var possession = await _db.PersonnagePossessions
            .FirstOrDefaultAsync(p => p.Id == possessionId && p.PersonnageId == id);

        if (possession == null) return NotFound();

        _db.PersonnagePossessions.Remove(possession);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // --- Sorts et parchemins ---

    [HttpPost("{id}/sorts")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> AjouterSort(int id, AjoutSortRequest request)
    {
        var personnage = await _db.Personnages
            .Include(p => p.Sorts)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (personnage == null) return NotFound();
        if (!IsOwnerOrAdmin(personnage.KeycloakId)) return Forbid();

        if (personnage.Sorts.Any(s => s.SortReferenceId == request.SortReferenceId))
            return BadRequest(new { Error = "Le personnage connaît déjà ce sort." });

        var sort = await _db.SortsReference.FindAsync(request.SortReferenceId);
        if (sort == null) return BadRequest(new { Error = "Sort introuvable." });

        personnage.Sorts.Add(new Infrastructure.Entities.PersonnageSort
        {
            SortReferenceId = request.SortReferenceId,
        });
        personnage.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}/sorts/{sortId}")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> SupprimerSort(int id, int sortId)
    {
        var sort = await _db.PersonnageSorts
            .Include(s => s.Personnage)
            .FirstOrDefaultAsync(s => s.Id == sortId && s.PersonnageId == id);

        if (sort == null) return NotFound();
        if (!IsOwnerOrAdmin(sort.Personnage.KeycloakId)) return Forbid();

        _db.PersonnageSorts.Remove(sort);
        sort.Personnage.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/parchemins")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> AjouterParchemin(int id, AjoutParcheminRequest request)
    {
        var personnage = await _db.Personnages
            .Include(p => p.Parchemins)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (personnage == null) return NotFound();
        if (!IsOwnerOrAdmin(personnage.KeycloakId)) return Forbid();

        var sort = await _db.SortsReference.FindAsync(request.SortReferenceId);
        if (sort == null) return BadRequest(new { Error = "Sort introuvable." });

        var quantite = request.Quantite > 0 ? request.Quantite : 1;
        var existant = personnage.Parchemins.FirstOrDefault(p => p.SortReferenceId == request.SortReferenceId);
        if (existant != null)
        {
            existant.Quantite += quantite;
        }
        else
        {
            personnage.Parchemins.Add(new Infrastructure.Entities.PersonnageParchemin
            {
                SortReferenceId = request.SortReferenceId,
                Quantite = quantite,
            });
        }

        personnage.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}/parchemins/{parcheminId}")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> SupprimerParchemin(int id, int parcheminId)
    {
        var parchemin = await _db.PersonnageParchemins
            .Include(p => p.Personnage)
            .FirstOrDefaultAsync(p => p.Id == parcheminId && p.PersonnageId == id);

        if (parchemin == null) return NotFound();
        if (!IsOwnerOrAdmin(parchemin.Personnage.KeycloakId)) return Forbid();

        _db.PersonnageParchemins.Remove(parchemin);
        parchemin.Personnage.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // --- Ajout compétence / talent ---

    [HttpPost("{id}/competences")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> AjouterCompetence(int id, AjoutCompetenceRequest request)
    {
        var personnage = await _db.Personnages
            .Include(p => p.Competences)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (personnage == null) return NotFound();
        if (!IsOwnerOrAdmin(personnage.KeycloakId)) return Forbid();

        if (personnage.Competences.Any(c => c.CompetenceId == request.CompetenceId))
            return BadRequest(new { Error = "Le personnage possède déjà cette compétence." });

        var competence = await _db.Competences.FindAsync(request.CompetenceId);
        if (competence == null) return BadRequest(new { Error = "Compétence introuvable." });

        personnage.Competences.Add(new Infrastructure.Entities.PersonnageCompetence
        {
            CompetenceId = request.CompetenceId,
            Avances = 0,
        });
        personnage.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("{id}/talents")]
    [ServiceFilter(typeof(PersonnageOwnerFilter))]
    public async Task<IActionResult> AjouterTalent(int id, AjoutTalentRequest request)
    {
        var personnage = await _db.Personnages
            .Include(p => p.Talents)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (personnage == null) return NotFound();
        if (!IsOwnerOrAdmin(personnage.KeycloakId)) return Forbid();

        var existant = personnage.Talents.FirstOrDefault(t => t.TalentId == request.TalentId);
        if (existant != null)
        {
            existant.Fois++;
        }
        else
        {
            var talent = await _db.Talents.FindAsync(request.TalentId);
            if (talent == null) return BadRequest(new { Error = "Talent introuvable." });

            personnage.Talents.Add(new Infrastructure.Entities.PersonnageTalent
            {
                TalentId = request.TalentId,
                Fois = 1,
            });
        }

        personnage.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok();
    }
}
