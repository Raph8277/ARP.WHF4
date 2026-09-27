using System.ComponentModel.DataAnnotations;
using Wfrp4.Shared.DTOs;

namespace Wfrp4.Server.Services;

public static class MjPdfRequestValidator
{
    public const int MaxPages = 64;
    public static void Validate(MjPdfExportRequest request)
    {
        if (request == null || request.Sections == null || request.Sections.Count > 64)
            throw new ValidationException("Un export doit contenir au maximum 64 sections.");
        var characters = 0;
        void Text(string? value, int max)
        {
            if (value == null || value.Length > max)
                throw new ValidationException($"Un texte est absent ou dépasse {max} caractères.");
            characters += value.Length;
            if (characters > 100000) throw new ValidationException("L'export dépasse 100 000 caractères.");
        }
        Text(request.Title, 256);
        Text(request.Type, 40);
        if (request.Options == null)
            throw new ValidationException("Options d'export absentes.");
        Validator.ValidateObject(request.Options, new ValidationContext(request.Options), validateAllProperties: true);
        var lines = 0;
        var stats = 0;
        foreach (var section in request.Sections)
        {
            if (section == null || section.Lines == null || section.Stats == null)
                throw new ValidationException("Section ou contenu absent.");
            lines += section.Lines.Count;
            stats += section.Stats.Count;
            if (lines > 512 || stats > 128)
                throw new ValidationException("L'export est limité à 512 lignes et 128 profils.");
            Text(section.Title, 256);
            foreach (var line in section.Lines)
            {
                if (line == null) throw new ValidationException("Ligne absente.");
                Text(line.Label, 256); Text(line.Value, 4000);
            }
            foreach (var stat in section.Stats)
            {
                if (stat == null) throw new ValidationException("Profil absent.");
                Text(stat.Name, 256); Text(stat.Role, 256); Text(stat.Origin, 256); Text(stat.Danger, 256);
            }
        }

        void OptionalText(string? value, int max)
        {
            if (value is null)
                return;
            Text(value, max);
        }

        void ValidateStats(MjPdfStatsDto? stat)
        {
            if (stat == null)
                throw new ValidationException("Profil absent.");
            stats++;
            if (stats > 128)
                throw new ValidationException("L'export est limité à 512 lignes et 128 profils.");
            Text(stat.Name, 256); Text(stat.Role, 256); Text(stat.Origin, 256); Text(stat.Danger, 256);
        }

        void ValidatePnj(MjPdfPnjDto? pnj)
        {
            if (pnj == null)
                throw new ValidationException("PNJ absent.");
            Text(pnj.Name, 256); Text(pnj.Role, 256); Text(pnj.Profession, 256); Text(pnj.Origin, 256);
            Text(pnj.Danger, 256); Text(pnj.Appearance, 4000); Text(pnj.Manner, 4000);
            Text(pnj.Motivation, 4000); Text(pnj.Secret, 4000); Text(pnj.Hook, 4000);
            Text(pnj.Profile, 4000); Text(pnj.Weapon, 4000); Text(pnj.Equipment, 4000); Text(pnj.Spell, 4000);
            ValidateStats(pnj.Stats);
        }

        if (request.Pnjs == null)
            throw new ValidationException("Liste de PNJ absente.");
        foreach (var pnj in request.Pnjs)
            ValidatePnj(pnj);

        if (request.Loot is { } loot)
        {
            if (loot.Items == null || loot.Items.Count > 128)
                throw new ValidationException("Le trésor est limité à 128 objets.");
            Text(loot.Context, 4000); Text(loot.Nature, 4000); Text(loot.Danger, 256); Text(loot.Coins, 4000);
            foreach (var item in loot.Items) Text(item, 4000);
            Text(loot.UsefulItem, 4000); Text(loot.MagicItem, 4000); Text(loot.MagicEffect, 4000);
            Text(loot.MagicRisk, 4000); Text(loot.ValueEstimate, 4000); Text(loot.Owner, 4000);
            Text(loot.Condition, 4000); Text(loot.Clue, 4000); Text(loot.Complication, 4000);
        }

        void ValidateAdventure(MjPdfAdventureDto? adventure)
        {
            if (adventure == null || adventure.KeyNpcs == null || adventure.Acts == null ||
                adventure.Resolutions == null || adventure.Rewards == null)
                throw new ValidationException("Aventure ou contenu absent.");
            if (adventure.KeyNpcs.Count > 64 || adventure.Acts.Count > 64 ||
                adventure.Resolutions.Count > 128 || adventure.Rewards.Count > 128)
                throw new ValidationException("L'aventure dépasse les limites autorisées.");
            Text(adventure.Title, 256); Text(adventure.Theme, 256); Text(adventure.Region, 256);
            Text(adventure.Danger, 256); Text(adventure.Duration, 256); Text(adventure.Synopsis, 4000);
            Text(adventure.NarrativeSynopsis, 12000); Text(adventure.PlayerSynopsis, 12000);
            Text(adventure.Location, 4000); Text(adventure.Hook, 4000); Text(adventure.Threat, 4000);
            Text(adventure.Atmosphere, 4000); Text(adventure.Twist, 4000); Text(adventure.Climax, 4000);
            OptionalText(adventure.CampaignLink, 4000);
            foreach (var pnj in adventure.KeyNpcs) ValidatePnj(pnj);
            foreach (var act in adventure.Acts)
            {
                if (act == null) throw new ValidationException("Acte absent.");
                Text(act.Title, 256); Text(act.Description, 4000); Text(act.Challenge, 4000); Text(act.Location, 4000);
            }
            foreach (var resolution in adventure.Resolutions) Text(resolution, 4000);
            foreach (var reward in adventure.Rewards) Text(reward, 4000);
        }

        if (request.Adventure is not null)
            ValidateAdventure(request.Adventure);

        if (request.Campaign is { } campaign)
        {
            if (campaign.Episodes == null || campaign.Episodes.Count > 16)
                throw new ValidationException("La campagne est limitée à 16 épisodes.");
            Text(campaign.ArcSummary, 4000); Text(campaign.RecurringVillain, 4000); Text(campaign.Notes, 4000);
            foreach (var episode in campaign.Episodes) ValidateAdventure(episode);
        }
        if (request.Layout is { } layout)
        {
            Validator.ValidateObject(layout, new ValidationContext(layout), validateAllProperties: true);
            if (layout.MarginRight - layout.MarginLeft < 200)
                throw new ValidationException("La largeur du contenu doit être d'au moins 200 pixels.");
        }
    }
}

public sealed class PdfExportBusyException : Exception
{
    public PdfExportBusyException() : base("Deux exports PDF sont déjà en cours. Réessayez dans un instant.") { }
}
