using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Shared.Models;

namespace Wfrp4.Infrastructure.Data;

public static class Wfrp4DataSeeder
{
    public const string DemoJoueurId = "11111111-1111-1111-1111-111111111111";
    public const string DemoMjId = "22222222-2222-2222-2222-222222222222";
    public const string DemoAdminId = "33333333-3333-3333-3333-333333333333";

    public static async Task SeedAsync(Wfrp4DbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.Especes.AnyAsync(cancellationToken))
        {
            db.Especes.AddRange(
                CreateEspece(
                    "HUMAIN",
                    "Humain",
                    4,
                    "L'espèce la plus répandue du Vieux Monde.",
                    ("CC", 30), ("CT", 30), ("F", 30), ("E", 30), ("I", 30), ("Ag", 30), ("Dex", 30), ("Int", 30), ("FM", 30), ("Soc", 30)),
                CreateEspece(
                    "NAIN",
                    "Nain",
                    3,
                    "Courts, robustes et rancuniers.",
                    ("CC", 40), ("CT", 30), ("F", 30), ("E", 40), ("I", 20), ("Ag", 10), ("Dex", 30), ("Int", 30), ("FM", 40), ("Soc", 20)),
                CreateEspece(
                    "HALFLING",
                    "Halfling",
                    3,
                    "Petits, discrets et amateurs de bonne chère.",
                    ("CC", 20), ("CT", 30), ("F", 10), ("E", 20), ("I", 20), ("Ag", 30), ("Dex", 30), ("Int", 30), ("FM", 30), ("Soc", 30)),
                CreateEspece(
                    "HAUT_ELFE",
                    "Haut Elfe",
                    5,
                    "Nobles et anciens, venus d'Ulthuan.",
                    ("CC", 30), ("CT", 30), ("F", 20), ("E", 20), ("I", 40), ("Ag", 40), ("Dex", 30), ("Int", 30), ("FM", 30), ("Soc", 20)),
                CreateEspece(
                    "ELFE_BOIS",
                    "Elfe des Bois",
                    5,
                    "Gardiens farouches d'Athel Loren.",
                    ("CC", 30), ("CT", 40), ("F", 20), ("E", 20), ("I", 40), ("Ag", 40), ("Dex", 30), ("Int", 30), ("FM", 20), ("Soc", 20))
            );

            await db.SaveChangesAsync(cancellationToken);
        }

        // Classes — idempotent par code
        {
            var existingClassCodes = (await db.Classes.Select(c => c.Code).ToListAsync(cancellationToken)).ToHashSet();

            var allClassesData = new (string Code, string Nom, (string Code, string Nom, (string Intitule, StatutTier Statut, int StatutNumerique)[] Niveaux)[] Carrieres)[]
            {
                ("CITOYEN", "Citoyen", [
                    ("AGITATEUR", "Agitateur", [("Auteur de Brochures", StatutTier.Bronze, 1), ("Agitateur", StatutTier.Bronze, 2), ("Fauteur de Troubles", StatutTier.Bronze, 3), ("Démagogue", StatutTier.Bronze, 5)]),
                    ("ARTISAN", "Artisan", [("Apprenti Artisan", StatutTier.Bronze, 2), ("Artisan", StatutTier.Argent, 1), ("Maître Artisan", StatutTier.Argent, 3), ("Maître de guilde", StatutTier.Or, 1)]),
                    ("CITADIN", "Citadin", [("Commis", StatutTier.Argent, 1), ("Citadin", StatutTier.Argent, 2), ("Conseiller municipal", StatutTier.Argent, 5), ("Bourgmestre", StatutTier.Or, 1)]),
                    ("ENQUETEUR", "Enquêteur", [("Limier", StatutTier.Argent, 1), ("Enquêteur", StatutTier.Argent, 2), ("Enquêteur principal", StatutTier.Argent, 3), ("Détective", StatutTier.Argent, 5)]),
                    ("MARCHAND", "Marchand", [("Négociant", StatutTier.Argent, 2), ("Marchand", StatutTier.Argent, 5), ("Maître Marchand", StatutTier.Or, 1), ("Prince Marchand", StatutTier.Or, 3)]),
                    ("MENDIANT", "Mendiant", [("Pauvre", StatutTier.Bronze, 0), ("Mendiant", StatutTier.Bronze, 2), ("Maître mendiant", StatutTier.Bronze, 4), ("Roi mendiant", StatutTier.Argent, 2)]),
                    ("RATIER", "Ratier", [("Chasseur de Rats", StatutTier.Bronze, 3), ("Ratier", StatutTier.Argent, 1), ("Vérin d'égout", StatutTier.Argent, 2), ("Exterminateur", StatutTier.Argent, 3)]),
                    ("SENTINELLE", "Sentinelle", [("Recrue du guet", StatutTier.Bronze, 3), ("Sentinelle", StatutTier.Argent, 1), ("Sergent de quart", StatutTier.Argent, 3), ("Capitaine de quart", StatutTier.Or, 1)])
                ]),
                ("COURTISAN", "Courtisan", [
                    ("ARTISTE", "Artiste", [("Apprenti Artiste", StatutTier.Argent, 1), ("Artiste", StatutTier.Argent, 3), ("Maître Artiste", StatutTier.Argent, 5), ("Maestro", StatutTier.Or, 2)]),
                    ("CONSEILLER", "Conseiller", [("Aide", StatutTier.Argent, 2), ("Conseiller", StatutTier.Argent, 4), ("Consultant", StatutTier.Or, 1), ("Chancelier", StatutTier.Or, 3)]),
                    ("DUELLISTE", "Duelliste", [("Escrimeur", StatutTier.Argent, 3), ("Duelliste", StatutTier.Argent, 5), ("Maître du duel", StatutTier.Or, 1), ("Champion judiciaire", StatutTier.Or, 3)]),
                    ("EMISSAIRE", "Émissaire", [("Héraut", StatutTier.Argent, 2), ("Émissaire", StatutTier.Argent, 4), ("Diplomate", StatutTier.Or, 2), ("Ambassadeur", StatutTier.Or, 5)]),
                    ("ESPION", "Espion", [("Informateur", StatutTier.Bronze, 3), ("Espion", StatutTier.Argent, 3), ("Agent", StatutTier.Or, 1), ("Maître Espion", StatutTier.Or, 1)]),
                    ("INTENDANT", "Intendant", [("Dépositaire", StatutTier.Argent, 1), ("Intendant", StatutTier.Argent, 3), ("Sénéchal", StatutTier.Or, 1), ("Gouverneur", StatutTier.Or, 3)]),
                    ("NOBLE", "Noble", [("Scion", StatutTier.Or, 1), ("Noble", StatutTier.Or, 3), ("Magnat", StatutTier.Or, 5), ("Seigneur Noble", StatutTier.Or, 7)]),
                    ("SERVITEUR", "Serviteur", [("Subalterne", StatutTier.Argent, 1), ("Serviteur", StatutTier.Argent, 3), ("Préposé", StatutTier.Argent, 5), ("Régisseur", StatutTier.Or, 1)])
                ]),
                ("FILOU", "Filou", [
                    ("CHARLATAN", "Charlatan", [("Escroc", StatutTier.Bronze, 3), ("Charlatan", StatutTier.Bronze, 5), ("Artiste d'Escroquerie", StatutTier.Argent, 2), ("Canaille", StatutTier.Argent, 4)]),
                    ("HORS_LOI", "Hors-la-loi", [("Brigand", StatutTier.Bronze, 1), ("Hors-la-loi", StatutTier.Bronze, 2), ("Chef des hors-la-loi", StatutTier.Bronze, 4), ("Roi Bandit", StatutTier.Argent, 2)]),
                    ("PILLEUR_TOMBES", "Pilleur de Tombes", [("Voleur de Corps", StatutTier.Bronze, 2), ("Pilleur de Tombe", StatutTier.Bronze, 3), ("Voleur des Tombeaux", StatutTier.Argent, 1), ("Chasseur de Trésor", StatutTier.Argent, 5)]),
                    ("RACKETTEUR", "Racketteur", [("Voyou", StatutTier.Bronze, 3), ("Racketteur", StatutTier.Bronze, 5), ("Chef de gang", StatutTier.Argent, 3), ("Seigneur du Crime", StatutTier.Argent, 5)]),
                    ("RECELEUR", "Receleur", [("Courtier", StatutTier.Argent, 1), ("Receleur", StatutTier.Argent, 2), ("Maître Receleur", StatutTier.Argent, 3), ("Marché Noir", StatutTier.Argent, 4)]),
                    ("SORCIERE", "Sorcière", [("Envoûteuse", StatutTier.Bronze, 1), ("Sorcière", StatutTier.Bronze, 2), ("Gigoteuse", StatutTier.Bronze, 3), ("Démone-sorcière", StatutTier.Bronze, 5)]),
                    ("TRAFIQUANT", "Trafiquant", [("Arnaqueur", StatutTier.Bronze, 1), ("Trafiquant", StatutTier.Bronze, 3), ("Fournisseur", StatutTier.Argent, 1), ("Meneur du Cercle", StatutTier.Argent, 3)]),
                    ("VOLEUR", "Voleur", [("Rôdeur", StatutTier.Bronze, 1), ("Voleur", StatutTier.Bronze, 3), ("Maître voleur", StatutTier.Bronze, 5), ("Cambrioleur", StatutTier.Argent, 3)])
                ]),
                ("FORESTIER", "Forestier", [
                    ("AMUSEUR", "Amuseur", [("Artiste de Rue", StatutTier.Bronze, 3), ("Artiste", StatutTier.Bronze, 5), ("Troubadour", StatutTier.Argent, 3), ("Chef de Troupe", StatutTier.Or, 1)]),
                    ("CHASSEUR_PRIMES", "Chasseur de Primes", [("Pique-voleur", StatutTier.Argent, 1), ("Chasseur de primes", StatutTier.Argent, 3), ("Maître chasseur de primes", StatutTier.Argent, 5), ("Général Chasseur de Primes", StatutTier.Or, 1)]),
                    ("CHASSEUR_SORCIERES", "Chasseur de Sorcières", [("Interrogateur", StatutTier.Argent, 1), ("Chasseur de sorcières", StatutTier.Argent, 3), ("Inquisiteur", StatutTier.Argent, 5), ("Traqueur de Sorcières Général", StatutTier.Or, 1)]),
                    ("COCHER", "Cocher", [("Postillon", StatutTier.Argent, 1), ("Cocher", StatutTier.Argent, 2), ("Maître Cocher", StatutTier.Argent, 3), ("Maître de Route", StatutTier.Argent, 5)]),
                    ("COLPORTEUR", "Colporteur", [("Vagabond", StatutTier.Bronze, 1), ("Colporteur", StatutTier.Bronze, 4), ("Maître Colporteur", StatutTier.Argent, 1), ("Négociant Errant", StatutTier.Argent, 3)]),
                    ("FLAGELLANT", "Flagellant", [("Zélote", StatutTier.Bronze, 0), ("Flagellant", StatutTier.Bronze, 0), ("Pénitent", StatutTier.Bronze, 0), ("Prophète de malheur", StatutTier.Bronze, 0)]),
                    ("MESSAGER", "Messager", [("Coureur", StatutTier.Bronze, 3), ("Messager", StatutTier.Argent, 1), ("Courrier", StatutTier.Argent, 3), ("Capitaine-Courrier", StatutTier.Argent, 5)]),
                    ("PATROUILLEUR", "Patrouilleur", [("Collecteur de péage", StatutTier.Bronze, 5), ("Patrouilleur", StatutTier.Argent, 2), ("Sergent de route", StatutTier.Argent, 4), ("Capitaine de route", StatutTier.Or, 1)])
                ]),
                ("GUERRIER", "Guerrier", [
                    ("CAVALIER", "Cavalier", [("Homme de cavalerie", StatutTier.Argent, 2), ("Cavalier", StatutTier.Argent, 4), ("Sergent de cavalerie", StatutTier.Or, 1), ("Officier de cavalerie", StatutTier.Or, 2)]),
                    ("CHEVALIER", "Chevalier", [("Écuyer", StatutTier.Argent, 3), ("Chevalier", StatutTier.Argent, 5), ("Premier Chevalier", StatutTier.Or, 2), ("Chevalier du Cercle Intérieur", StatutTier.Or, 4)]),
                    ("GARDE", "Garde", [("Sentinelle", StatutTier.Argent, 1), ("Garde", StatutTier.Argent, 2), ("Garde d'honneur", StatutTier.Argent, 3), ("Officier de garde", StatutTier.Argent, 5)]),
                    ("GLADIATEUR", "Gladiateur", [("Pugiliste", StatutTier.Bronze, 4), ("Gladiateur", StatutTier.Argent, 2), ("Champion de la fosse", StatutTier.Argent, 5), ("Légende de la fosse", StatutTier.Or, 2)]),
                    ("GROS_BRAS", "Gros Bras", [("Bagarreur", StatutTier.Bronze, 2), ("Gros Bras", StatutTier.Argent, 1), ("Homme de Main", StatutTier.Argent, 4), ("Assassin", StatutTier.Or, 1)]),
                    ("PRETRE_GUERRIER", "Prêtre Guerrier", [("Noviciat", StatutTier.Bronze, 2), ("Prêtre Guerrier", StatutTier.Argent, 2), ("Sergent Prêtre", StatutTier.Argent, 3), ("Capitaine Prêtre", StatutTier.Argent, 4)]),
                    ("SOLDAT", "Soldat", [("Recrue", StatutTier.Argent, 1), ("Soldat", StatutTier.Argent, 3), ("Sergent", StatutTier.Argent, 5), ("Officier", StatutTier.Or, 1)]),
                    ("TUEUR", "Tueur", [("Tueur de Trolls", StatutTier.Bronze, 2), ("Tueur de géant", StatutTier.Bronze, 2), ("Tueur de dragon", StatutTier.Bronze, 2), ("Tueur de démon", StatutTier.Bronze, 2)])
                ]),
                ("LETTRE", "Lettré", [
                    ("APOTHICAIRE", "Apothicaire", [("Apprenti Apothicaire", StatutTier.Bronze, 3), ("Apothicaire", StatutTier.Argent, 1), ("Maître Apothicaire", StatutTier.Argent, 3), ("Apothicaire général", StatutTier.Or, 1)]),
                    ("AVOCAT", "Avocat", [("Étudiant en Droit", StatutTier.Bronze, 4), ("Avocat", StatutTier.Argent, 3), ("Procureur", StatutTier.Or, 1), ("Juge", StatutTier.Or, 2)]),
                    ("ERUDIT", "Érudit", [("Étudiant", StatutTier.Bronze, 3), ("Érudit", StatutTier.Argent, 2), ("Compagnon", StatutTier.Argent, 5), ("Professeur", StatutTier.Or, 1)]),
                    ("INGENIEUR", "Ingénieur", [("Étudiant ingénieur", StatutTier.Bronze, 4), ("Ingénieur", StatutTier.Argent, 2), ("Maître Ingénieur", StatutTier.Argent, 4), ("Ingénieur agréé", StatutTier.Or, 2)]),
                    ("MAGE", "Mage", [("Apprenti Mage", StatutTier.Bronze, 3), ("Mage", StatutTier.Argent, 3), ("Maître Mage", StatutTier.Or, 1), ("Seigneur Mage", StatutTier.Or, 2)]),
                    ("MEDECIN", "Médecin", [("Apprenti Médecin", StatutTier.Bronze, 4), ("Médecin", StatutTier.Argent, 3), ("Docteur", StatutTier.Argent, 5), ("Médecin de la Court", StatutTier.Or, 1)]),
                    ("NONNE", "Nonne", [("Novice", StatutTier.Bronze, 1), ("Nonne", StatutTier.Bronze, 4), ("Abbesse", StatutTier.Argent, 2), ("Prieure Générale", StatutTier.Argent, 5)]),
                    ("PRETRE", "Prêtre", [("Initié", StatutTier.Bronze, 3), ("Prêtre", StatutTier.Argent, 1), ("Grand Prêtre", StatutTier.Or, 1), ("Lecteur", StatutTier.Or, 2)])
                ]),
                ("PAYSAN", "Paysan", [
                    ("CHASSEUR", "Chasseur", [("Trappeur", StatutTier.Bronze, 2), ("Chasseur", StatutTier.Bronze, 4), ("Traqueur", StatutTier.Argent, 1), ("Maître-Chasseur", StatutTier.Argent, 3)]),
                    ("ECLAIREUR", "Éclaireur", [("Guide", StatutTier.Bronze, 3), ("Éclaireur", StatutTier.Bronze, 5), ("Pionnier", StatutTier.Argent, 1), ("Explorateur", StatutTier.Argent, 5)]),
                    ("HERBORISTE", "Herboriste", [("Récolteur d'herbes", StatutTier.Bronze, 2), ("Herboriste", StatutTier.Bronze, 4), ("Maître Herboriste", StatutTier.Argent, 1), ("Sage Herboriste", StatutTier.Argent, 3)]),
                    ("HUISSIER", "Huissier", [("Percepteur", StatutTier.Argent, 1), ("Huissier", StatutTier.Argent, 5), ("Préfet", StatutTier.Or, 1), ("Magistrat", StatutTier.Or, 3)]),
                    ("MINEUR", "Mineur", [("Prospecteur", StatutTier.Bronze, 2), ("Mineur", StatutTier.Bronze, 4), ("Maître mineur", StatutTier.Bronze, 5), ("Contremaître de mine", StatutTier.Argent, 4)]),
                    ("MYSTIQUE", "Mystique", [("Diseuse de bonne aventure", StatutTier.Bronze, 1), ("Mystique", StatutTier.Bronze, 2), ("Sage", StatutTier.Bronze, 3), ("Prophétesse", StatutTier.Bronze, 4)]),
                    ("SORCIER_VILLAGE", "Sorcier de Village", [("Apprenti", StatutTier.Bronze, 1), ("Sorcier de Village", StatutTier.Bronze, 2), ("Maître", StatutTier.Bronze, 3), ("Sage", StatutTier.Bronze, 5)]),
                    ("VILLAGEOIS", "Villageois", [("Paysan", StatutTier.Bronze, 2), ("Villageois", StatutTier.Bronze, 3), ("Conseiller", StatutTier.Bronze, 4), ("Aîné du village", StatutTier.Argent, 2)])
                ]),
                ("RIVERAIN", "Riverain", [
                    ("BATELIER", "Batelier", [("Passeur", StatutTier.Argent, 1), ("Batelier", StatutTier.Argent, 2), ("Maître d'Équipage", StatutTier.Argent, 3), ("Maître chaland", StatutTier.Argent, 5)]),
                    ("CONTREBANDIER", "Contrebandier", [("Coursier de rivière", StatutTier.Bronze, 2), ("Contrebandier", StatutTier.Bronze, 3), ("Maître contrebandier", StatutTier.Bronze, 5), ("Roi Contrebandier", StatutTier.Argent, 2)]),
                    ("DEBARDEUR", "Débardeur", [("Arrimeur", StatutTier.Bronze, 3), ("Débardeur", StatutTier.Argent, 1), ("Contremaître", StatutTier.Argent, 3), ("Maître de quai", StatutTier.Argent, 5)]),
                    ("MATELOT", "Matelot", [("Homme d'Équipage", StatutTier.Argent, 1), ("Matelot", StatutTier.Argent, 3), ("Maître d'équipage", StatutTier.Argent, 5), ("Capitaine", StatutTier.Or, 2)]),
                    ("NAUFRAGEUR", "Naufrageur", [("Piégeur de cargaison", StatutTier.Bronze, 2), ("Naufrageur", StatutTier.Bronze, 3), ("Pirate de rivière", StatutTier.Bronze, 5), ("Capitaine Naufrageur", StatutTier.Argent, 2)]),
                    ("PATROUILLEUR_FLUVIAL", "Patrouilleur Fluvial", [("Recrue de rivière", StatutTier.Argent, 1), ("Patrouilleur fluvial", StatutTier.Argent, 2), ("Épée de navire", StatutTier.Argent, 4), ("Maître d'Épée de navire", StatutTier.Or, 1)]),
                    ("PILOTE", "Pilote", [("Guide fluvial", StatutTier.Bronze, 4), ("Pilote", StatutTier.Argent, 1), ("Guide Pilote", StatutTier.Argent, 3), ("Maître Pilote", StatutTier.Argent, 5)]),
                    ("RIVERAINE", "Riveraine", [("Pêcheur vernie", StatutTier.Bronze, 2), ("Riveraine", StatutTier.Bronze, 3), ("Sage de la rivière", StatutTier.Bronze, 5), ("Aîné de la rivière", StatutTier.Argent, 2)])
                ]),
            };

            foreach (var (classeCode, classeNom, carrieres) in allClassesData)
            {
                if (existingClassCodes.Contains(classeCode)) continue;

                var classe = new Classe
                {
                    Code = classeCode,
                    Nom = classeNom,
                };

                foreach (var (carriereCode, carriereNom, niveaux) in carrieres)
                {
                    var carriere = new Carriere
                    {
                        Code = carriereCode,
                        Nom = carriereNom,
                    };

                    var niveau = 1;
                    foreach (var (intitule, statut, statutNumerique) in niveaux)
                    {
                        carriere.Niveaux.Add(new NiveauCarriere
                        {
                            Niveau = niveau++,
                            Intitule = intitule,
                            Statut = statut,
                            StatutNumerique = statutNumerique,
                        });
                    }

                    classe.Carrieres.Add(carriere);
                }

                db.Classes.Add(classe);
            }

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(cancellationToken);
        }

        // Compétences — idempotent par code (liste complète WFRP4)
        {
            var existingCompCodes = (await db.Competences.Select(c => c.Code).ToListAsync(cancellationToken)).ToHashSet();
            var allCompetences = new Competence[]
            {
                // --- Compétences de Base ---
                new() { Code = "ART",              Nom = "Art",                    Caracteristique = "Dex", EstAvancee = false, EstGroupee = true,  Description = "Créer des œuvres artistiques (peinture, sculpture, musique...)." },
                new() { Code = "ATHLETISME",       Nom = "Athlétisme",             Caracteristique = "Ag",  EstAvancee = false, EstGroupee = false, Description = "Courir, sauter, grimper et se déplacer avec grâce." },
                new() { Code = "CALME",            Nom = "Calme",                  Caracteristique = "FM",  EstAvancee = false, EstGroupee = false, Description = "Résister à la peur, au stress et à la corruption." },
                new() { Code = "CANOTAGE",         Nom = "Canotage",               Caracteristique = "F",   EstAvancee = false, EstGroupee = false, Description = "Ramer, pagayer et manœuvrer de petites embarcations." },
                new() { Code = "CHARME",           Nom = "Charme",                 Caracteristique = "Soc", EstAvancee = false, EstGroupee = false, Description = "Influencer autrui par l'éloquence et le tact." },
                new() { Code = "COMMANDEMENT",     Nom = "Commandement",           Caracteristique = "Soc", EstAvancee = false, EstGroupee = false, Description = "Diriger un groupe et inspirer obéissance." },
                new() { Code = "COMMERAGE",        Nom = "Commérages",             Caracteristique = "Soc", EstAvancee = false, EstGroupee = false, Description = "Obtenir des informations par les rumeurs locales." },
                new() { Code = "CONDUITE",         Nom = "Conduite",               Caracteristique = "Ag",  EstAvancee = false, EstGroupee = true,  Description = "Conduire véhicules et attelages à traction animale." },
                new() { Code = "DISCRETION",       Nom = "Discrétion",             Caracteristique = "Ag",  EstAvancee = false, EstGroupee = false, Description = "Se déplacer et agir sans être remarqué." },
                new() { Code = "EMPRISE_ANIMAUX",  Nom = "Emprise sur les Animaux",Caracteristique = "FM",  EstAvancee = false, EstGroupee = false, Description = "Calmer, contrôler ou intimider les animaux." },
                new() { Code = "EQUITATION",       Nom = "Équitation",             Caracteristique = "Ag",  EstAvancee = false, EstGroupee = false, Description = "Monter et guider des animaux de trait." },
                new() { Code = "ESCALADE",         Nom = "Escalade",               Caracteristique = "F",   EstAvancee = false, EstGroupee = false, Description = "Grimper des parois, murs et obstacles." },
                new() { Code = "ESQUIVE",          Nom = "Esquive",                Caracteristique = "Ag",  EstAvancee = false, EstGroupee = false, Description = "Éviter les attaques et dangers physiques." },
                new() { Code = "INTIMIDATION",     Nom = "Intimidation",           Caracteristique = "F",   EstAvancee = false, EstGroupee = false, Description = "Faire peur ou plier autrui par la menace." },
                new() { Code = "INTERPRETATION",   Nom = "Interprétation",         Caracteristique = "Soc", EstAvancee = false, EstGroupee = true,  Description = "Jouer, chanter, danser ou se produire devant un public." },
                new() { Code = "INTUITION",        Nom = "Intuition",              Caracteristique = "I",   EstAvancee = false, EstGroupee = false, Description = "Lire une situation ou déceler les intentions d'une personne." },
                new() { Code = "JEU",              Nom = "Jeu",                    Caracteristique = "Int", EstAvancee = false, EstGroupee = false, Description = "Jouer aux jeux de hasard ou de stratégie." },
                new() { Code = "MARCHANDAGE",      Nom = "Marchandage",            Caracteristique = "Soc", EstAvancee = false, EstGroupee = false, Description = "Négocier prix, achats et contrats." },
                new() { Code = "MELEE",            Nom = "Mêlée",                  Caracteristique = "CC",  EstAvancee = false, EstGroupee = true,  Description = "Combattre en corps à corps (Armes de base, Escrime, etc.)." },
                new() { Code = "NAVIGATION",       Nom = "Navigation",             Caracteristique = "Int", EstAvancee = false, EstGroupee = false, Description = "Lire cartes, étoiles et se repérer en milieu naturel." },
                new() { Code = "PERCEPTION",       Nom = "Perception",             Caracteristique = "I",   EstAvancee = false, EstGroupee = false, Description = "Repérer les détails, ennemis cachés et menaces." },
                new() { Code = "RESISTANCE",       Nom = "Résistance",             Caracteristique = "E",   EstAvancee = false, EstGroupee = false, Description = "Endurer la douleur, la fatigue et les poisons." },
                new() { Code = "RESISTANCE_ALCOOL",Nom = "Résistance à l'Alcool",  Caracteristique = "E",   EstAvancee = false, EstGroupee = false, Description = "Supporter les effets de l'alcool et autres substances." },
                new() { Code = "SURVIE",           Nom = "Survie en Extérieur",    Caracteristique = "Int", EstAvancee = false, EstGroupee = false, Description = "Trouver nourriture, abri et eau en pleine nature." },
                new() { Code = "CORRUPTION",       Nom = "Corruption",             Caracteristique = "Soc", EstAvancee = false, EstGroupee = false, Description = "Soudoyer et corrompre pour obtenir des faveurs." },
                // --- Compétences Avancées ---
                new() { Code = "A_DISTANCE",       Nom = "À Distance",             Caracteristique = "CT",  EstAvancee = true,  EstGroupee = true,  Description = "Utiliser arcs, arbalètes, pistolets et armes de jet." },
                new() { Code = "BRACONNAGE",       Nom = "Braconnage",             Caracteristique = "Dex", EstAvancee = true,  EstGroupee = false, Description = "Poser et désamorcer des pièges de toutes sortes." },
                new() { Code = "CANALISATION",     Nom = "Canalisation",           Caracteristique = "FM",  EstAvancee = true,  EstGroupee = true,  Description = "Maîtriser et canaliser les Vents de Magie." },
                new() { Code = "CONNAISSANCES",    Nom = "Connaissances",          Caracteristique = "Int", EstAvancee = true,  EstGroupee = true,  Description = "Savoir académique dans un domaine précis (histoire, théologie...)." },
                new() { Code = "CROCHETAGE",       Nom = "Crochetage",             Caracteristique = "Dex", EstAvancee = true,  EstGroupee = false, Description = "Forcer serrures et mécanismes sans la clé." },
                new() { Code = "DRESSAGE",         Nom = "Dressage d'Animaux",     Caracteristique = "Int", EstAvancee = true,  EstGroupee = false, Description = "Entraîner et dresser des animaux à des tâches spécifiques." },
                new() { Code = "EVALUATION",       Nom = "Évaluation",             Caracteristique = "Int", EstAvancee = true,  EstGroupee = false, Description = "Estimer la valeur marchande d'un bien ou d'un objet magique." },
                new() { Code = "ESCAMOTAGE",       Nom = "Escamotage",             Caracteristique = "Dex", EstAvancee = true,  EstGroupee = false, Description = "Faire disparaître ou apparaître des objets subtilement." },
                new() { Code = "JOUER",            Nom = "Jouer d'un Instrument",  Caracteristique = "Dex", EstAvancee = true,  EstGroupee = true,  Description = "Maîtriser un instrument de musique (luth, flûte, tambour...)." },
                new() { Code = "LANGUE",           Nom = "Langue",                 Caracteristique = "Int", EstAvancee = true,  EstGroupee = true,  Description = "Parler et comprendre des idiomes spécifiques." },
                new() { Code = "METIER",           Nom = "Métier",                 Caracteristique = "Int", EstAvancee = true,  EstGroupee = true,  Description = "Exercer un métier artisanal ou technique précis." },
                new() { Code = "NATATION",         Nom = "Natation",               Caracteristique = "F",   EstAvancee = true,  EstGroupee = false, Description = "Nager et se déplacer dans l'eau." },
                new() { Code = "PISTAGE",          Nom = "Pistage",                Caracteristique = "Int", EstAvancee = true,  EstGroupee = false, Description = "Suivre des traces en milieu naturel ou urbain." },
                new() { Code = "PRIER",            Nom = "Prier",                  Caracteristique = "FM",  EstAvancee = true,  EstGroupee = true,  Description = "Invoquer les faveurs divines d'une divinité spécifique." },
                new() { Code = "PSYCHOLOGIE",      Nom = "Psychologie",            Caracteristique = "Int", EstAvancee = true,  EstGroupee = false, Description = "Comprendre les motivations et émotions d'autrui." },
                new() { Code = "RECHERCHE",        Nom = "Recherche",              Caracteristique = "Int", EstAvancee = true,  EstGroupee = false, Description = "Trouver des informations dans des archives ou bibliothèques." },
                new() { Code = "REPRESENTATION",   Nom = "Représentation",         Caracteristique = "Soc", EstAvancee = true,  EstGroupee = true,  Description = "Jouer la comédie, se déguiser ou incarner un personnage." },
                new() { Code = "SIGNES_SECRETS",   Nom = "Signes Secrets",         Caracteristique = "Int", EstAvancee = true,  EstGroupee = true,  Description = "Utiliser des codes et symboles d'une organisation secrète." },
                new() { Code = "SOINS",            Nom = "Soins",                  Caracteristique = "Int", EstAvancee = true,  EstGroupee = false, Description = "Soigner blessures et maladies." },
                new() { Code = "SOINS_ANIMAUX",    Nom = "Soins des Animaux",      Caracteristique = "Int", EstAvancee = true,  EstGroupee = false, Description = "Traiter blessures et maladies chez les animaux." },
                new() { Code = "VOILE",            Nom = "Voile",                  Caracteristique = "Int", EstAvancee = true,  EstGroupee = false, Description = "Naviguer et manœuvrer un navire à voile." },
            };
            foreach (var comp in allCompetences)
            {
                if (!existingCompCodes.Contains(comp.Code))
                    db.Competences.Add(comp);
            }
            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(cancellationToken);
        }

        // Talents — idempotent par code (liste complète WFRP4)
        {
            var existingTalentCodes = (await db.Talents.Select(t => t.Code).ToListAsync(cancellationToken)).ToHashSet();
            var allTalents = new Talent[]
            {
                new() { Code = "ACUITE",              Nom = "Acuïté Visuelle",           MaxFois = 1, Empilable = false, Description = "Vous remarquez plus facilement les détails visuels.", Effet = "+10 aux tests de Perception liés à la vue." },
                new() { Code = "ALERTE",              Nom = "Alerte",                    MaxFois = 1, Empilable = false, Description = "Vous réagissez vite au danger.", Effet = "+10 à l'Initiative au premier round de combat." },
                new() { Code = "AMBIDEXTRE",          Nom = "Ambidextre",                MaxFois = 1, Empilable = false, Description = "Vous maniez les armes avec les deux mains sans pénalité.", Effet = "Supprime la pénalité de main directrice." },
                new() { Code = "ARME_PREDILECTION",   Nom = "Arme de Prédilection",      MaxFois = null, Empilable = true,  Description = "Vous excellez avec un groupe d'armes spécifique.", Effet = "+10 aux tests de Mêlée ou À Distance avec le groupe choisi." },
                new() { Code = "ATTAQUE_PUISSANTE",   Nom = "Attaque Puissante",         MaxFois = 1, Empilable = false, Description = "Vos frappes portent plus loin.", Effet = "+Bonus de Force aux Dégâts en mêlée." },
                new() { Code = "BATAILLEUR",          Nom = "Batailleur",                MaxFois = 1, Empilable = false, Description = "Les bagarres de taverne sont votre terrain.", Effet = "+10 aux tests de Mêlée (Improvisation)." },
                new() { Code = "BEAU_PARLEUR",        Nom = "Beau Parleur",              MaxFois = 1, Empilable = false, Description = "Vos paroles coulent comme du miel.", Effet = "+10 aux tests de Charme." },
                new() { Code = "CARACTERE_AFFIRME",   Nom = "Caractère Affirmé",         MaxFois = 1, Empilable = false, Description = "Votre volonté est un roc.", Effet = "+5 à la Force Mentale." },
                new() { Code = "COMBAT_RAPPROCHE",    Nom = "Combat Rapproché",          MaxFois = 1, Empilable = false, Description = "Vous excellez dans les espaces confinés.", Effet = "Ignore la pénalité de petite arme en espace étroit." },
                new() { Code = "COOLHEAD",            Nom = "Sang-froid",                MaxFois = 1, Empilable = false, Description = "Vous gardez vos moyens sous pression.", Effet = "+10 aux tests de Calme." },
                new() { Code = "COUP_PRECIS",         Nom = "Coup Précis",               MaxFois = 1, Empilable = false, Description = "Vous visez les points faibles.", Effet = "+1 au nombre de Niveaux de Succès en mêlée pour les dégâts." },
                new() { Code = "COURAGEUX",           Nom = "Courageux",                 MaxFois = 1, Empilable = false, Description = "La peur ne vous terrasse pas aisément.", Effet = "+10 aux tests de Calme contre la peur." },
                new() { Code = "CUIR_EPAIS",          Nom = "Cuir Épais",                MaxFois = 1, Empilable = false, Description = "Votre peau est dure à percer.", Effet = "+1 Point de Protection naturel." },
                new() { Code = "DEFENSE_TOTALE",      Nom = "Défense Totale",            MaxFois = 1, Empilable = false, Description = "Vous pouvez tout sacrifier pour parer les coups.", Effet = "Action : doubles votre bonus de Défense ce round." },
                new() { Code = "DETERMINATION",       Nom = "Détermination",             MaxFois = null, Empilable = true,  Description = "Votre résolution est à toute épreuve.", Effet = "+1 point de Résolution par prise." },
                new() { Code = "DEUX_ARMES",          Nom = "Deux Armes",                MaxFois = 1, Empilable = false, Description = "Vous combattez avec une arme dans chaque main.", Effet = "Supprime la pénalité de main non directrice en bi-maniement." },
                new() { Code = "DOCTEUR",             Nom = "Docteur",                   MaxFois = 1, Empilable = false, Description = "Vous maîtrisez les gestes médicaux avancés.", Effet = "Peut soigner des blessures critiques sans pénalité." },
                new() { Code = "DON_LANGUES",         Nom = "Don des Langues",           MaxFois = 1, Empilable = false, Description = "Vous maîtrisez les langues avec facilité.", Effet = "+10 à tous les tests de Langue." },
                new() { Code = "ENDURANCE_NAT",       Nom = "Endurance Naturelle",       MaxFois = null, Empilable = true,  Description = "Votre corps récupère vite.", Effet = "+1 Blessure par prise." },
                new() { Code = "FRENESIE",            Nom = "Frénésie",                  MaxFois = 1, Empilable = false, Description = "Vous entrez en transe guerrière dévastatrice.", Effet = "+10 CC, +Bonus de F aux dégâts, immunité Peur/psychologie, mais ne peut ni esquiver ni fuir." },
                new() { Code = "FURIE_MARTIALE",      Nom = "Furie Martiale",            MaxFois = 1, Empilable = false, Description = "En combat, une rage froide vous habite.", Effet = "+Bonus de Force aux dégâts quand vous chargez." },
                new() { Code = "INSTINCT_COMBAT",     Nom = "Instinct de Combat",        MaxFois = 1, Empilable = false, Description = "Vous lisez la bataille comme un livre.", Effet = "+1 à l'Initiative." },
                new() { Code = "INTREPIDE",           Nom = "Intrépide",                 MaxFois = 1, Empilable = false, Description = "La Terreur ne vous arrête pas.", Effet = "Immunité à la condition Terreur (traité comme Peur)." },
                new() { Code = "LECTURE_ECRITURE",    Nom = "Lecture/Écriture",          MaxFois = 1, Empilable = false, Description = "Vous savez lire et écrire dans au moins une langue.", Effet = "Accès plein à Recherche et Connaissances." },
                new() { Code = "MEMOIRE_PHOTO",       Nom = "Mémoire Photographique",    MaxFois = 1, Empilable = false, Description = "Vous mémorisez parfaitement ce que vous voyez.", Effet = "+20 aux tests de Connaissances pour se souvenir d'un détail." },
                new() { Code = "NERFS_ACIER",         Nom = "Nerfs d'Acier",             MaxFois = 1, Empilable = false, Description = "Les épreuves ne vous ébranlent pas.", Effet = "+10 aux tests de Calme contre le stress et la Corruption." },
                new() { Code = "NOBLESSE",            Nom = "Étiquette",                 MaxFois = 1, Empilable = false, Description = "Vous connaissez les codes sociaux des puissants.", Effet = "+10 aux interactions en milieu noble ou de cour." },
                new() { Code = "PERSPICACE",          Nom = "Perspicace",                MaxFois = 1, Empilable = false, Description = "Vous décelez les mensonges et les motivations.", Effet = "+10 aux tests de Psychologie." },
                new() { Code = "PIED_LEGER",          Nom = "Pied Léger",                MaxFois = 1, Empilable = false, Description = "Vous vous déplacez sans bruit.", Effet = "+10 aux tests de Discrétion liés au mouvement." },
                new() { Code = "RESISTANCE_CHAOS",    Nom = "Résistance à la Corruption",MaxFois = null, Empilable = true,  Description = "Le Chaos a du mal à vous atteindre.", Effet = "-1 Point de Corruption subi par prise." },
                new() { Code = "RESISTANCE_POISON",   Nom = "Résistance au Poison",      MaxFois = null, Empilable = true,  Description = "Les poisons s'atténuent en vous.", Effet = "+10 aux tests de Résistance contre les poisons." },
                new() { Code = "ROBUSTE",             Nom = "Robuste",                   MaxFois = null, Empilable = true,  Description = "Vous encaissez mieux les coups.", Effet = "+1 Blessure par prise." },
                new() { Code = "RODEUR_FOREST",       Nom = "Rôdeur Forestier",          MaxFois = 1, Empilable = false, Description = "La forêt est votre demeure.", Effet = "+10 aux tests de Discrétion et Survie en forêt." },
                new() { Code = "SAVOIR",              Nom = "Esprit Affûté",             MaxFois = 1, Empilable = false, Description = "Votre raisonnement est précis et analytique.", Effet = "+10 à un groupe de tests intellectuels (choix à l'achat)." },
                new() { Code = "SENS_DANGER",         Nom = "Sens du Danger",            MaxFois = 1, Empilable = false, Description = "Vos instincts vous avertissent du danger.", Effet = "Test d'Intuition gratuit face aux embuscades." },
                new() { Code = "SENS_NATURE",         Nom = "Sens de la Nature",         MaxFois = 1, Empilable = false, Description = "Vous lisez la nature comme un livre ouvert.", Effet = "+10 aux tests de Survie et de Pistage." },
                new() { Code = "SIXIEME_SENS",        Nom = "Sixième Sens",              MaxFois = 1, Empilable = false, Description = "Vous percevez des menaces invisibles.", Effet = "Le MJ peut fournir un avertissement cryptique face à un danger." },
                new() { Code = "TIREUR_ELITE",        Nom = "Tireur d'Élite",            MaxFois = 1, Empilable = false, Description = "Votre précision au tir est remarquable.", Effet = "+1 au nombre de NS au tir pour les dégâts." },
                new() { Code = "TRES_RESISTANT",      Nom = "Très Résistant",            MaxFois = 1, Empilable = false, Description = "Votre constitution est exceptionnelle.", Effet = "+5 à l'Endurance." },
                new() { Code = "VIFS_REFLEXES",       Nom = "Vifs Réflexes",             MaxFois = 1, Empilable = false, Description = "Votre vitesse de réaction est surprenante.", Effet = "+5 à l'Agilité." },
                new() { Code = "AFFABLE",              Nom = "Affable",                   MaxFois = 1, Empilable = false, Description = "Votre chaleur naturelle met les gens à l'aise.", Effet = "+10 aux tests de Commérages." },
                new() { Code = "CHANCE",               Nom = "Chance",                    MaxFois = null, Empilable = true,  Description = "La fortune vous sourit.", Effet = "+1 point de Fortune par prise." },
                new() { Code = "DOIGTS_AGILES",        Nom = "Doigts Agiles",             MaxFois = 1, Empilable = false, Description = "Vos doigts sont d'une dextérité remarquable.", Effet = "+10 aux tests de Crochetage et Escamotage." },
                new() { Code = "GUERRIER_NE",          Nom = "Guerrier-Né",               MaxFois = 1, Empilable = false, Description = "Le combat est dans votre sang.", Effet = "+5 à la Capacité de Combat." },
                new() { Code = "HATE",                 Nom = "Haine",                     MaxFois = null, Empilable = true,  Description = "Une haine féroce vous anime contre un ennemi.", Effet = "+10 CC contre le groupe haï et immunité à la Peur de ce groupe." },
                new() { Code = "INVOCATION",           Nom = "Invocation",                MaxFois = 1, Empilable = false, Description = "Vous pouvez invoquer les miracles de votre dieu.", Effet = "Accès aux Bénédictions et Miracles de votre divinité." },
                new() { Code = "MAGIE_ARCANE",         Nom = "Magie Arcane",              MaxFois = 1, Empilable = false, Description = "Vous maîtrisez les arcanes d'un Domaine de Magie.", Effet = "Accès aux sorts d'un Domaine arcanique spécifique." },
                new() { Code = "MAGIE_MINEUR",         Nom = "Magie Mineure",             MaxFois = 1, Empilable = false, Description = "Vous connaissez les sorts mineurs du Vieux Monde.", Effet = "Accès aux sorts de Magie Mineure." },
                new() { Code = "NUIT",                 Nom = "Vision Nocturne",           MaxFois = 1, Empilable = false, Description = "Vous voyez clairement dans la pénombre.", Effet = "Ignore les pénalités de visibilité en cas de faible luminosité." },
                new() { Code = "RESISTANCE_MALADIE",   Nom = "Résistance aux Maladies",   MaxFois = null, Empilable = true,  Description = "Votre corps résiste aux infections.", Effet = "+10 aux tests de Résistance contre les maladies." },
                new() { Code = "RUSE",                 Nom = "Ruse",                      MaxFois = 1, Empilable = false, Description = "Vous connaissez les codes du milieu.", Effet = "+10 aux tests d'Intuition dans les milieux criminels." },
                new() { Code = "TUEUR_NE",             Nom = "Tueur-Né",                  MaxFois = 1, Empilable = false, Description = "Vous avez juré de mourir au combat.", Effet = "Peut dépenser la Résolution pour ignorer une Blessure Critique." },
                // --- Talents ajoutés depuis le Livre de Base (carrières niveau 1) ---
                new() { Code = "BARAGOUINER",          Nom = "Baragouiner",               MaxFois = 1, Empilable = false, Description = "Vous noyez vos interlocuteurs sous un flot de paroles.", Effet = "Test de Charme Opposé pour confondre un interlocuteur pendant 1 round." },
                new() { Code = "SOCIABLE",             Nom = "Sociable",                  MaxFois = 1, Empilable = false, Description = "Vous vous intégrez facilement dans n'importe quel groupe.", Effet = "+10 aux tests de Commérages." },
                new() { Code = "FAIRE_MANCHE",         Nom = "Faire la Manche",           MaxFois = 1, Empilable = false, Description = "Vous savez apitoyer les passants pour obtenir l'aumône.", Effet = "Gagnez 1d10 sous de cuivre par heure de mendicité." },
                new() { Code = "ARTISTIQUE",           Nom = "Artistique",                MaxFois = 1, Empilable = false, Description = "Vous avez un don inné pour les arts.", Effet = "+10 aux tests d'Art." },
                new() { Code = "ARTISAN_TALENT",       Nom = "Artisan",                   MaxFois = null, Empilable = true, Description = "Vous maîtrisez un métier artisanal.", Effet = "+10 aux tests de Métier pour la spécialité choisie." },
                new() { Code = "DOS_SOLIDE",           Nom = "Dos Solide",                MaxFois = 1, Empilable = false, Description = "Vous portez de lourdes charges sans difficulté.", Effet = "+1 à l'Encombrement maximum transportable." },
                new() { Code = "TRES_FORT",            Nom = "Très Fort",                 MaxFois = 1, Empilable = false, Description = "Votre force physique est exceptionnelle.", Effet = "+5 à la Force." },
                new() { Code = "CHAT_GOUTTIERE",       Nom = "Chat de Gouttière",         MaxFois = 1, Empilable = false, Description = "Les rues et ruelles n'ont aucun secret pour vous.", Effet = "+10 aux tests de Discrétion en milieu urbain." },
                new() { Code = "INDIGNE_INTERET",      Nom = "Indigne d'Intérêt",         MaxFois = 1, Empilable = false, Description = "Vous passez inaperçu dans la foule.", Effet = "Les gardes et autorités vous ignorent sauf raison spécifique." },
                new() { Code = "ETIQUETTE",            Nom = "Étiquette",                 MaxFois = null, Empilable = true, Description = "Vous connaissez les codes sociaux d'un milieu.", Effet = "+10 aux interactions sociales dans le milieu choisi." },
                new() { Code = "COSTAUD",              Nom = "Costaud",                   MaxFois = 1, Empilable = false, Description = "Vous êtes compact et difficile à renverser.", Effet = "+10 aux tests pour résister aux bousculades et renversements." },
                new() { Code = "SOUPE_CAILLOUX",       Nom = "Soupe de Cailloux",         MaxFois = 1, Empilable = false, Description = "Vous survivez avec presque rien.", Effet = "Vous trouvez toujours de quoi manger, même sans ressources." },
                new() { Code = "CONCOCTER",            Nom = "Concocter",                 MaxFois = 1, Empilable = false, Description = "Vous préparez remèdes et potions.", Effet = "Permet de fabriquer des drogues et potions avec un test de Métier." },
                new() { Code = "ARTILLEUR",            Nom = "Artilleur",                 MaxFois = 1, Empilable = false, Description = "Vous maniez les armes de siège.", Effet = "+10 aux tests d'utilisation d'engins de siège et canons." },
                new() { Code = "BRICOLEUR",            Nom = "Bricoleur",                 MaxFois = 1, Empilable = false, Description = "Vous réparez et améliorez les objets.", Effet = "+10 aux tests de Métier pour réparer des objets." },
                new() { Code = "LECTURE_RAPIDE",       Nom = "Lecture Rapide",            MaxFois = 1, Empilable = false, Description = "Vous dévorez les textes à grande vitesse.", Effet = "Temps de Recherche divisé par deux." },
                new() { Code = "BENI",                 Nom = "Béni",                      MaxFois = null, Empilable = true, Description = "Vous avez reçu la bénédiction d'une divinité.", Effet = "Accès aux Bénédictions de votre divinité." },
                new() { Code = "SAINTE_VISION",        Nom = "Sainte Vision",             MaxFois = 1, Empilable = false, Description = "Vous recevez des visions de votre divinité.", Effet = "Le MJ peut accorder des visions divines guidant le personnage." },
                new() { Code = "CHARMANT",             Nom = "Charmant",                  MaxFois = 1, Empilable = false, Description = "Votre charisme magnétique attire les gens.", Effet = "+10 aux tests de Charme envers le sexe opposé ou le genre attirant." },
                new() { Code = "PORTER_TOAST",         Nom = "Porter un Toast",           MaxFois = 1, Empilable = false, Description = "Vous animez les beuveries comme personne.", Effet = "+10 aux tests de Résistance à l'Alcool et de Commérages lors de fêtes." },
                new() { Code = "CALCULATEUR",          Nom = "Calculateur Mental",        MaxFois = 1, Empilable = false, Description = "Vous manipulez les chiffres mentalement.", Effet = "Permet les calculs complexes sans matériel, +10 aux tests liés aux nombres." },
                new() { Code = "HARMONISATION",        Nom = "Harmonisation Aethyrique",  MaxFois = 1, Empilable = false, Description = "Vous percevez les Vents de Magie.", Effet = "Peut détecter la présence de magie avec un test d'Intuition." },
                new() { Code = "SECONDE_VUE",          Nom = "Seconde Vue",               MaxFois = 1, Empilable = false, Description = "Vous voyez le surnaturel invisible aux autres.", Effet = "Peut voir les manifestations magiques et les créatures éthérées." },
                new() { Code = "BOUQUINEUR",           Nom = "Bouquineur",                MaxFois = 1, Empilable = false, Description = "Vous tirez le maximum des textes écrits.", Effet = "+10 aux tests de Recherche." },
                new() { Code = "PANSEMENT_TERRAIN",    Nom = "Pansement de Terrain",      MaxFois = 1, Empilable = false, Description = "Vous prodiguez les premiers soins efficacement.", Effet = "+10 aux tests de Soins pour les premiers secours." },
                new() { Code = "COUP_ASSOMMANT",       Nom = "Coup Assommant",            MaxFois = 1, Empilable = false, Description = "Vous frappez pour assommer, pas pour tuer.", Effet = "Peut assommer un adversaire sans le blesser gravement." },
                new() { Code = "VIF_ESPRIT",           Nom = "Vif d'Esprit",              MaxFois = 1, Empilable = false, Description = "Vous analysez vite les situations.", Effet = "+5 à l'Initiative." },
                new() { Code = "PERCE_LIGNE",          Nom = "Perce la Ligne",            MaxFois = 1, Empilable = false, Description = "Votre entraînement militaire est rigoureux.", Effet = "+10 aux tests de Commandement pour maintenir une formation." },
                new() { Code = "TENACE",               Nom = "Tenace",                    MaxFois = 1, Empilable = false, Description = "Votre détermination est sans faille.", Effet = "+10 aux tests de Calme pour résister à la fatigue et au découragement." },
                new() { Code = "NEGOCIATEUR",          Nom = "Négociateur",               MaxFois = 1, Empilable = false, Description = "Vous obtenez toujours le meilleur prix.", Effet = "+10 aux tests de Marchandage." },
                new() { Code = "ORIENTATION",          Nom = "Orientation",               MaxFois = 1, Empilable = false, Description = "Vous retrouvez toujours votre chemin.", Effet = "+10 aux tests de Navigation." },
                new() { Code = "VAGABOND",             Nom = "Vagabond",                  MaxFois = 1, Empilable = false, Description = "Vous êtes habitué à la vie sur les routes.", Effet = "Ignore les pénalités de voyage à pied prolongé." },
                new() { Code = "MARCHEUR",             Nom = "Marcheur",                  MaxFois = null, Empilable = true, Description = "Vous traversez un terrain difficile sans ralentir.", Effet = "Ignore les pénalités de Mouvement dans le terrain choisi." },
                new() { Code = "MENACANT",             Nom = "Menaçant",                  MaxFois = 1, Empilable = false, Description = "Votre seule présence inspire la crainte.", Effet = "+10 aux tests d'Intimidation." },
                new() { Code = "DETOURNER",            Nom = "Détourner",                 MaxFois = 1, Empilable = false, Description = "Vous subtilisez des fonds discrètement.", Effet = "Peut détourner des fonds lors de transactions financières." },
                new() { Code = "NUMISMATIQUE",         Nom = "Numismatique",              MaxFois = 1, Empilable = false, Description = "Vous identifiez les pièces et leur valeur.", Effet = "+10 aux tests d'Évaluation des devises et détection de fausse monnaie." },
                new() { Code = "FRAPPER_LAME",         Nom = "Frapper la Lame",           MaxFois = 1, Empilable = false, Description = "Vous déviez la lame adversaire d'un coup sec.", Effet = "Action : test de Mêlée Opposé pour désarmer l'adversaire." },
                new() { Code = "DISTRAIRE",            Nom = "Distraire",                 MaxFois = 1, Empilable = false, Description = "Vous détournez l'attention de vos adversaires.", Effet = "Test de Charme Opposé pour faire perdre l'Avantage à un ennemi." },
                new() { Code = "FEINTER",              Nom = "Feinter",                   MaxFois = 1, Empilable = false, Description = "Vos feintes trompent les défenses adverses.", Effet = "Test de Mêlée Opposé pour ignorer les bonus défensifs de l'adversaire." },
                new() { Code = "PAS_DE_COTE",          Nom = "Pas de Côté",               MaxFois = 1, Empilable = false, Description = "Vous esquivez avec une grâce surnaturelle.", Effet = "Test d'Esquive supplémentaire gratuit par round." },
                new() { Code = "AFFINITE_ANIMALE",     Nom = "Affinité Animale",          MaxFois = 1, Empilable = false, Description = "Les animaux vous font naturellement confiance.", Effet = "+10 aux tests d'Emprise sur les Animaux et de Soins des Animaux." },
                new() { Code = "VOYAGEUR",             Nom = "Voyageur Expérimenté",      MaxFois = 1, Empilable = false, Description = "Les voyages n'ont plus de secrets pour vous.", Effet = "+10 aux tests de Survie lors de longs trajets." },
                new() { Code = "ASTUCE_EQUITATION",    Nom = "Astuce d'Équitation",       MaxFois = 1, Empilable = false, Description = "Vous réalisez des prouesses équestres.", Effet = "+10 aux tests d'Équitation pour les manœuvres complexes." },
                new() { Code = "ATTRAYANT",            Nom = "Attrayant",                 MaxFois = 1, Empilable = false, Description = "Votre apparence physique est séduisante.", Effet = "+5 aux tests de Charme basés sur l'apparence." },
                new() { Code = "IMITATEUR",            Nom = "Imitateur",                 MaxFois = 1, Empilable = false, Description = "Vous reproduisez voix et sons avec précision.", Effet = "+10 aux tests de Représentation pour imiter quelqu'un." },
                new() { Code = "ORATEUR_PUBLIC",       Nom = "Orateur Public",            MaxFois = 1, Empilable = false, Description = "Vous captivez les foules par vos discours.", Effet = "+10 aux tests de Charme devant un public nombreux." },
                new() { Code = "CHARGE_BERSERK",       Nom = "Charge Berserk",            MaxFois = 1, Empilable = false, Description = "Vous chargez l'ennemi avec une rage folle.", Effet = "+Bonus de Force aux dégâts quand vous chargez en Frénésie." },
                new() { Code = "FUYEZ",                Nom = "Fuyez!",                    MaxFois = 1, Empilable = false, Description = "Vous excellez dans l'art de la retraite rapide.", Effet = "+10 aux tests d'Athlétisme pour fuir le combat." },
                new() { Code = "SPRINTEUR",            Nom = "Sprinteur",                 MaxFois = 1, Empilable = false, Description = "Votre vitesse de pointe est impressionnante.", Effet = "+1 au Mouvement lors d'un sprint." },
                new() { Code = "PECHEUR",              Nom = "Pêcheur",                   MaxFois = 1, Empilable = false, Description = "Vous attrapez du poisson avec aisance.", Effet = "Peut se nourrir en pêchant sans test, +10 si test requis." },
                new() { Code = "BON_TIREUR",           Nom = "Bon Tireur",                MaxFois = 1, Empilable = false, Description = "Votre précision au tir est remarquable.", Effet = "+5 à la Capacité de Tir." },
                new() { Code = "CRIMINEL",             Nom = "Criminel",                  MaxFois = 1, Empilable = false, Description = "Vous connaissez les codes du milieu criminel.", Effet = "+10 aux tests de Commérages et d'Intuition dans le milieu criminel." },
                new() { Code = "APNEISTE",             Nom = "Apnéiste",                  MaxFois = 1, Empilable = false, Description = "Vous retenez votre souffle très longtemps.", Effet = "Double la durée de rétention du souffle sous l'eau." },
                new() { Code = "MARIN",                Nom = "Marin",                     MaxFois = 1, Empilable = false, Description = "Vous êtes à l'aise sur et dans l'eau.", Effet = "+10 aux tests de Canotage et de Voile." },
                new() { Code = "SANG_NOBLE",           Nom = "Sang Noble",                MaxFois = 1, Empilable = false, Description = "Vous êtes de sang noble.", Effet = "Statut social reconnu : accès aux cercles aristocratiques." },
                new() { Code = "CONSCIENT_COMBAT",     Nom = "Conscient du Combat",       MaxFois = 1, Empilable = false, Description = "Vous lisez la bataille comme un livre.", Effet = "+10 aux tests de Perception en combat." },
                new() { Code = "CLAQUER_FOUET",        Nom = "Claquer le Fouet",          MaxFois = 1, Empilable = false, Description = "Vous motivez les montures et bêtes de trait.", Effet = "+10 aux tests de Conduite et d'Équitation pour accélérer." },
                new() { Code = "CAVALIER_RIGOUREUX",   Nom = "Cavalier Rigoureux",        MaxFois = 1, Empilable = false, Description = "Vous tenez en selle même dans les pires conditions.", Effet = "+10 aux tests d'Équitation en terrain difficile." },
                new() { Code = "TRICHEUR",             Nom = "Tricheur",                  MaxFois = 1, Empilable = false, Description = "Vous trichez aux cartes sans vous faire prendre.", Effet = "+10 aux tests de Jeu impliquant des cartes." },
                new() { Code = "JOUEUR_DES",           Nom = "Joueur de Dés",             MaxFois = 1, Empilable = false, Description = "Vous maîtrisez les jeux de dés.", Effet = "+10 aux tests de Jeu impliquant des dés." },
                new() { Code = "ENTREE_EFFRACTION",    Nom = "Entrée par Effraction",     MaxFois = 1, Empilable = false, Description = "Vous pénétrez partout sans laisser de traces.", Effet = "+10 aux tests de Crochetage et d'Escalade pour entrer quelque part." },
                new() { Code = "OMBRE",                Nom = "Je suis une Ombre",         MaxFois = 1, Empilable = false, Description = "Vous vous fondez dans les ombres.", Effet = "+10 aux tests de Discrétion la nuit ou dans l'obscurité." },
                new() { Code = "OBSERVER",             Nom = "Observer",                  MaxFois = 1, Empilable = false, Description = "Vous remarquez les comportements suspects.", Effet = "+10 aux tests d'Intuition pour détecter les menteurs." },
                new() { Code = "DICTION_INSTINCTIVE",  Nom = "Diction Instinctive",       MaxFois = 1, Empilable = false, Description = "Vous lancez des sorts de manière intuitive.", Effet = "Peut lancer des sorts sans composante verbale." },
                new() { Code = "COUPS_BAS",            Nom = "Coups Bas",                 MaxFois = 1, Empilable = false, Description = "Vous n'hésitez pas à frapper sous la ceinture.", Effet = "+10 aux tests de Mêlée (Bagarre) pour les coups vicieux." },
                new() { Code = "MACHOIRE_ACIER",       Nom = "Mâchoire d'Acier",          MaxFois = 1, Empilable = false, Description = "Vous encaissez les coups au visage sans broncher.", Effet = "+10 aux tests de Résistance contre les coups critiques à la tête." },
                new() { Code = "RENVERSEMENT",         Nom = "Renversement",              MaxFois = 1, Empilable = false, Description = "Vous retournez les attaques contre l'adversaire.", Effet = "Peut utiliser les NS d'Esquive pour contre-attaquer." },
                new() { Code = "TRAPPEUR",             Nom = "Trappeur",                  MaxFois = 1, Empilable = false, Description = "Vous posez et dissimulez des pièges.", Effet = "+10 aux tests de Braconnage pour poser des pièges." },
                new() { Code = "MAINS_RAPIDES",        Nom = "Mains Rapides",             MaxFois = 1, Empilable = false, Description = "Vos gestes sont d'une rapidité déconcertante.", Effet = "+10 aux tests d'Escamotage." },
            };
            foreach (var talent in allTalents)
            {
                if (!existingTalentCodes.Contains(talent.Code))
                    db.Talents.Add(talent);
            }
            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(cancellationToken);
        }

        // Compétences et Talents par niveau de carrière (niveau 1) — WFRP4 Livre de Base
        // Écrase systématiquement pour appliquer les corrections
        {
            var allNiveau1 = await db.NiveauCarrieres
                .Include(n => n.Carriere)
                .Where(n => n.Niveau == 1)
                .ToListAsync(cancellationToken);

            var data = new Dictionary<string, (string Competences, string Talents)>
            {
                // --- CITOYEN ---
                ["AGITATEUR"]          = ("ART,CORRUPTION,CHARME,RESISTANCE_ALCOOL,COMMERAGE,MARCHANDAGE,CONNAISSANCES,METIER", "BARAGOUINER,SOCIABLE,FAIRE_MANCHE,LECTURE_ECRITURE"),
                ["ARTISAN"]            = ("ATHLETISME,CALME,RESISTANCE_ALCOOL,ESQUIVE,RESISTANCE,EVALUATION,DISCRETION,METIER", "ARTISTIQUE,ARTISAN_TALENT,DOS_SOLIDE,TRES_FORT"),
                ["CITADIN"]            = ("CHARME,ESCALADE,RESISTANCE_ALCOOL,CONDUITE,ESQUIVE,JEU,COMMERAGE,MARCHANDAGE", "CHAT_GOUTTIERE,INDIGNE_INTERET,ETIQUETTE,COSTAUD"),
                ["ENQUETEUR"]          = ("CHARME,ESCALADE,CALME,COMMERAGE,PERCEPTION,DISCRETION,PISTAGE,INTUITION", "CHAT_GOUTTIERE,INDIGNE_INTERET,LECTURE_ECRITURE,VIF_ESPRIT"),
                ["MARCHAND"]           = ("SOINS_ANIMAUX,CORRUPTION,CHARME,RESISTANCE_ALCOOL,CONDUITE,JEU,COMMERAGE,MARCHANDAGE", "BARAGOUINER,NEGOCIATEUR,LECTURE_ECRITURE,CHARMANT"),
                ["MENDIANT"]           = ("ATHLETISME,CHARME,RESISTANCE_ALCOOL,CALME,ESQUIVE,RESISTANCE,INTUITION,DISCRETION", "FAIRE_MANCHE,RESISTANCE_MALADIE,SOUPE_CAILLOUX,TRES_RESISTANT"),
                ["RATIER"]             = ("ATHLETISME,DRESSAGE,EMPRISE_ANIMAUX,RESISTANCE_ALCOOL,RESISTANCE,MELEE,A_DISTANCE,DISCRETION", "NUIT,RESISTANCE_MALADIE,ATTAQUE_PUISSANTE,COUP_ASSOMMANT"),
                ["SENTINELLE"]         = ("ATHLETISME,ESCALADE,RESISTANCE_ALCOOL,ESQUIVE,RESISTANCE,JEU,MELEE,PERCEPTION", "PERCE_LIGNE,ROBUSTE,COUP_ASSOMMANT,TENACE"),
                // --- COURTISAN ---
                ["ARTISTE"]            = ("ART,CALME,RESISTANCE_ALCOOL,EVALUATION,RESISTANCE,COMMERAGE,PERCEPTION,DISCRETION", "ARTISTIQUE,VIF_ESPRIT,DOS_SOLIDE,TENACE"),
                ["CONSEILLER"]         = ("CORRUPTION,RESISTANCE_ALCOOL,RESISTANCE,COMMERAGE,MARCHANDAGE,LANGUE,CONNAISSANCES,PERCEPTION", "INDIGNE_INTERET,ETIQUETTE,SOCIABLE,LECTURE_ECRITURE"),
                ["DUELLISTE"]          = ("ATHLETISME,ESQUIVE,RESISTANCE,SOINS,INTUITION,LANGUE,MELEE,PERCEPTION", "FRAPPER_LAME,DISTRAIRE,FEINTER,PAS_DE_COTE"),
                ["EMISSAIRE"]          = ("ATHLETISME,CHARME,CONDUITE,ESQUIVE,RESISTANCE,INTUITION,EQUITATION,CANOTAGE", "BARAGOUINER,ETIQUETTE,LECTURE_ECRITURE,CHARMANT"),
                ["ESPION"]             = ("COMMERAGE,CORRUPTION,CHARME,CALME,JEU,MARCHANDAGE,PERCEPTION,DISCRETION", "BARAGOUINER,PORTER_TOAST,SOCIABLE,OBSERVER"),
                ["INTENDANT"]          = ("ATHLETISME,EMPRISE_ANIMAUX,RESISTANCE_ALCOOL,CALME,RESISTANCE,INTUITION,CONNAISSANCES,PERCEPTION", "MENACANT,NUIT,VIF_ESPRIT,COUP_ASSOMMANT"),
                ["NOBLE"]              = ("CORRUPTION,RESISTANCE_ALCOOL,JEU,INTIMIDATION,COMMANDEMENT,CONNAISSANCES,MELEE,JOUER", "ETIQUETTE,CHANCE,SANG_NOBLE,LECTURE_ECRITURE"),
                ["SERVITEUR"]          = ("ATHLETISME,ESCALADE,CONDUITE,ESQUIVE,RESISTANCE,INTUITION,PERCEPTION,DISCRETION", "INDIGNE_INTERET,DOS_SOLIDE,CARACTERE_AFFIRME,COSTAUD"),
                // --- FILOU ---
                ["CHARLATAN"]          = ("CORRUPTION,RESISTANCE_ALCOOL,CHARME,CALME,JEU,COMMERAGE,MARCHANDAGE,ESCAMOTAGE", "CHANCE,TRICHEUR,JOUEUR_DES,ETIQUETTE"),
                ["HORS_LOI"]           = ("ATHLETISME,RESISTANCE_ALCOOL,CALME,RESISTANCE,JEU,INTIMIDATION,MELEE,SURVIE", "CONSCIENT_COMBAT,CRIMINEL,VAGABOND,FUYEZ"),
                ["PILLEUR_TOMBES"]     = ("ESCALADE,INTUITION,PERCEPTION,CALME,DISCRETION,ESQUIVE,RESISTANCE,COMMERAGE", "CHAT_GOUTTIERE,CRIMINEL,FUYEZ,DOS_SOLIDE"),
                ["RACKETTEUR"]         = ("RESISTANCE_ALCOOL,CALME,ESQUIVE,RESISTANCE,INTIMIDATION,CONNAISSANCES,MELEE,DISCRETION", "CRIMINEL,ETIQUETTE,MENACANT,ATTAQUE_PUISSANTE"),
                ["RECELEUR"]           = ("CHARME,RESISTANCE_ALCOOL,ESQUIVE,EVALUATION,JEU,COMMERAGE,MARCHANDAGE,MELEE", "CHAT_GOUTTIERE,TRICHEUR,NEGOCIATEUR,SOCIABLE"),
                ["SORCIERE"]           = ("CANALISATION,CALME,RESISTANCE,DISCRETION,LANGUE,COMMERAGE,INTIMIDATION,ESCAMOTAGE", "CRIMINEL,DICTION_INSTINCTIVE,MENACANT,MAGIE_MINEUR"),
                ["TRAFIQUANT"]         = ("CORRUPTION,CHARME,RESISTANCE_ALCOOL,INTERPRETATION,JEU,COMMERAGE,MARCHANDAGE,INTIMIDATION", "ATTRAYANT,CHAT_GOUTTIERE,BARAGOUINER,SOCIABLE"),
                ["VOLEUR"]             = ("ATHLETISME,ESCALADE,CALME,ESQUIVE,RESISTANCE,INTUITION,PERCEPTION,DISCRETION", "CHAT_GOUTTIERE,CRIMINEL,FUYEZ,COUP_ASSOMMANT"),
                // --- FORESTIER ---
                ["AMUSEUR"]            = ("ATHLETISME,CHARME,INTERPRETATION,COMMERAGE,MARCHANDAGE,REPRESENTATION,JOUER,ESCAMOTAGE", "ATTRAYANT,IMITATEUR,ORATEUR_PUBLIC,CHARMANT"),
                ["CHASSEUR_PRIMES"]    = ("CORRUPTION,CHARME,COMMERAGE,MARCHANDAGE,INTUITION,MELEE,SURVIE,PERCEPTION", "ENTREE_EFFRACTION,OMBRE,COUP_ASSOMMANT,CHARMANT"),
                ["CHASSEUR_SORCIERES"] = ("CHARME,RESISTANCE_ALCOOL,SOINS,INTIMIDATION,INTUITION,CONNAISSANCES,MELEE,PERCEPTION", "COOLHEAD,MENACANT,LECTURE_ECRITURE,DETERMINATION"),
                ["COCHER"]             = ("SOINS_ANIMAUX,EMPRISE_ANIMAUX,ESCALADE,CONDUITE,RESISTANCE,PERCEPTION,A_DISTANCE,EQUITATION", "AFFINITE_ANIMALE,VOYAGEUR,ASTUCE_EQUITATION,TENACE"),
                ["COLPORTEUR"]         = ("CHARME,RESISTANCE,INTERPRETATION,COMMERAGE,MARCHANDAGE,INTUITION,SURVIE,DISCRETION", "PECHEUR,FUYEZ,VAGABOND,BRICOLEUR"),
                ["FLAGELLANT"]         = ("ESQUIVE,RESISTANCE,SOINS,INTIMIDATION,INTUITION,CONNAISSANCES,MELEE,SURVIE", "CHARGE_BERSERK,FRENESIE,LECTURE_ECRITURE,SOUPE_CAILLOUX"),
                ["MESSAGER"]           = ("ATHLETISME,ESCALADE,ESQUIVE,RESISTANCE,COMMERAGE,NAVIGATION,PERCEPTION,MELEE", "FUYEZ,PIED_LEGER,SPRINTEUR,PAS_DE_COTE"),
                ["PATROUILLEUR"]       = ("CORRUPTION,RESISTANCE_ALCOOL,JEU,COMMERAGE,MARCHANDAGE,MELEE,PERCEPTION,A_DISTANCE", "COOLHEAD,DETOURNER,BON_TIREUR,NUMISMATIQUE"),
                // --- GUERRIER ---
                ["CAVALIER"]           = ("SOINS_ANIMAUX,EMPRISE_ANIMAUX,RESISTANCE,LANGUE,MELEE,SURVIE,EQUITATION,PERCEPTION", "CONSCIENT_COMBAT,CLAQUER_FOUET,VIFS_REFLEXES,CAVALIER_RIGOUREUX"),
                ["CHEVALIER"]          = ("ATHLETISME,SOINS_ANIMAUX,EMPRISE_ANIMAUX,SOINS,CONNAISSANCES,MELEE,EQUITATION,METIER", "ETIQUETTE,CAVALIER_RIGOUREUX,COSTAUD,GUERRIER_NE"),
                ["GARDE"]              = ("RESISTANCE_ALCOOL,RESISTANCE,INTERPRETATION,JEU,COMMERAGE,INTUITION,MELEE,PERCEPTION", "JOUEUR_DES,ETIQUETTE,COUP_ASSOMMANT,TENACE"),
                ["GLADIATEUR"]         = ("ATHLETISME,CALME,ESQUIVE,RESISTANCE,JEU,INTIMIDATION,MELEE", "COUPS_BAS,COMBAT_RAPPROCHE,MACHOIRE_ACIER,RENVERSEMENT"),
                ["GROS_BRAS"]          = ("ATHLETISME,ESQUIVE,RESISTANCE,INTERPRETATION,COMMERAGE,MARCHANDAGE,INTIMIDATION,MELEE", "COMBAT_RAPPROCHE,COUPS_BAS,MENACANT,GUERRIER_NE"),
                ["PRETRE_GUERRIER"]    = ("CALME,ESQUIVE,RESISTANCE,SOINS,COMMANDEMENT,CONNAISSANCES,MELEE,PRIER", "BENI,ETIQUETTE,LECTURE_ECRITURE,CARACTERE_AFFIRME"),
                ["SOLDAT"]             = ("ATHLETISME,ESCALADE,CALME,ESQUIVE,RESISTANCE,LANGUE,MELEE,JOUER", "JOUEUR_DES,BON_TIREUR,DOS_SOLIDE,GUERRIER_NE"),
                ["TUEUR"]              = ("RESISTANCE_ALCOOL,CALME,ESQUIVE,RESISTANCE,JEU,SOINS,CONNAISSANCES,MELEE", "DEUX_ARMES,INTREPIDE,FRENESIE,TUEUR_NE"),
                // --- LETTRE ---
                ["APOTHICAIRE"]        = ("RESISTANCE_ALCOOL,SOINS,LANGUE,CONNAISSANCES,METIER", "CONCOCTER,ARTISAN_TALENT,ETIQUETTE,LECTURE_ECRITURE"),
                ["AVOCAT"]             = ("RESISTANCE_ALCOOL,RESISTANCE,MARCHANDAGE,LANGUE,CONNAISSANCES,PERCEPTION,RECHERCHE", "BARAGOUINER,ETIQUETTE,LECTURE_ECRITURE,LECTURE_RAPIDE"),
                ["ERUDIT"]             = ("RESISTANCE_ALCOOL,INTERPRETATION,JEU,COMMERAGE,MARCHANDAGE,LANGUE,CONNAISSANCES,RECHERCHE", "PORTER_TOAST,LECTURE_ECRITURE,SAVOIR,CALCULATEUR"),
                ["INGENIEUR"]          = ("RESISTANCE_ALCOOL,CALME,RESISTANCE,LANGUE,CONNAISSANCES,PERCEPTION,METIER,A_DISTANCE", "ARTISTIQUE,ARTILLEUR,LECTURE_ECRITURE,BRICOLEUR"),
                ["MAGE"]               = ("CANALISATION,ESQUIVE,INTUITION,LANGUE,CONNAISSANCES,MELEE,PERCEPTION", "HARMONISATION,MAGIE_MINEUR,LECTURE_ECRITURE,SECONDE_VUE"),
                ["MEDECIN"]            = ("CORRUPTION,CALME,CONDUITE,RESISTANCE,COMMERAGE,SOINS,PERCEPTION,ESCAMOTAGE", "BOUQUINEUR,PANSEMENT_TERRAIN,LECTURE_ECRITURE,COUP_ASSOMMANT"),
                ["NONNE"]              = ("ART,CALME,RESISTANCE,INTERPRETATION,COMMERAGE,SOINS,CONNAISSANCES,PRIER", "BENI,SOUPE_CAILLOUX,FAIRE_MANCHE,LECTURE_ECRITURE"),
                ["PRETRE"]             = ("ATHLETISME,CALME,RESISTANCE,INTUITION,CONNAISSANCES,PERCEPTION,PRIER,RECHERCHE", "BENI,SAINTE_VISION,LECTURE_ECRITURE,CHARMANT"),
                // --- PAYSAN ---
                ["CHASSEUR"]           = ("EMPRISE_ANIMAUX,ESCALADE,RESISTANCE,CONNAISSANCES,SURVIE,PERCEPTION,A_DISTANCE,BRACONNAGE", "ROBUSTE,VAGABOND,MARCHEUR,TRAPPEUR"),
                ["ECLAIREUR"]          = ("EMPRISE_ANIMAUX,ESCALADE,RESISTANCE,COMMERAGE,CONNAISSANCES,MELEE,SURVIE,PERCEPTION", "ORIENTATION,VAGABOND,VIF_ESPRIT,MARCHEUR"),
                ["HERBORISTE"]         = ("EMPRISE_ANIMAUX,ESCALADE,RESISTANCE,CONNAISSANCES,SURVIE,PERCEPTION,NATATION,METIER", "ACUITE,ORIENTATION,VAGABOND,MARCHEUR"),
                ["HUISSIER"]           = ("CALME,ESQUIVE,RESISTANCE,COMMERAGE,MARCHANDAGE,INTIMIDATION,MELEE,PERCEPTION", "DETOURNER,NUMISMATIQUE,DOS_SOLIDE,TENACE"),
                ["MINEUR"]             = ("CALME,RESISTANCE,INTUITION,CONNAISSANCES,MELEE,SURVIE,PERCEPTION,NATATION", "VAGABOND,MARCHEUR,COSTAUD,TENACE"),
                ["MYSTIQUE"]           = ("CHARME,INTERPRETATION,ESQUIVE,COMMERAGE,MARCHANDAGE,INTUITION,PERCEPTION,ESCAMOTAGE", "ATTRAYANT,CHANCE,SECONDE_VUE,CHARMANT"),
                ["SORCIER_VILLAGE"]    = ("CANALISATION,RESISTANCE,LANGUE,INTUITION,CONNAISSANCES,SURVIE,PERCEPTION", "MAINS_RAPIDES,MAGIE_MINEUR,VAGABOND,MARCHEUR"),
                ["VILLAGEOIS"]         = ("SOINS_ANIMAUX,ATHLETISME,RESISTANCE_ALCOOL,RESISTANCE,COMMERAGE,MELEE,CONNAISSANCES,SURVIE", "VAGABOND,DOS_SOLIDE,CARACTERE_AFFIRME,SOUPE_CAILLOUX"),
                // --- RIVERAIN ---
                ["BATELIER"]           = ("RESISTANCE_ALCOOL,ESQUIVE,RESISTANCE,COMMERAGE,MELEE,CANOTAGE,VOILE,NATATION", "COUPS_BAS,PECHEUR,DOS_SOLIDE,APNEISTE"),
                ["CONTREBANDIER"]      = ("ATHLETISME,CORRUPTION,CALME,RESISTANCE_ALCOOL,CANOTAGE,VOILE,DISCRETION,NATATION", "CRIMINEL,PECHEUR,MARCHEUR,DOS_SOLIDE"),
                ["DEBARDEUR"]          = ("ATHLETISME,ESCALADE,RESISTANCE_ALCOOL,ESQUIVE,RESISTANCE,COMMERAGE,MELEE,NATATION", "COUPS_BAS,DOS_SOLIDE,COSTAUD,TRES_FORT"),
                ["MATELOT"]            = ("ESCALADE,RESISTANCE_ALCOOL,JEU,COMMERAGE,CANOTAGE,MELEE,VOILE,NATATION", "PECHEUR,MARCHEUR,DOS_SOLIDE,APNEISTE"),
                ["NAUFRAGEUR"]         = ("ESCALADE,RESISTANCE_ALCOOL,ESQUIVE,RESISTANCE,CANOTAGE,MELEE,SURVIE,NATATION", "ENTREE_EFFRACTION,CRIMINEL,PECHEUR,DOS_SOLIDE"),
                ["PATROUILLEUR_FLUVIAL"] = ("ATHLETISME,ESQUIVE,RESISTANCE,MELEE,PERCEPTION,CANOTAGE,VOILE,NATATION", "APNEISTE,DOS_SOLIDE,TRES_FORT,MARIN"),
                ["PILOTE"]             = ("CONNAISSANCES,PERCEPTION,CANOTAGE,NATATION,RESISTANCE_ALCOOL,COMMERAGE,INTUITION", "PECHEUR,NUIT,ORIENTATION,MARIN"),
                ["RIVERAINE"]          = ("ATHLETISME,RESISTANCE_ALCOOL,ESQUIVE,RESISTANCE,COMMERAGE,SURVIE,CANOTAGE,NATATION", "PECHEUR,SOCIABLE,MARCHEUR,APNEISTE"),
            };

            foreach (var niveau in allNiveau1)
            {
                if (data.TryGetValue(niveau.Carriere.Code, out var d))
                {
                    niveau.CompetenceRevenu = d.Competences;
                    niveau.TalentsRevenu = d.Talents;
                }
            }

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(cancellationToken);
        }

        // Dotations par niveau de carrière (niveau 1) — WFRP4 Livre de Base
        {
            var allNiveau1 = await db.NiveauCarrieres
                .Include(n => n.Carriere)
                .Where(n => n.Niveau == 1)
                .ToListAsync(cancellationToken);

            var dotations = new Dictionary<string, string>
            {
                // --- CITOYEN ---
                ["AGITATEUR"]          = "Nécessaire d'écriture|Marteau et Clous|Pile de Dépliants",
                ["ARTISAN"]            = "Craie|Veste en Cuir|d10 chiffons",
                ["CITADIN"]            = "Hébergements|Bottes Robustes",
                ["ENQUETEUR"]          = "Lanterne|Huile pour lampe|Journal|Plume d'oie et encre",
                ["MARCHAND"]           = "Boulier|Mule et Charrette|Bâche en Toile|3d10 Argent",
                ["MENDIANT"]           = "Couverture|Gobelet de Mauvaise Qualité",
                ["RATIER"]             = "Fronde avec Munitions|Sac|Petit Chien Vicieux",
                ["SENTINELLE"]         = "Arme de poing|Chemise de Cuir|Uniforme",
                // --- COURTISAN ---
                ["ARTISTE"]            = "Pinceau ou burin ou plume d'oie",
                ["CONSEILLER"]         = "Matériel d'écriture",
                ["DUELLISTE"]          = "Arme de poing ou Rapière|Sac en Bandoulière contenant Vêtements et 1d10 Bandages",
                ["EMISSAIRE"]          = "Veste en cuir|Livrée|Étui à rouleau",
                ["ESPION"]             = "Bâton de charbon de bois|Sac de bandoulière contenant 2 ensembles de vêtements et cape à capuchon",
                ["INTENDANT"]          = "Clés|Lanterne|Huile pour Lampe|Livrée",
                ["NOBLE"]              = "Costume|Miroir à Main|Bijoux d'une Valeur de 3d10 CO|Serviteur Personnel",
                ["SERVITEUR"]          = "Balai",
                // --- FILOU ---
                ["CHARLATAN"]          = "Sac à dos|2 jeux de vêtements|Jeu de cartes|Dés",
                ["HORS_LOI"]           = "Sac de couchage|Arme à une Main|Veste en cuir|Boîte à amadou",
                ["PILLEUR_TOMBES"]     = "Barre à mine|Charrette|Cape|Bâche",
                ["RACKETTEUR"]         = "Coup-de-poings|Chemise en cuir",
                ["RECELEUR"]           = "Arme de poing|Marchandise volée d'une valeur de 3d10 Pistoles",
                ["SORCIERE"]           = "Bougies|Craie|Poupée|Épingles",
                ["TRAFIQUANT"]         = "Bouteille de Spiritueux",
                ["VOLEUR"]             = "Pied de Biche|Veste en Cuir|Sac",
                // --- FORESTIER ---
                ["AMUSEUR"]            = "Bol|Instrument",
                ["CHASSEUR_PRIMES"]    = "Arme de poing|Veste en cuir|Corde",
                ["CHASSEUR_SORCIERES"] = "Armes de poing|Instruments de torture",
                ["COCHER"]             = "Manteau et gants chauds|Fouet",
                ["COLPORTEUR"]         = "Sac à dos|Literie|Marchandises d'une valeur de 2d10 Sous de cuivre|Tente",
                ["FLAGELLANT"]         = "Fléau|Peignoirs en lamelles",
                ["MESSAGER"]           = "Étui à parchemin",
                ["PATROUILLEUR"]       = "Arbalète à 10 carreaux|Veste en cuir",
                // --- GUERRIER ---
                ["CAVALIER"]           = "Chemise en cuir|Cheval d'équitation avec selle et harnais",
                ["CHEVALIER"]          = "Chemise de Cuir|Veste de Maille|Lance|Cheval d'équitation avec selle et Harnais|Bouclier|Outils de Métier (Maréchal-Ferrant)",
                ["GARDE"]              = "Rondache|Veste en cuir|Lampe Tempête avec huile",
                ["GLADIATEUR"]         = "Bandages|Coup de Poing en Acier|Veste en cuir",
                ["GROS_BRAS"]          = "Capuche ou masque|Coup-de-Poing|Chemise en cuir",
                ["PRETRE_GUERRIER"]    = "Livre (Religion)|Veste en cuir|Symbole religieux|Robes|Arme de Mêlée (Choix)",
                ["SOLDAT"]             = "Dague|Plastron en Cuir Poli|Uniforme",
                ["TUEUR"]              = "Hache|Flacon de Spiritueux|Tatouages",
                // --- LETTRE ---
                ["APOTHICAIRE"]        = "Livre (vierge)|Breuvage Curatif|Chemise en cuir|Pilon et Mortier",
                ["AVOCAT"]             = "Livre (Droit)|Loupe",
                ["ERUDIT"]             = "Alcool|Livre|Matériel d'Écriture",
                ["INGENIEUR"]          = "Livre (Ingénieur)|Marteau et clous",
                ["MAGE"]               = "Grimoire|Grand Bâton",
                ["MEDECIN"]            = "Bandages|Breuvage de Guérison",
                ["NONNE"]              = "Symbole religieux|Robes",
                ["PRETRE"]             = "Symbole Religieux|Robe",
                // --- PAYSAN ---
                ["CHASSEUR"]           = "Sélection de pièges à animaux|Arme de poing|Bottes robustes et manteau|Fronde avec munitions",
                ["ECLAIREUR"]          = "Arme de poing|Veste en cuir|Bottes robustes|Cape et corde",
                ["HERBORISTE"]         = "Bottes|Cape|Sac en bandoulière contenant un assortiment d'herbes médicinales",
                ["HUISSIER"]           = "Arme de Poing|Petit Coffret Verrouillable",
                ["MINEUR"]             = "Bâton de charbon de bois|Carte de gisement|Pelle|Pioche",
                ["MYSTIQUE"]           = "Jeu de Cartes ou de Dés|Bijoux pas Chers",
                ["SORCIER_VILLAGE"]    = "1d10 Porte-bonheur|Bâton|Sac à dos",
                ["VILLAGEOIS"]         = "",
                // --- RIVERAIN ---
                ["BATELIER"]           = "Arme de Poing|Chemise en Cuir|Perche",
                ["CONTREBANDIER"]      = "Grand Sac|Masque ou Foulard|Poudrière|Lanterne de Tempête et Huile",
                ["DEBARDEUR"]          = "Arme de Poing (Crochet d'Amarrage)|Gants de Cuir",
                ["MATELOT"]            = "Seau|Brosse|Balai à Franges",
                ["NAUFRAGEUR"]         = "Barre à mine|Grand sac|Gants en cuir",
                ["PATROUILLEUR_FLUVIAL"] = "Arme de poing (Épée)|Chemise de Cuir|Uniforme",
                ["PILOTE"]             = "Arme de Poing (Crochet)|Lanterne de Tempête et Huile",
                ["RIVERAINE"]          = "Seau|Canne à pêche et appât|Jambières en cuir",
            };

            foreach (var niveau in allNiveau1)
            {
                if (dotations.TryGetValue(niveau.Carriere.Code, out var d))
                    niveau.Dotations = string.IsNullOrEmpty(d) ? null : d;
            }

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(cancellationToken);
        }

        // Restrictions espèces-carrières (données du livre de base p.35-112)
        {
            var restrictedCareers = new Dictionary<string, string>
            {
                // Humain uniquement
                ["NONNE"] = "HUMAIN",
                ["PRETRE"] = "HUMAIN",
                ["FLAGELLANT"] = "HUMAIN",
                ["SORCIER_VILLAGE"] = "HUMAIN",
                ["CHASSEUR_SORCIERES"] = "HUMAIN",
                ["PRETRE_GUERRIER"] = "HUMAIN",
                ["SORCIERE"] = "HUMAIN",
                // Nain uniquement
                ["TUEUR"] = "NAIN",
                // Humain, Elfe des bois
                ["MYSTIQUE"] = "HUMAIN,ELFE_BOIS",
                // Haut Elfe, Humain, Elfe des bois
                ["MAGE"] = "HAUT_ELFE,HUMAIN,ELFE_BOIS",
                ["CAVALIER"] = "HAUT_ELFE,HUMAIN,ELFE_BOIS",
                ["CHEVALIER"] = "HAUT_ELFE,HUMAIN,ELFE_BOIS",
                // Halfling, Humain
                ["PATROUILLEUR"] = "HALFLING,HUMAIN",
                ["PATROUILLEUR_FLUVIAL"] = "HALFLING,HUMAIN",
                ["PILLEUR_TOMBES"] = "HALFLING,HUMAIN",
                // Halfling, Haut Elfe, Humain
                ["TRAFIQUANT"] = "HALFLING,HAUT_ELFE,HUMAIN",
                ["CHARLATAN"] = "HALFLING,HAUT_ELFE,HUMAIN",
                // Halfling, Haut Elfe, Humain, Elfe des bois (pas Nain)
                ["HERBORISTE"] = "HALFLING,HAUT_ELFE,HUMAIN,ELFE_BOIS",
                // Nain, Halfling, Humain (pas Haut Elfe, pas Elfe des bois)
                ["AGITATEUR"] = "NAIN,HALFLING,HUMAIN",
                ["MENDIANT"] = "NAIN,HALFLING,HUMAIN",
                ["RATIER"] = "NAIN,HALFLING,HUMAIN",
                ["SERVITEUR"] = "NAIN,HALFLING,HUMAIN",
                ["HUISSIER"] = "NAIN,HALFLING,HUMAIN",
                ["MINEUR"] = "NAIN,HALFLING,HUMAIN",
                ["VILLAGEOIS"] = "NAIN,HALFLING,HUMAIN",
                ["COCHER"] = "NAIN,HALFLING,HUMAIN",
                ["COLPORTEUR"] = "NAIN,HALFLING,HUMAIN",
                ["PILOTE"] = "NAIN,HALFLING,HUMAIN",
                ["RIVERAINE"] = "NAIN,HALFLING,HUMAIN",
                ["DEBARDEUR"] = "NAIN,HALFLING,HUMAIN",
                ["RECELEUR"] = "NAIN,HALFLING,HUMAIN",
                ["RACKETTEUR"] = "NAIN,HALFLING,HUMAIN",
                ["VOLEUR"] = "NAIN,HALFLING,HUMAIN",
                ["INGENIEUR"] = "NAIN,HALFLING,HUMAIN",
                // Nain, Halfling, Haut Elfe, Humain (pas Elfe des bois)
                ["APOTHICAIRE"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["AVOCAT"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["MEDECIN"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["ENQUETEUR"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["MARCHAND"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["CITADIN"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["SENTINELLE"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["INTENDANT"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["BATELIER"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["MATELOT"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                ["CONTREBANDIER"] = "NAIN,HALFLING,HAUT_ELFE,HUMAIN",
                // Nain, Haut Elfe, Humain
                ["DUELLISTE"] = "NAIN,HAUT_ELFE,HUMAIN",
                ["GROS_BRAS"] = "NAIN,HAUT_ELFE,HUMAIN",
                // Nain, Haut Elfe, Humain, Elfe des bois (pas Halfling)
                ["NOBLE"] = "NAIN,HAUT_ELFE,HUMAIN,ELFE_BOIS",
                // Nain, Humain, Elfe des bois
                ["NAUFRAGEUR"] = "NAIN,HUMAIN,ELFE_BOIS",
            };

            // Carrières ouvertes à toutes les espèces (EspecesAutorisees = null)
            var unrestrictedCodes = new[]
            {
                "ERUDIT", "ARTISAN", "CHASSEUR", "ECLAIREUR", "CONSEILLER",
                "ARTISTE", "EMISSAIRE", "ESPION", "CHASSEUR_PRIMES", "AMUSEUR",
                "MESSAGER", "SOLDAT", "HORS_LOI", "GARDE", "GLADIATEUR",
            };

            var allCodes = restrictedCareers.Keys.Concat(unrestrictedCodes).ToList();
            var careers = await db.Carrieres
                .Where(c => allCodes.Contains(c.Code))
                .ToListAsync(cancellationToken);

            foreach (var career in careers)
            {
                if (restrictedCareers.TryGetValue(career.Code, out var especes))
                    career.EspecesAutorisees = especes;
                else
                    career.EspecesAutorisees = null;
            }

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.ArmesReference.AnyAsync(cancellationToken))
        {
            db.ArmesReference.AddRange(
                // === ARMES DE MÊLÉE ===
                // Base
                new() { Code = "ARME_POING",       Nom = "Arme de Poing",    Groupe = "Base",      TypeArme = "Mêlée", Prix = "1 CO",   Encombrement = 1, Dommage = "+BF+4", Disponibilite = "Commun",     Longueur = "Moyen",      DeuxMains = false },
                new() { Code = "ARME_IMPROVISEE",   Nom = "Arme Improvisée",  Groupe = "Base",      TypeArme = "Mêlée", Prix = null,     Encombrement = 0, Dommage = "+BF+1", Disponibilite = "Variable",   Longueur = "Variable",   DeuxMains = false, Qualites = "Inoffensif" },
                new() { Code = "DAGUE",             Nom = "Dague",            Groupe = "Base",      TypeArme = "Mêlée", Prix = "16/–",   Encombrement = 0, Dommage = "+BF+2", Disponibilite = "Commun",     Longueur = "Très Court", DeuxMains = false },
                new() { Code = "COUTEAU",           Nom = "Couteau",          Groupe = "Base",      TypeArme = "Mêlée", Prix = "8/–",    Encombrement = 0, Dommage = "+BF+1", Disponibilite = "Commun",     Longueur = "Très Court", DeuxMains = false, Qualites = "Inoffensif" },
                new() { Code = "BOUCLIER_TARGE",    Nom = "Bouclier (Targe)", Groupe = "Base",      TypeArme = "Mêlée", Prix = "18/2",   Encombrement = 0, Dommage = "+BF+1", Disponibilite = "Commun",     Longueur = "Personnel",  DeuxMains = false, Qualites = "Bouclier 1, Défensif, Inoffensif" },
                new() { Code = "BOUCLIER",          Nom = "Bouclier",         Groupe = "Base",      TypeArme = "Mêlée", Prix = "2 CO",   Encombrement = 1, Dommage = "+BF+2", Disponibilite = "Commun",     Longueur = "Très Court", DeuxMains = false, Qualites = "Bouclier 2, Défensif, Inoffensif" },
                new() { Code = "BOUCLIER_LARGE",    Nom = "Bouclier (Large)", Groupe = "Base",      TypeArme = "Mêlée", Prix = "3 CO",   Encombrement = 3, Dommage = "+BF+3", Disponibilite = "Commun",     Longueur = "Très Court", DeuxMains = false, Qualites = "Bouclier 3, Défensif, Inoffensif" },
                // Cavalerie
                new() { Code = "MARTEAU_CAVALERIE", Nom = "Marteau de Cavalerie", Groupe = "Cavalerie", TypeArme = "Mêlée", Prix = "3 CO", Encombrement = 3, Dommage = "+BF+5", Disponibilite = "Peu Commun", Longueur = "Long",   DeuxMains = true, Qualites = "Contondant" },
                new() { Code = "LANCE_CAV",         Nom = "Lance",            Groupe = "Cavalerie", TypeArme = "Mêlée", Prix = "1 CO",   Encombrement = 3, Dommage = "+BF+6", Disponibilite = "Rare",       Longueur = "Très Long",  DeuxMains = true, Qualites = "Impact, Empalant" },
                // Escrime
                new() { Code = "FLEURET",           Nom = "Fleuret",          Groupe = "Escrime",   TypeArme = "Mêlée", Prix = "5 CO",   Encombrement = 1, Dommage = "+BF+3", Disponibilite = "Peu Commun", Longueur = "Moyen",      DeuxMains = false, Qualites = "Rapide, Empalant, Précis, Inoffensif" },
                new() { Code = "RAPIERE",           Nom = "Rapière",          Groupe = "Escrime",   TypeArme = "Mêlée", Prix = "5 CO",   Encombrement = 1, Dommage = "+BF+4", Disponibilite = "Peu Commun", Longueur = "Long",       DeuxMains = false, Qualites = "Rapide, Empalant" },
                // Bagarre
                new() { Code = "MAIN_NUE",          Nom = "Main Nue",         Groupe = "Bagarre",   TypeArme = "Mêlée", Prix = null,     Encombrement = 0, Dommage = "+BF+0", Disponibilite = "–",          Longueur = "Personnel",  DeuxMains = false, Qualites = "Inoffensif" },
                new() { Code = "POINGS_ACIER",      Nom = "Poings d'Acier",   Groupe = "Bagarre",   TypeArme = "Mêlée", Prix = "2/6",    Encombrement = 0, Dommage = "+BF+2", Disponibilite = "Commun",     Longueur = "Personnel",  DeuxMains = false },
                // Fléau
                new() { Code = "FLEAU_GRAIN",       Nom = "Fléau à Grain",    Groupe = "Fléau",     TypeArme = "Mêlée", Prix = "10/–",   Encombrement = 1, Dommage = "+BF+3", Disponibilite = "Commun",     Longueur = "Moyen",      DeuxMains = false, Qualites = "Distrayant", Defauts = "Imprécis, Enchevêtré" },
                new() { Code = "FLEAU",             Nom = "Fléau",            Groupe = "Fléau",     TypeArme = "Mêlée", Prix = "2 CO",   Encombrement = 1, Dommage = "+BF+5", Disponibilite = "Peu Commun", Longueur = "Moyen",      DeuxMains = false, Qualites = "Distrayant", Defauts = "Enchevêtré" },
                new() { Code = "FLEAU_MILITAIRE",   Nom = "Fléau Militaire",  Groupe = "Fléau",     TypeArme = "Mêlée", Prix = "3 CO",   Encombrement = 2, Dommage = "+BF+6", Disponibilite = "Rare",       Longueur = "Long",       DeuxMains = true, Qualites = "Distrayant, Impact", Defauts = "Épuisant, Enchevêtré" },
                // Parade
                new() { Code = "MAIN_GAUCHE",       Nom = "Main Gauche",      Groupe = "Parade",    TypeArme = "Mêlée", Prix = "1 CO",   Encombrement = 0, Dommage = "+BF+2", Disponibilite = "Rare",       Longueur = "Très Court", DeuxMains = false, Qualites = "Défensif" },
                new() { Code = "BRISE_EPEE",        Nom = "Brise-épée",       Groupe = "Parade",    TypeArme = "Mêlée", Prix = "1 CO",   Encombrement = 1, Dommage = "+BF+3", Disponibilite = "Peu Commun", Longueur = "Court",      DeuxMains = false, Qualites = "Défensif, Piège-lame" },
                // Arme d'Hast
                new() { Code = "HALLEBARDE",        Nom = "Hallebarde",       Groupe = "Arme d'Hast", TypeArme = "Mêlée", Prix = "2 CO", Encombrement = 3, Dommage = "+BF+4", Disponibilite = "Commun",     Longueur = "Long",       DeuxMains = true, Qualites = "Défensif, Entailler, Empalant" },
                new() { Code = "LANCE_HAST",        Nom = "Lance",            Groupe = "Arme d'Hast", TypeArme = "Mêlée", Prix = "15/–", Encombrement = 2, Dommage = "+BF+4", Disponibilite = "Commun",     Longueur = "Très Long",  DeuxMains = true, Qualites = "Empalant" },
                new() { Code = "PIQUE",             Nom = "Pique",            Groupe = "Arme d'Hast", TypeArme = "Mêlée", Prix = "18/–", Encombrement = 4, Dommage = "+BF+4", Disponibilite = "Rare",       Longueur = "Massive",    DeuxMains = true, Qualites = "Empalant" },
                new() { Code = "BATON",             Nom = "Bâton",            Groupe = "Arme d'Hast", TypeArme = "Mêlée", Prix = "3/–",  Encombrement = 2, Dommage = "+BF+4", Disponibilite = "Commun",     Longueur = "Long",       DeuxMains = true, Qualites = "Défensif, Contondant" },
                // Deux-Mains
                new() { Code = "EPEE_BATARDE",      Nom = "Épée Bâtarde",     Groupe = "Deux-Mains", TypeArme = "Mêlée", Prix = "8 CO", Encombrement = 3, Dommage = "+BF+5", Disponibilite = "Peu Commun", Longueur = "Long",       DeuxMains = true, Qualites = "Endommageant, Défensif" },
                new() { Code = "GRANDE_HACHE",      Nom = "Grande Hache",     Groupe = "Deux-Mains", TypeArme = "Mêlée", Prix = "4 CO", Encombrement = 3, Dommage = "+BF+6", Disponibilite = "Peu Commun", Longueur = "Long",       DeuxMains = true, Qualites = "Entailler, Impact", Defauts = "Fatiguant" },
                new() { Code = "PIOCHE",            Nom = "Pioche",           Groupe = "Deux-Mains", TypeArme = "Mêlée", Prix = "9/–",  Encombrement = 3, Dommage = "+BF+5", Disponibilite = "Commun",     Longueur = "Moyen",      DeuxMains = true, Qualites = "Endommageant, Empalant", Defauts = "Lent" },
                new() { Code = "MARTEAU_GUERRE",    Nom = "Marteau de Guerre", Groupe = "Deux-Mains", TypeArme = "Mêlée", Prix = "3 CO", Encombrement = 3, Dommage = "+BF+6", Disponibilite = "Commun",    Longueur = "Moyen",      DeuxMains = true, Qualites = "Endommageant, Contondant", Defauts = "Lent" },
                new() { Code = "ZWEIHANDER",        Nom = "Zweihänder",       Groupe = "Deux-Mains", TypeArme = "Mêlée", Prix = "10 CO", Encombrement = 3, Dommage = "+BF+5", Disponibilite = "Peu Commun", Longueur = "Long",      DeuxMains = true, Qualites = "Endommageant, Entailler" },

                // === ARMES À DISTANCE ===
                // Poudre Noir
                new() { Code = "TROMBLON",          Nom = "Tromblon",         Groupe = "Poudre Noir", TypeArme = "Distance", Prix = "2 CO", Encombrement = 1, Dommage = "+8", Disponibilite = "Peu Commun", Portee = "20", DeuxMains = true, Qualites = "Explosion 3", Defauts = "Dangereux, Rechargement 2" },
                new() { Code = "FUSIL_HOCHLAND",    Nom = "Fusil du Hochland", Groupe = "Poudre Noir", TypeArme = "Distance", Prix = "100 CO", Encombrement = 3, Dommage = "+9", Disponibilite = "Exotique", Portee = "100", DeuxMains = true, Qualites = "Minutieux, Précis", Defauts = "Rechargement 4" },
                new() { Code = "FUSIL",             Nom = "Fusil",            Groupe = "Poudre Noir", TypeArme = "Distance", Prix = "4 CO", Encombrement = 2, Dommage = "+9", Disponibilite = "Peu Commun", Portee = "50", DeuxMains = true, Defauts = "Dangereux, Rechargement 3" },
                new() { Code = "PISTOLET",          Nom = "Pistolet",         Groupe = "Poudre Noir", TypeArme = "Distance", Prix = "8 CO", Encombrement = 0, Dommage = "+8", Disponibilite = "Rare",       Portee = "20", DeuxMains = false, Qualites = "Pistolet", Defauts = "Rechargement 1" },
                // Arc
                new() { Code = "ARC_ELFIQUE",       Nom = "Arc Elfique",      Groupe = "Arc",       TypeArme = "Distance", Prix = "10 CO", Encombrement = 2, Dommage = "+BF+4", Disponibilite = "Exotique", Portee = "150", DeuxMains = true, Qualites = "Endommageant, Précis" },
                new() { Code = "ARC_LONG",          Nom = "Arc Long",         Groupe = "Arc",       TypeArme = "Distance", Prix = "5 CO", Encombrement = 3, Dommage = "+BF+4", Disponibilite = "Peu Commun", Portee = "100", DeuxMains = true, Qualites = "Endommageant" },
                new() { Code = "ARC",               Nom = "Arc",              Groupe = "Arc",       TypeArme = "Distance", Prix = "4 CO", Encombrement = 2, Dommage = "+BF+3", Disponibilite = "Commun",    Portee = "50",  DeuxMains = true },
                new() { Code = "ARC_COURT",         Nom = "Arc Court",        Groupe = "Arc",       TypeArme = "Distance", Prix = "3 CO", Encombrement = 1, Dommage = "+BF+2", Disponibilite = "Commun",    Portee = "20",  DeuxMains = true },
                // Arbalète
                new() { Code = "ARBALETE_POING",    Nom = "Arbalète de Poing", Groupe = "Arbalète",  TypeArme = "Distance", Prix = "6 CO", Encombrement = 0, Dommage = "+7", Disponibilite = "Peu Commun", Portee = "10",  DeuxMains = false, Qualites = "Pistolet" },
                new() { Code = "ARBALETE_LOURDE",   Nom = "Arbalète Lourde",  Groupe = "Arbalète",  TypeArme = "Distance", Prix = "7 CO", Encombrement = 3, Dommage = "+9", Disponibilite = "Rare",       Portee = "100", DeuxMains = true, Qualites = "Endommageant", Defauts = "Rechargement 2" },
                new() { Code = "ARBALETE",          Nom = "Arbalète",         Groupe = "Arbalète",  TypeArme = "Distance", Prix = "5 CO", Encombrement = 2, Dommage = "+9", Disponibilite = "Commun",      Portee = "60",  DeuxMains = true, Defauts = "Rechargement 1" },
                // Ingénierie
                new() { Code = "FUSIL_REPETITION",  Nom = "Fusil à Répétition", Groupe = "Ingénierie", TypeArme = "Distance", Prix = "10 CO", Encombrement = 3, Dommage = "+9", Disponibilite = "Rare", Portee = "30", DeuxMains = true, Defauts = "Dangereux, Rechargement 5, Répétition 4" },
                new() { Code = "PISTOLET_REPETITION", Nom = "Pistolet à Répétition", Groupe = "Ingénierie", TypeArme = "Distance", Prix = "15 CO", Encombrement = 1, Dommage = "+8", Disponibilite = "Rare", Portee = "10", DeuxMains = false, Qualites = "Pistolet", Defauts = "Dangereux, Rechargement 4" },
                // Emmêlant
                new() { Code = "LASSO",             Nom = "Lasso",            Groupe = "Emmêlant",  TypeArme = "Distance", Prix = "6/–", Encombrement = 0, Dommage = "–",    Disponibilite = "Commun",     Portee = "BFx2", DeuxMains = false, Qualites = "Enchevêtré" },
                new() { Code = "FOUET",             Nom = "Fouet",            Groupe = "Emmêlant",  TypeArme = "Distance", Prix = "5/–", Encombrement = 0, Dommage = "+BF+2", Disponibilite = "Commun",    Portee = "6",    DeuxMains = false, Qualites = "Enchevêtré" },
                // Explosifs
                new() { Code = "BOMBE",             Nom = "Bombe",            Groupe = "Explosifs", TypeArme = "Distance", Prix = "3 CO", Encombrement = 0, Dommage = "+12", Disponibilite = "Rare",       Portee = "BF",   DeuxMains = false, Qualites = "Explosion 5, Impact", Defauts = "Dangereux" },
                new() { Code = "INCENDIAIRE",       Nom = "Incendiaire",      Groupe = "Explosifs", TypeArme = "Distance", Prix = "1 CO", Encombrement = 0, Dommage = "Spécial", Disponibilite = "Peu Commun", Portee = "BF", DeuxMains = false, Qualites = "Explosion 4", Defauts = "Dangereux" },
                // Fronde
                new() { Code = "FRONDE",            Nom = "Fronde",           Groupe = "Fronde",    TypeArme = "Distance", Prix = "1/–", Encombrement = 0, Dommage = "+6",    Disponibilite = "Commun",     Portee = "60",  DeuxMains = false },
                new() { Code = "BATON_FRONDE",      Nom = "Bâton Fronde",     Groupe = "Fronde",    TypeArme = "Distance", Prix = "4/–", Encombrement = 2, Dommage = "+7",    Disponibilite = "Peu Commun", Portee = "100", DeuxMains = true },
                // Lancer
                new() { Code = "BOLAS",             Nom = "Bolas",            Groupe = "Lancer",    TypeArme = "Distance", Prix = "10/–", Encombrement = 0, Dommage = "+BF",   Disponibilite = "Rare",      Portee = "BFx3", DeuxMains = false, Qualites = "Enchevêtré" },
                new() { Code = "FLECHETTE",         Nom = "Fléchette",        Groupe = "Lancer",    TypeArme = "Distance", Prix = "2/–", Encombrement = 0, Dommage = "+BF+1", Disponibilite = "Peu Commun", Portee = "BFx2", DeuxMains = false, Qualites = "Empalant" },
                new() { Code = "JAVELOT",           Nom = "Javelot",          Groupe = "Lancer",    TypeArme = "Distance", Prix = "10/6", Encombrement = 1, Dommage = "+BF+3", Disponibilite = "Peu Commun", Portee = "BFx3", DeuxMains = false, Qualites = "Empalant" },
                new() { Code = "CAILLOU",           Nom = "Caillou",          Groupe = "Lancer",    TypeArme = "Distance", Prix = null,  Encombrement = 0, Dommage = "+BF",   Disponibilite = "Commun",     Portee = "BFx3", DeuxMains = false },
                new() { Code = "HACHE_LANCER",      Nom = "Hache de Lancer",  Groupe = "Lancer",    TypeArme = "Distance", Prix = "1 CO", Encombrement = 1, Dommage = "+BF+3", Disponibilite = "Commun",    Portee = "BFx2", DeuxMains = false, Qualites = "Entailler" },
                new() { Code = "COUTEAU_LANCER",    Nom = "Couteau de Lancer", Groupe = "Lancer",   TypeArme = "Distance", Prix = "18/–", Encombrement = 0, Dommage = "+BF+2", Disponibilite = "Commun",    Portee = "BFx2", DeuxMains = false }
            );
            await db.SaveChangesAsync(cancellationToken);
        }

        await SeedTraitsPhysiquesAsync(db, cancellationToken);

        if (!await db.Personnages.AnyAsync(p => p.KeycloakId == DemoJoueurId, cancellationToken))
        {
            var espece = await db.Especes.FirstAsync(e => e.Code == "HUMAIN", cancellationToken);
            var niveauCarriere = await db.NiveauCarrieres
                .Include(n => n.Carriere)
                .FirstAsync(n => n.Carriere.Code == "APOTHICAIRE" && n.Niveau == 1, cancellationToken);
            var perception = await db.Competences.FirstAsync(c => c.Code == "PERCEPTION", cancellationToken);
            var charme = await db.Competences.FirstAsync(c => c.Code == "CHARME", cancellationToken);
            var lecture = await db.Talents.FirstAsync(t => t.Code == "LECTURE_ECRITURE", cancellationToken);
            var now = DateTime.UtcNow;
            var caracs = JsonSerializer.Deserialize<Dictionary<string, int>>(espece.CaracInitiales ?? "{}") ?? new Dictionary<string, int>();

            var personnage = new Personnage
            {
                KeycloakId = DemoJoueurId,
                Nom = "Adelbert le Prudhomme",
                EspeceId = espece.Id,
                CarriereCouranteId = niveauCarriere.Id,
                Motivation = "Survivre assez longtemps pour ouvrir une boutique à Altdorf.",
                StatutSocial = "Bronze 3",
                Age = 27,
                CouleurYeux = "Gris",
                CouleurCheveux = "Bruns",
                TailleCm = 178,
                Mouvement = espece.MouvementBase,
                XpTotal = 175,
                XpDepense = 35,
                CreatedAt = now,
                UpdatedAt = now,
            };

            foreach (var (code, valeur) in caracs)
            {
                personnage.Caracteristiques.Add(new PersonnageCaracteristique
                {
                    Code = code,
                    ValeurInitiale = valeur,
                    Avances = code == "Int" ? 1 : 0,
                });
            }

            personnage.Competences.Add(new PersonnageCompetence
            {
                CompetenceId = perception.Id,
                Avances = 2,
            });

            personnage.Competences.Add(new PersonnageCompetence
            {
                CompetenceId = charme.Id,
                Avances = 1,
            });

            personnage.Talents.Add(new PersonnageTalent
            {
                TalentId = lecture.Id,
                Fois = 1,
            });

            personnage.Carrieres.Add(new PersonnageCarriere
            {
                NiveauCarriereId = niveauCarriere.Id,
                EstCourante = true,
                DateEntree = now.AddDays(-18),
            });

            personnage.Partages.Add(new PersonnagePartage
            {
                MjKeycloakId = DemoMjId,
                Permission = PermissionPartage.XP,
                CreatedAt = now.AddDays(-7),
            });

            personnage.HistoriqueXP.Add(new HistoriqueXP
            {
                AuteurKeycloakId = DemoMjId,
                Montant = 100,
                Type = TypeXP.Gain,
                Notes = "Fin du scénario d'introduction.",
                CreatedAt = now.AddDays(-7),
            });

            personnage.HistoriqueXP.Add(new HistoriqueXP
            {
                AuteurKeycloakId = DemoJoueurId,
                Montant = -25,
                Type = TypeXP.Caracteristique,
                Cible = "Int",
                Notes = "Études nocturnes.",
                CreatedAt = now.AddDays(-5),
            });

            personnage.HistoriqueXP.Add(new HistoriqueXP
            {
                AuteurKeycloakId = DemoJoueurId,
                Montant = -10,
                Type = TypeXP.Competence,
                Cible = perception.Id.ToString(),
                Notes = "Habitude des ruelles d'Altdorf.",
                CreatedAt = now.AddDays(-3),
            });

            ApplyDerivedAttributes(personnage);
            db.Personnages.Add(personnage);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static void ApplyDerivedAttributes(Personnage personnage)
    {
        var caracs = personnage.Caracteristiques.ToDictionary(c => c.Code);

        int GetCarac(string code) =>
            caracs.TryGetValue(code, out var caracteristique) ? caracteristique.ValeurInitiale + caracteristique.Avances : 0;

        var force = GetCarac("F");
        var endurance = GetCarac("E");
        var volonte = GetCarac("FM");

        personnage.BlessuresMax = (force / 10) + 2 * (endurance / 10) + (volonte / 10);
        personnage.Destin = 2;
        personnage.Fortune = personnage.Destin;
        personnage.Resilience = 1;
        personnage.Resolution = personnage.Resilience;
    }

    private static Espece CreateEspece(
        string code,
        string nom,
        int mouvementBase,
        string description,
        params (string Code, int Valeur)[] caracteristiques)
    {
        return new Espece
        {
            Code = code,
            Nom = nom,
            MouvementBase = mouvementBase,
            Description = description,
            CaracInitiales = JsonSerializer.Serialize(caracteristiques.ToDictionary(c => c.Code, c => c.Valeur)),
        };
    }

    private static async Task SeedTraitsPhysiquesAsync(Wfrp4DbContext db, CancellationToken ct)
    {
        var especes = await db.Especes.ToListAsync(ct);
        var anyChanged = false;

        foreach (var espece in especes)
        {
            if (!string.IsNullOrWhiteSpace(espece.TraitsPhysiques)) continue;

            var traits = GetTraitsPhysiques(espece.Code);
            if (traits == null) continue;

            espece.TraitsPhysiques = JsonSerializer.Serialize(traits, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            anyChanged = true;
        }

        if (anyChanged)
            await db.SaveChangesAsync(ct);
    }

    private static object? GetTraitsPhysiques(string code) => code switch
    {
        "HUMAIN" => new
        {
            Age = new { Base = 15, NbDes = 1, Multiplicateur = 1 },
            Taille = new { BaseCm = 144, NbDes = 2, FacteurCm = 2.5, DesSupSur10 = true },
            DoubleTirageYeux = false,
            CouleursYeux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Ivoire" },
                new { Min = 3, Max = 3, Valeur = "Charbon" },
                new { Min = 4, Max = 4, Valeur = "Aigue-marine" },
                new { Min = 5, Max = 7, Valeur = "Bleu" },
                new { Min = 8, Max = 11, Valeur = "Châtaigne" },
                new { Min = 12, Max = 14, Valeur = "Brun foncé" },
                new { Min = 15, Max = 17, Valeur = "Marron" },
                new { Min = 18, Max = 18, Valeur = "Cuivre" },
                new { Min = 19, Max = 19, Valeur = "Brun sable" },
                new { Min = 20, Max = 20, Valeur = "Violet" },
            },
            CouleursCheveux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Blanc" },
                new { Min = 3, Max = 3, Valeur = "Blond cendré" },
                new { Min = 4, Max = 4, Valeur = "Roux" },
                new { Min = 5, Max = 7, Valeur = "Blond" },
                new { Min = 8, Max = 11, Valeur = "Brun" },
                new { Min = 12, Max = 14, Valeur = "Brun foncé" },
                new { Min = 15, Max = 17, Valeur = "Noir" },
                new { Min = 18, Max = 18, Valeur = "Auburn" },
                new { Min = 19, Max = 19, Valeur = "Roux" },
                new { Min = 20, Max = 20, Valeur = "Noir" },
            },
            Noms = new
            {
                Structure = 0, // PrenomNom
                Prenoms = new[] { "Adhemar", "Anders", "Artur", "Beatrijs", "Clementia", "Detlev", "Erika", "Frauke", "Frederich", "Gerner", "Gertraud", "Haletha", "Heinrich", "Helga", "Henryk", "Irmina", "Jehanne", "Karl", "Kruger", "Lorelay", "Marieke", "Sebastien", "Sigfreda", "Talther", "Talunda", "Ulrich", "Ulrika", "Werther", "Willelma", "Wilryn" },
                NomsFamille = new[] { "Bauer", "Fleischer", "Schmidt", "Schuster", "Augenlos", "Dunn", "Lang", "Laut", "Stark", "Braun", "Weiss", "Schwarz", "Engel", "Richter", "Koch" },
            },
        },
        "NAIN" => new
        {
            Age = new { Base = 15, NbDes = 10, Multiplicateur = 1 },
            Taille = new { BaseCm = 129, NbDes = 1, FacteurCm = 2.5, DesSupSur10 = false },
            DoubleTirageYeux = false,
            CouleursYeux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Libre choix" },
                new { Min = 3, Max = 3, Valeur = "Vert" },
                new { Min = 4, Max = 4, Valeur = "Vert lierre" },
                new { Min = 5, Max = 7, Valeur = "Bleu" },
                new { Min = 8, Max = 11, Valeur = "Gris pâle" },
                new { Min = 12, Max = 14, Valeur = "Châtaigne" },
                new { Min = 15, Max = 17, Valeur = "Marron" },
                new { Min = 18, Max = 18, Valeur = "Beige" },
                new { Min = 19, Max = 19, Valeur = "Brun foncé" },
                new { Min = 20, Max = 20, Valeur = "Noir" },
            },
            CouleursCheveux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Argenté" },
                new { Min = 3, Max = 3, Valeur = "Blond doré" },
                new { Min = 4, Max = 4, Valeur = "Brun roux" },
                new { Min = 5, Max = 7, Valeur = "Brun doré" },
                new { Min = 8, Max = 11, Valeur = "Marron clair" },
                new { Min = 12, Max = 14, Valeur = "Gingembre" },
                new { Min = 15, Max = 17, Valeur = "Blond rougeâtre" },
                new { Min = 18, Max = 18, Valeur = "Auburn" },
                new { Min = 19, Max = 19, Valeur = "Brun rougeâtre" },
                new { Min = 20, Max = 20, Valeur = "Gris" },
            },
            Noms = new
            {
                Structure = 1, // PrenomPatronymeClan
                Prenoms = new[] { "Alrik", "Bronda", "Dimzad", "Fenna", "Gottri", "Gudrun", "Snorri", "Baragaz", "Durak", "Galazil", "Gnoldok", "Nazril", "Okri" },
                SuffixesPatronymiques = new[] { "sson", "sdottir", "snev", "sniz" },
                Clans = new[] { "Ardrungan", "Bryntok", "Gazani", "Gromheld", "Harrazlings", "Unboki", "Dokkintroll", "Ganvalgger", "Kvitang", "Thrungtak", "Wyrgrinti", "Zankonk" },
            },
        },
        "HALFLING" => new
        {
            Age = new { Base = 15, NbDes = 5, Multiplicateur = 1 },
            Taille = new { BaseCm = 95, NbDes = 1, FacteurCm = 2.5, DesSupSur10 = false },
            DoubleTirageYeux = false,
            CouleursYeux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Jais" },
                new { Min = 3, Max = 3, Valeur = "Améthyste" },
                new { Min = 4, Max = 4, Valeur = "Bleu pâle" },
                new { Min = 5, Max = 7, Valeur = "Turquoise" },
                new { Min = 8, Max = 11, Valeur = "Vert" },
                new { Min = 12, Max = 14, Valeur = "Noisette" },
                new { Min = 15, Max = 17, Valeur = "Émeraude" },
                new { Min = 18, Max = 18, Valeur = "Cuivre" },
                new { Min = 19, Max = 19, Valeur = "Citrine" },
                new { Min = 20, Max = 20, Valeur = "Doré" },
            },
            CouleursCheveux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Blond blanc" },
                new { Min = 3, Max = 3, Valeur = "Gris lin" },
                new { Min = 4, Max = 4, Valeur = "Blond clair" },
                new { Min = 5, Max = 7, Valeur = "Doré" },
                new { Min = 8, Max = 11, Valeur = "Cuivré" },
                new { Min = 12, Max = 14, Valeur = "Brun foncé" },
                new { Min = 15, Max = 17, Valeur = "Moutarde" },
                new { Min = 18, Max = 18, Valeur = "Brun foncé" },
                new { Min = 19, Max = 19, Valeur = "Chocolat" },
                new { Min = 20, Max = 20, Valeur = "Noir" },
            },
            Noms = new
            {
                Structure = 2, // PrenomDiminutifClan
                Prenoms = new[] { "Antoniella", "Esmerelda", "Ferdinand", "Heironymus", "Maximilian", "Theodosius", "Thomasina", "Anni", "Esme", "Fred", "Hiro", "Max", "Theo", "Tina" },
                Clans = new[] { "Ashfield", "Brandysnap", "Hayfoot", "Rumster", "Shortbottom", "Thorncobble" },
            },
        },
        "HAUT_ELFE" => new
        {
            Age = new { Base = 30, NbDes = 10, Multiplicateur = 1 },
            Taille = new { BaseCm = 180, NbDes = 1, FacteurCm = 2.5, DesSupSur10 = false },
            DoubleTirageYeux = true,
            CouleursYeux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Gypse" },
                new { Min = 3, Max = 3, Valeur = "Plomb" },
                new { Min = 4, Max = 4, Valeur = "Bleu pâle" },
                new { Min = 5, Max = 7, Valeur = "Saphir" },
                new { Min = 8, Max = 11, Valeur = "Brun" },
                new { Min = 12, Max = 14, Valeur = "Gris" },
                new { Min = 15, Max = 17, Valeur = "Noisette" },
                new { Min = 18, Max = 18, Valeur = "Noisette" },
                new { Min = 19, Max = 19, Valeur = "Cuivre" },
                new { Min = 20, Max = 20, Valeur = "Doré" },
            },
            CouleursCheveux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Blanc" },
                new { Min = 3, Max = 3, Valeur = "Gris" },
                new { Min = 4, Max = 4, Valeur = "Blond clair" },
                new { Min = 5, Max = 7, Valeur = "Miel" },
                new { Min = 8, Max = 11, Valeur = "Châtaigne" },
                new { Min = 12, Max = 14, Valeur = "Bronze" },
                new { Min = 15, Max = 17, Valeur = "Brun" },
                new { Min = 18, Max = 18, Valeur = "Amande" },
                new { Min = 19, Max = 19, Valeur = "Roux" },
                new { Min = 20, Max = 20, Valeur = "Réglisse" },
            },
            Noms = new
            {
                Structure = 3, // GenerateurEpithete
                Elements1 = new[] { "Aes", "Ath", "Dor", "Far", "Gal", "Im", "Lin", "Mal", "Mor", "Ullia" },
                Elements2 = new[] { "arha", "anhu", "dda", "han", "loc", "noc", "oth", "ryn", "stra", "wyth" },
                Terminaisons = new[] { "andril", "anel", "ellion", "fin", "il", "irian", "mor", "nil", "ric", "wing" },
                Epithetes = new[] { "Emberfell", "Fireborn", "Foamheart", "Goldenhair", "Silverspray", "Spellsign" },
                Prenoms = Array.Empty<string>(),
            },
        },
        "ELFE_BOIS" => new
        {
            Age = new { Base = 30, NbDes = 10, Multiplicateur = 1 },
            Taille = new { BaseCm = 180, NbDes = 1, FacteurCm = 2.5, DesSupSur10 = false },
            DoubleTirageYeux = true,
            CouleursYeux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Gris pâle" },
                new { Min = 3, Max = 3, Valeur = "Gris" },
                new { Min = 4, Max = 4, Valeur = "Acier" },
                new { Min = 5, Max = 7, Valeur = "Vert Mousse" },
                new { Min = 8, Max = 11, Valeur = "Terre" },
                new { Min = 12, Max = 14, Valeur = "Brun foncé" },
                new { Min = 15, Max = 17, Valeur = "Ambre" },
                new { Min = 18, Max = 18, Valeur = "Vert" },
                new { Min = 19, Max = 19, Valeur = "Brun foncé" },
                new { Min = 20, Max = 20, Valeur = "Brun foncé" },
            },
            CouleursCheveux = new object[]
            {
                new { Min = 2, Max = 2, Valeur = "Bouleau argenté" },
                new { Min = 3, Max = 3, Valeur = "Blond cendré" },
                new { Min = 4, Max = 4, Valeur = "Rose doré" },
                new { Min = 5, Max = 7, Valeur = "Blond miel" },
                new { Min = 8, Max = 11, Valeur = "Blond cuivré" },
                new { Min = 12, Max = 14, Valeur = "Acajou" },
                new { Min = 15, Max = 17, Valeur = "Brun foncé" },
                new { Min = 18, Max = 18, Valeur = "Terre de sienne" },
                new { Min = 19, Max = 19, Valeur = "Ébène" },
                new { Min = 20, Max = 20, Valeur = "Bleu noirâtre" },
            },
            Noms = new
            {
                Structure = 3, // GenerateurEpithete
                Elements1 = new[] { "Aes", "Ath", "Dor", "Far", "Gal", "Im", "Lin", "Mal", "Mor", "Ullia" },
                Elements2 = new[] { "arha", "anhu", "dda", "han", "loc", "noc", "oth", "ryn", "stra", "wyth" },
                Terminaisons = new[] { "a", "ath", "dia", "en", "for", "lor", "mar", "ol", "sor", "than" },
                Epithetes = new[] { "Fleetriver", "Shadowstalker", "Treeshaper", "Weavewatcher", "Willowlimb", "Windrunner" },
                Prenoms = Array.Empty<string>(),
            },
        },
        _ => null,
    };
}