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

        {
            var existingSorts = await db.SortsReference.ToDictionaryAsync(s => s.Code, cancellationToken);

            foreach (var sort in GetSortsReference())
            {
                if (existingSorts.TryGetValue(sort.Code, out var existing))
                {
                    existing.Nom = sort.Nom;
                    existing.Categorie = sort.Categorie;
                    existing.Domaine = sort.Domaine;
                    existing.Cn = sort.Cn;
                    existing.Portee = sort.Portee;
                    existing.Cible = sort.Cible;
                    existing.Duree = sort.Duree;
                    existing.Resume = sort.Resume;
                }
                else
                {
                    db.SortsReference.Add(sort);
                }
            }

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(cancellationToken);
        }

        {
            var existingBases = await db.TitresBaseReference.ToDictionaryAsync(t => t.Code, cancellationToken);

            foreach (var titre in GetTitresBaseReference())
            {
                if (existingBases.TryGetValue(titre.Code, out var existing))
                {
                    existing.Libelle = titre.Libelle;
                    existing.Ordre = titre.Ordre;
                    existing.NiveauMaitrise = titre.NiveauMaitrise;
                }
                else
                {
                    db.TitresBaseReference.Add(titre);
                }
            }

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(cancellationToken);
        }

        {
            var existingQualificatifs = await db.TitresQualificatifReference.ToDictionaryAsync(t => t.Code, cancellationToken);

            foreach (var titre in GetTitresQualificatifReference())
            {
                if (existingQualificatifs.TryGetValue(titre.Code, out var existing))
                {
                    existing.Libelle = titre.Libelle;
                    existing.Ordre = titre.Ordre;
                    existing.NiveauMaitrise = titre.NiveauMaitrise;
                }
                else
                {
                    db.TitresQualificatifReference.Add(titre);
                }
            }

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(cancellationToken);
        }

        {
            var existingCreatures = await db.CreaturesReference.ToDictionaryAsync(c => c.Code, cancellationToken);

            foreach (var creature in GetCreaturesReference())
            {
                if (existingCreatures.TryGetValue(creature.Code, out var existing))
                {
                    existing.Nom = creature.Nom;
                    existing.Categorie = creature.Categorie;
                    existing.M = creature.M;
                    existing.CC = creature.CC;
                    existing.CT = creature.CT;
                    existing.F = creature.F;
                    existing.E = creature.E;
                    existing.I = creature.I;
                    existing.Ag = creature.Ag;
                    existing.Dex = creature.Dex;
                    existing.Int = creature.Int;
                    existing.FM = creature.FM;
                    existing.Soc = creature.Soc;
                    existing.B = creature.B;
                    existing.Traits = creature.Traits;
                    existing.TraitsOptionnels = creature.TraitsOptionnels;
                    existing.Page = creature.Page;
                }
                else
                {
                    db.CreaturesReference.Add(creature);
                }
            }

            if (db.ChangeTracker.HasChanges())
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

    private static IEnumerable<SortReference> GetSortsReference()
    {
        var sorts = new List<SortReference>();

        void Add(string categorie, string? domaine, string nom, int? cn = null, string? portee = null, string? cible = null, string? duree = null, string? resume = null)
        {
            sorts.Add(new SortReference
            {
                Code = $"{categorie}:{domaine ?? "GENERAL"}:{nom}",
                Categorie = categorie,
                Domaine = domaine,
                Nom = nom,
                Cn = cn,
                Portee = portee,
                Cible = cible,
                Duree = duree,
                Resume = resume ?? $"Sort de {categorie.ToLowerInvariant()}{(string.IsNullOrWhiteSpace(domaine) ? string.Empty : $" ({domaine})")}. Consultez les champs de lancement et la description complète dans le livre de règles si la table joue avec les effets détaillés.",
            });
        }

        Add("Mineur", null, "Repères", 0, "Vous", "Vous", "Instantanée", "Le lanceur ressent la provenance des vents de magie et sait immédiatement où se trouve le nord.");
        Add("Mineur", null, "Éblouir", 0, "Toucher", "1", "Bonus FM rounds", "La cible reçoit une condition Aveuglé, puis en reçoit une autre au début de chaque round pendant la durée du sort.");
        Add("Mineur", null, "Pas Prudent", 0, "Vous", "Vous", "FM minutes", "Le passage du lanceur ne marque presque pas les matières organiques ; les tests de Pistage en milieu rural subissent un fort malus.");
        Add("Mineur", null, "Ami des Animaux", 0, "1 mètre", "1", "1 heure", "Une petite créature bestiale fait confiance au lanceur et le considère comme un ami.");
        Add("Mineur", null, "Conservez", 0, "1 mètre", "1", "Bonus FM jours", "Préserve jusqu'à une journée de rations contre la décomposition naturelle pendant la durée du sort.");
        Add("Mineur", null, "Fléchette", 0, "FM mètres", "1", "Instantanée", "Projette une petite fléchette d'énergie, traitée comme un missile magique de dégâts +0.");
        Add("Mineur", null, "Drainage", 0, "Toucher", "1", "Instantanée", "Draine la vitalité d'une cible comme un missile magique de dégâts +0 ignorant la protection, puis soigne 1 blessure au lanceur.");
        Add("Mineur", null, "Écoute Indiscrète", 0, "FM mètres", "1", "Bonus I minutes", "Permet d'entendre les paroles de la cible comme si le lanceur se trouvait juste à côté.");
        Add("Mineur", null, "Ouvrir la Serrure", 0, "Toucher", "Spécial", "Instantanée", "Ouvre une serrure non magique touchée par le lanceur.");
        Add("Mineur", null, "Produire un Petit Animal", 0, "Toucher", "Spécial", "Instantanée", "Fait apparaître un petit animal local attendu dans un sac, un chapeau, un terrier ou une cache similaire.");
        Add("Mineur", null, "Rafale", 0, "FM mètres", "Spécial", "Instantanée", "Crée un bref coup de vent capable d'éteindre une bougie, pousser une porte ou disperser quelques pages.");
        Add("Mineur", null, "Lumière", 0, "Vous", "Vous", "FM minutes", "Produit une lumière portée par le lanceur, généralement comparable à une torche et modulable avec de la canalisation.");
        Add("Mineur", null, "Protection Contre la Pluie", 0, "Vous", "Vous", "Bonus E heures", "Garde le lanceur au sec contre les précipitations naturelles ou similaires venues du ciel.");
        Add("Mineur", null, "Flamme Magique", 0, "Vous", "Spécial", "Instantanée", "Allume dans la main une petite flamme inoffensive pour le lanceur, mais capable de chauffer et d'enflammer comme une flamme ordinaire.");
        Add("Mineur", null, "Purifier Eau", 0, "1 mètre", "Vous", "Bonus FM rounds", "Purifie l'eau contenue dans un récipient, retirant les impuretés non magiques et rendant le liquide potable.");
        Add("Mineur", null, "Feux de Marais", 0, "FM mètres", "Spécial", "FM minutes", "Crée plusieurs lumières magiques mobiles que le lanceur peut diriger avec un test de Canalisation.");
        Add("Mineur", null, "Flétrir", 0, "1 mètre", "Spécial", "Instantanée", "Fait pourrir un petit volume de matière organique, comme nourriture, tissu, cuir ou végétation.");
        Add("Mineur", null, "Sommeil", 0, "Toucher", "1", "Bonus FM rounds", "Plonge la cible touchée dans le sommeil ; une cible déjà à terre peut devenir inconsciente.");
        Add("Mineur", null, "Murmure Chuchotant", 0, "FM mètres", "Spécial", "Bonus FM rounds", "Fait entendre la voix du lanceur depuis un point choisi à portée, indépendamment de la ligne de vue.");
        Add("Mineur", null, "Avertissement", 0, "1 mètre", "Spécial", "Instantanée", "Révèle immédiatement si un objet touché est empoisonné ou piégé.");
        Add("Mineur", null, "Printemps", 0, "Toucher", "Spécial", "Bonus FM rounds", "Fait jaillir de l'eau du sol par petites quantités pendant la durée du sort.");
        Add("Mineur", null, "Choc", 0, "Toucher", "1", "Instantanée", "La cible touchée reçoit 1 condition Étourdi.");
        Add("Mineur", null, "Mains Sournoises", 0, "Vous", "Vous", "Bonus FM rounds", "Téléporte dans la main du lanceur un petit objet de la taille d'un poing ou moins.");
        Add("Mineur", null, "Sons", 0, "FM mètres", "Spécial", "Bonus FM rounds", "Crée de petits bruits indistincts ou évocateurs à portée, pouvant être contrôlés par canalisation.");
        Add("Mineur", null, "Tics", 0, "Bonus FM mètres", "Spécial", "Instantanée", "Déplace légèrement un petit objet ; un porteur peut tenter un test de Dextérité pour ne pas le lâcher.");

        Add("Arcanique", null, "Armure Aéthyrique", 2, "Vous", "Vous", "Bonus FM rounds +", "Enveloppe le lanceur d'une protection magique accordant +1 point de protection à toutes les localisations.");
        Add("Arcanique", null, "Arme Aéthyrique", 2, "Vous", "Vous", "Bonus FM rounds +", "Crée une arme de mêlée magique dont les dégâts dépendent du Bonus de FM et dont la forme est choisie par le lanceur.");
        Add("Arcanique", null, "Bouclier Flèche", 3, "Vous", "AoE (Bonus FM mètres)", "Bonus FM rounds +", "Détruit automatiquement les projectiles organiques traversant la zone, comme les flèches à fût de bois.");
        Add("Arcanique", null, "Sang Corrosif", 4, "Vous", "Vous", "Bonus FM rounds", "Imprègne le sang du lanceur d'une puissance corrosive et lui donne le trait de créature correspondant.");
        Add("Arcanique", null, "Explosion redoutable", 4, "FM mètres", "AoE (Bonus FM mètres)", "Instantanée", "Déclenche une détonation magique en zone, traitée comme un missile magique de dégâts +3.");
        Add("Arcanique", null, "Sombre Vision", 1, "Vous", "Vous", "Bonus FM rounds", "Renforce la seconde vue et les sens ordinaires du lanceur en lui donnant Vision Noire.");
        Add("Arcanique", null, "Projectile", 4, "FM mètres", "1", "Instantanée", "Lance un projectile d'énergie, traité comme un missile magique de dégâts +4.");
        Add("Arcanique", null, "Distrayant", 4, "Vous", "Vous", "Bonus FM rounds", "Enveloppe le lanceur d'une magie perturbante qui lui donne le trait Distrayant.");
        Add("Arcanique", null, "Souffle", 6, "1 mètre", "Spécial", "Instantanée", "Produit immédiatement une attaque de souffle magique dont les dégâts dépendent du Bonus d'Endurance.");
        Add("Arcanique", null, "Dôme", 7, "Vous", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Crée un dôme protecteur contre les attaques magiques ou à distance venant de l'extérieur.");
        Add("Arcanique", null, "Pont", 4, "FM mètres", "AoE (voir description)", "Bonus FM rounds +", "Forme un pont d'énergie magique dont les dimensions dépendent du Bonus de FM et de la surpuissance.");
        Add("Arcanique", null, "Tomber", 1, "FM mètres", "1", "Instantanée", "Force une cible à lâcher un objet tenu sauf réussite d'un test de Dextérité, avec malus possible en surpuissance.");
        Add("Arcanique", null, "Attaque en Chaîne", 6, "FM mètres", "Spécial", "Instantanée", "Rayon magique de dégâts +4 pouvant rebondir vers une nouvelle cible si la précédente tombe à 0 blessure.");
        Add("Arcanique", null, "Enchevêtrement", 3, "FM mètres", "1", "Spécial", "Inflige une condition Enchevêtré dont la force dépend de l'Intelligence, avec conditions supplémentaires possibles.");
        Add("Arcanique", null, "Aura Ordinaire", 4, "Vous", "Vous", "FM minutes", "Masque l'aura magique du lanceur et de ses possessions tant qu'il ne canalise ni ne lance d'autre sort.");
        Add("Arcanique", null, "Effrayant", 3, "Vous", "Vous", "Bonus FM rounds", "Rend le lanceur intimidant et lui donne Peur 1, améliorable par la surpuissance.");
        Add("Arcanique", null, "Pousser", 6, "Vous", "Vous", "Instantanée", "Repousse les créatures vivantes proches, les met à terre et peut infliger des dégâts contre un obstacle.");
        Add("Arcanique", null, "Téléportation", 5, "Vous", "Vous", "Instantanée", "Téléporte le lanceur sur une courte distance dépendant du Bonus de FM, augmentable par la surpuissance.");
        Add("Arcanique", null, "Voler", 8, "Vous", "Vous", "Bonus FM rounds +", "Permet au lanceur de voler en gagnant le trait Vol basé sur son Agilité.");
        Add("Arcanique", null, "Bouclier Magique", 4, "Vous", "Vous", "Bonus FM rounds", "Entoure le lanceur d'une protection qui augmente ses tentatives de dissipation.");
        Add("Arcanique", null, "Terrifiant", 7, "Vous", "Vous", "Bonus FM rounds", "Confère au lanceur Terreur (1) pendant la durée du sort.");
        Add("Arcanique", null, "Éviter", 5, "Vous", "Vous", "1 round", "Protège magiquement le lanceur en lui donnant Éviter (9+).");
        Add("Arcanique", null, "Déplacer un Objet", 4, "FM mètres", "1 objet", "Bonus FM rounds", "Déplace par volonté un objet non conscient jusqu'à une taille comparable au lanceur, avec opposition possible.");

        Add("Tradition", "Bêtes", "Langue de Bête", 3, "Vous", "Vous", "FM minutes", "Permet de communiquer avec les créatures bestiales et donne un bonus aux interactions animales, mais empêche de parler normalement et de lancer des sorts.");
        Add("Tradition", "Bêtes", "Talons d'Ambre", 6, "Vous", "Vous", "Bonus FM rounds", "Transforme les ongles en serres magiques utilisables en bagarre, avec dégâts basés sur le Bonus de FM et risque de saignement.");
        Add("Tradition", "Bêtes", "Le Fléau du Destin", 8, "FM mètres", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Appelle une nuée d'oiseaux locaux qui attaque les ennemis dans la zone et les gêne fortement.");
        Add("Tradition", "Bêtes", "Forme de Bête", 5, "Vous", "Vous", "FM minutes", "Transforme le lanceur en bête du Reikland, remplaçant plusieurs caractéristiques et interdisant parole, incantation et dissipation.");
        Add("Tradition", "Bêtes", "Maître des Bêtes", 10, "Bonus FM mètres", "1", "Bonus FM jours", "Soumet une créature bestiale au rôle de protecteur fidèle capable de suivre des instructions simples.");
        Add("Tradition", "Bêtes", "Peau de Chasseur", 6, "Vous", "Vous", "Bonus FM rounds", "Couvre le lanceur d'un manteau de Ghur, améliorant son endurance et lui donnant plusieurs traits de prédateur.");
        Add("Tradition", "Bêtes", "La Lance d'Ambre", 8, "FM mètres", "Spécial", "Instantanée", "Projette une lance de Ghur en ligne droite, missile magique puissant qui traverse plusieurs cibles tant qu'il blesse.");
        Add("Tradition", "Bêtes", "Condition de Fourrure Pourpre", 9, "Vous", "Vous", "Bonus FM rounds", "Enveloppe le lanceur d'une fourrure mystique augmentant l'armure et la peur.");
        Add("Tradition", "Bêtes", "Forme Sauvage de Wyssan", 8, "Vous", "Vous", "Bonus FM rounds", "Imprègne le lanceur de sauvagerie, lui accordant plusieurs traits de créature combatifs et bestiaux.");

        Add("Tradition", "Mort", "Caresse de Laniph", 7, "Toucher", "Spécial", "Instantanée", "Attaque au toucher comme un missile magique qui ignore endurance et protection, en convertissant une partie des blessures en soins.");
        Add("Tradition", "Mort", "Mots de la Mort", 6, "Toucher", "1", "Bonus FM rounds", "Rappelle brièvement l'âme d'un mort récent pour permettre au lanceur de lui parler.");
        Add("Tradition", "Mort", "Vortex de l'âme", 8, "FM mètres", "AoE (Bonus FM mètres)", "Instantanée", "Boule de Shyish infligeant la panique aux vivants et frappant très durement les morts-vivants.");
        Add("Tradition", "Mort", "Volez la Vie", 7, "FM mètres", "1", "Instantanée", "Missile magique vampirique de dégâts +6 ignorant les armures, qui retire la fatigue du lanceur et peut le soigner.");
        Add("Tradition", "Mort", "Franchissement du Seuil", 6, "Toucher", "Spécial", "Instantanée", "Accorde une mort définitive à une cible mourante lourdement blessée et empêche son relèvement mort-vivant.");
        Add("Tradition", "Mort", "Sanctifier", 10, "Toucher", "AoE (Bonus FM mètres)", "FM minutes", "Trace un cercle protégé par Shyish que les morts-vivants ne peuvent ni franchir ni quitter.");
        Add("Tradition", "Mort", "Faux de Shythe", 6, "Vous", "Vous", "Bonus FM rounds", "Fait apparaître une faux magique utilisable en mêlée, particulièrement intimidante contre les morts-vivants.");

        Add("Tradition", "Feu", "Égide d'Aqshy", 5, "Vous", "Vous", "Bonus FM rounds", "Manteau ardent protégeant des flammes non magiques, des conditions Enflammé et de certaines attaques de feu magiques.");
        Add("Tradition", "Feu", "Cautérisation", 4, "Toucher", "1", "Instantanée", "Soigne des blessures, retire les saignements et prévient l'infection, au prix d'une douleur intense pour les non-initiés au feu.");
        Add("Tradition", "Feu", "Couronne de Flamme", 8, "Vous", "Vous", "Bonus FM rounds", "Couronne d'Aqshy accordant Peur, autorité martiale et bonus aux tests liés au feu.");
        Add("Tradition", "Feu", "Cœurs Flamboyants", 8, "FM mètres", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Rallume le courage des alliés, retire certaines conditions et confère plusieurs talents de bravoure.");
        Add("Tradition", "Feu", "Purge", 10, "FM mètres", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Consume corruption, souillure et matières inflammables dans une zone ; l'entretien demande de canaliser.");
        Add("Tradition", "Feu", "Mur de Feu", 6, "FM mètres", "AoE (spécial)", "Bonus FM rounds", "Dresse une barrière de flammes qui brûle ceux qui la traversent et inflige un impact magique.");
        Add("Tradition", "Feu", "Les Grands Feux de U'Zhul", 10, "FM mètres", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Explosion majeure d'Aqshy, missile magique très violent qui ignore l'armure et propage le feu dans la zone.");
        Add("Tradition", "Feu", "Épée Flamboyante de Rhuin", 8, "FM mètres", "1", "Bonus FM rounds", "Enflamme une épée de magie, augmentant ses dégâts et pouvant infliger Enflammé aux cibles frappées.");

        Add("Tradition", "Cieux", "Bouclier Céruléen", 7, "Vous", "Vous", "Bonus FM rounds", "Cage électrique protectrice donnant de l'armure contre la mêlée et blessant les attaquants aux armes métalliques.");
        Add("Tradition", "Cieux", "Le Premier Signe d'Amul", 3, "Vous", "Vous", "Bonus I rounds", "Accorde temporairement un point de Fortune, avec points supplémentaires possibles par surpuissance.");
        Add("Tradition", "Cieux", "Le Second Signe d'Amul", 6, "Vous", "Vous", "Bonus I rounds", "Accorde une réserve temporaire de Fortune basée sur les SL du sort.");
        Add("Tradition", "Cieux", "Comète de Casandora", 10, "I mètres", "AoE (Bonus I mètres)", "Spécial", "Appelle une comète qui frappe au round suivant, avec dérive possible selon la perception du lanceur.");
        Add("Tradition", "Cieux", "Le Troisième Signe d'Amul", 12, "Vous", "Vous", "Bonus I rounds", "Accorde temporairement un point de Destin qui disparaît s'il n'est pas utilisé avant la fin du sort.");
        Add("Tradition", "Cieux", "Les Doigts Volage du Destin", 6, "Vous", "AoE (Bonus I mètres)", "Bonus FM rounds", "Crée une réserve commune de Fortune utilisable par les alliés dans la zone.");
        Add("Tradition", "Cieux", "Traversée Étoilé", 7, "FM mètres", "1", "Bonus I rounds", "Permet de dépenser des points de Fortune pour forcer un adversaire ciblé à relancer des tests pendant la durée.");

        Add("Tradition", "Métal", "Creuset de Chamon", 7, "Bonus FM mètres", "1", "Instantanée", "Fait fondre un objet métallique non magique ; s'il est porté, le porteur peut subir un impact magique ignorant l'endurance.");
        Add("Tradition", "Métal", "Arc T'Essla", 7, "FM mètres", "1", "Instantanée", "Projette un éclair de Chamon, missile magique de dégâts +10 qui inflige Aveuglé.");
        Add("Tradition", "Métal", "Arme Enchantée", 6, "Toucher", "Spécial", "Bonus FM rounds", "Enchante une arme non magique, la rendant magique, plus dommageable et plus fiable pour la durée.");
        Add("Tradition", "Métal", "Métal Mutable", 5, "Toucher", "1", "Bonus FM rounds", "Rend un objet métallique chaud et malléable, permettant de le plier ou le remodeler avec Force ou Métier.");
        Add("Tradition", "Métal", "Plume de Plomb", 5, "FM mètres", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Altère le poids des biens des cibles, les rendant encombrées ou au contraire soulagées de l'encombrement.");
        Add("Tradition", "Métal", "Transmutation de Chamon", 12, "FM mètres", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Transforme brièvement la chair des ennemis en métal, infligeant dégâts et conditions sensorielles, avec suffocation possible.");
        Add("Tradition", "Métal", "Or des Fous", 4, "Toucher", "1", "FM minutes", "Change temporairement le métal d'un objet en or véritable, avec des conséquences pratiques laissées au MJ.");
        Add("Tradition", "Métal", "Forge de Chamon", 9, "Bonus FM mètres", "Spécial", "FM minutes", "Altère la qualité d'un article métallique en ajoutant des qualités ou en retirant des défauts.");
        Add("Tradition", "Métal", "Robe Scintillante", 5, "Vous", "Vous", "Bonus E rounds", "Entoure le lanceur d'averses de Chamon qui dévient les attaques et améliorent progressivement Éviter.");

        Add("Tradition", "Vie", "Plancher de Terre", 8, "Vous", "Vous", "Instantanée", "Permet au lanceur de disparaître dans la terre ou l'eau puis de réapparaître à distance au début du tour suivant.");
        Add("Tradition", "Vie", "La Graisse de la Terre", 4, "Toucher", "1", "Bonus FM jours", "Inonde le corps de Ghyran, supprimant le besoin de manger ou boire pendant la durée.");
        Add("Tradition", "Vie", "Forêt d'Épines", 6, "FM mètres", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Fait jaillir ronces et vignes dans une zone de terre, blessant et enchevêtrant ceux qui la traversent.");
        Add("Tradition", "Vie", "Peau d'Écorce", 3, "Toucher", "1", "Bonus FM rounds", "Durcit la peau comme de l'écorce, augmentant l'endurance au prix d'une gêne en agilité et dextérité.");
        Add("Tradition", "Vie", "Le Mensonge de la Terre", 5, "Bonus I km", "Vous", "Spécial", "Communie avec la terre pour obtenir une carte mentale des éléments naturels d'une région.");
        Add("Tradition", "Vie", "Sang de la Terre", 6, "Vous", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Tant que le lanceur touche la terre, les créatures en contact avec elle dans la zone regagnent des blessures chaque round.");
        Add("Tradition", "Vie", "Fleur de Vie", 5, "FM mètres", "Spécial", "Spécial", "Ramène à la vie ou restaure une zone naturelle, un point d'eau, des cultures ou un animal domestique malade.");
        Add("Tradition", "Vie", "Régénérer", 6, "Toucher", "1", "Bonus FM rounds", "Donne à la cible le trait Régénération pour la durée.");

        Add("Tradition", "Lumière", "Lumière Aveuglante", 8, "Bonus FM mètres", "Spécial", "Instantanée", "Émet une lumière blanche intense depuis le lanceur et inflige Aveuglé à ceux qui la regardent.");
        Add("Tradition", "Lumière", "Clarté de la Pensée", 6, "Toucher", "1", "Int minutes", "Apaise l'esprit de la cible et neutralise les modificateurs négatifs affectant ses facultés intellectuelles.");
        Add("Tradition", "Lumière", "Le Fléau du Démon", 10, "Bonus FM mètres", "1", "Instantanée", "Oppose la magie de Hysh à une créature démoniaque pour tenter de la bannir dans une lumière aveuglante.");
        Add("Tradition", "Lumière", "Guérison de la Lumière", 9, "Bonus FM mètres", "1", "Instantanée", "Soigne par lumière purificatrice et peut retirer un point de corruption récemment gagné.");
        Add("Tradition", "Lumière", "Filet d'Amyntok", 8, "Bonus Int mètres", "1", "Bonus Int rounds", "Piège l'esprit de la cible dans des énigmes de Hysh, lui imposant Étourdi tant que dure le sort.");
        Add("Tradition", "Lumière", "Bannissement", 12, "Vous", "AoE (Bonus FM mètres)", "Instantanée", "Onde purificatrice affectant les créatures faibles de la zone et ravageant les morts-vivants ou démons instables.");
        Add("Tradition", "Lumière", "La Protection de Phâ", 10, "Vous", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Crée une aura sacrée interdisant l'entrée aux créatures profanes et protégeant de la corruption.");
        Add("Tradition", "Lumière", "Vitesse de la Pensée", 8, "Vous", "Vous", "Bonus FM rounds", "Accélère l'esprit du lanceur, augmentant Intelligence et Instinct pendant la durée.");

        Add("Tradition", "Ombres", "Illusion", 8, "FM mètres", "AoE (Bonus I mètres)", "FM minutes", "Crée une illusion statique dans la zone, perceptible surtout par ceux dotés de Seconde Vue.");
        Add("Tradition", "Ombres", "Troubles de l'Esprit", 6, "1 mètre", "1", "FM minutes", "Efface temporairement le souvenir du lanceur dans l'esprit de la cible, avec risque de persistance.");
        Add("Tradition", "Ombres", "Miasme Mystifiant", 6, "FM mètres", "AoE (Bonus FM mètres)", "Bonus FM rounds", "Remplit la zone d'une brume d'Ulgu qui trouble les sens, fatigue et fait chuter les victimes.");
        Add("Tradition", "Ombres", "Ombres Étouffantes", 6, "Bonus FM mètres", "1", "Bonus FM rounds", "Étrangle la cible avec des vrilles d'ombre, l'empêchant de parler et provoquant fatigue et suffocation.");
        Add("Tradition", "Ombres", "Coursier de l'Ombre", 6, "Bonus FM mètres", "1", "Jusqu'au prochain lever du soleil", "Invoque un coursier d'ombre, monture surnaturelle rapide, discrète et instable à la lumière de l'aube.");
        Add("Tradition", "Ombres", "Doppelgangeur", 10, "Vous", "Vous", "Bonus Int minutes", "Dissimule le lanceur sous l'apparence d'un humanoïde connu, trompant les sens ordinaires.");
        Add("Tradition", "Ombres", "Bonne Volonté", 0, "Vous", "AoE (Bonus Soc mètres)", "Bonus FM rounds", "Crée une atmosphère de bonne humeur, améliorant les tests sociaux et calmant animosité ou haine dans la zone.");
        Add("Tradition", "Ombres", "Pas de l'Ombre", 8, "FM mètres", "Vous", "Instantanée", "Ouvre un passage obscur dans l'aethyr pour téléporter le lanceur et surprendre les ennemis proches.");
        Add("Tradition", "Ombres", "Chevauchée de Mirk", 0, "Vous", "Vous", "Bonus FM minutes", "Projette l'esprit du lanceur dans l'Hedge, invisible et intangible, tandis que le corps reste immobile.");
        Add("Tradition", "Ombres", "Linceul d'Invisibilité", 8, "Toucher", "1", "Bonus FM rounds", "Rend la cible invisible aux sens ordinaires, jusqu'à dissipation ou action trop voyante.");

        Add("Tradition", "Hedgecraft", "Népenthe", 0, "Toucher", "Spécial", "Bonus FM rounds", "Enchante un philtre d'herbes pour faire oublier définitivement un individu à celui qui le boit pendant la durée.");
        Add("Tradition", "Hedgecraft", "Nostrum", 0, "Toucher", "Spécial", "Bonus FM rounds", "Imprègne une préparation d'un pouvoir curatif, soignant blessures et maladies si elle est bue à temps.");
        Add("Tradition", "Hedgecraft", "Menace Rampante", 0, "FM mètres", "1", "Bonus FM rounds", "Appelle des essaims de petites créatures pour harceler et attaquer les ennemis ciblés.");
        Add("Tradition", "Hedgecraft", "Séparez les Branches", 6, "Vous", "Vous", "FM minutes", "Ouvre la perception du monde spirituel et rend visibles esprits, démons et créatures invisibles.");
        Add("Tradition", "Hedgecraft", "Charme Protecteur", 0, "Toucher", "Spécial", "Bonus FM jours", "Imprègne un charme protecteur qui confère Résistance Magique à son porteur.");

        Add("Sorcellerie", "Dhar", "Malédiction de la Douleur Paralysante", 10, "FM mètres", "1", "Bonus FM rounds", "Inflige par poupée rituelle une douleur localisée pouvant neutraliser jambe, bras, corps ou tête.");
        Add("Sorcellerie", "Dhar", "Brûlure", 14, "Bonus FM mètres", "Spécial", "Spécial", "Flétrit puits, champs ou animaux domestiques, provoquant stagnation, maladie ou ruine des récoltes.");
        Add("Sorcellerie", "Dhar", "Malédiction du Malheur", 8, "Bonus FM km", "1", "Bonus FM jours", "Attache une malchance persistante à une cible liée par un objet personnel ou une mèche de cheveux.");
        Add("Sorcellerie", "Dhar", "Horreur Hantée", 8, "Toucher", "AoE (un lieu)", "FM jours", "Hante un lieu par rêves et présences dérangeantes qui fatiguent et brisent ceux qui y entrent.");
        Add("Sorcellerie", "Dhar", "Manifestation d'un Petit Démon", 8, "Bonus FM mètres", "Spécial", "Bonus FM rounds", "Ouvre brièvement la réalité pour manifester un petit démon, contrôlé seulement si le duel de volonté réussit.");
        Add("Sorcellerie", "Dhar", "Le Mauvais Œil", 6, "FM mètres", "Spécial", "Instantanée", "Fixe une cible qui croise le regard du lanceur et lui inflige fatigue ou brisure selon l'opposition.");
        Add("Sorcellerie", "Dhar", "Pentagramme", 10, "Toucher", "AoE (Bonus FM mètres max.)", "FM minutes", "Trace un pentagramme impie empêchant les démons d'entrer ou sortir sauf volonté démesurée.");

        Add("Sombre", "Démonologie", "Détecter le Démon", 4, "FM mètres", "Spécial", "Instantanée", "Révèle automatiquement la présence d'une influence démoniaque à portée.");
        Add("Sombre", "Démonologie", "Détruire les Démons Mineur", 6, "FM mètres", "1", "Bonus FM rounds", "Perturbe le Dhar d'un démon faible, lui infligeant des blessures qui ignorent endurance et armure.");
        Add("Sombre", "Nécromancie", "Ressusciter les Morts", 8, "FM mètres", "AoE (Bonus FM mètres)", "Jusqu'au lever du soleil", "Fait se dresser des squelettes depuis le sol, sous contrôle du nécromancien jusqu'à la fin du sort.");
        Add("Sombre", "Nécromancie", "Réanimer", 8, "FM mètres", "AoE (Bonus FM mètres)", "Jusqu'au lever du soleil", "Réanime des cadavres en morts-vivants simples obéissant aux ordres du lanceur.");
        Add("Sombre", "Nécromancie", "Crâne Hurlant", 8, "FM mètres", "Spécial", "Instantanée", "Projette un crâne hurlant de Dhar en ligne droite, missile magique qui brise les vivants touchés.");
        Add("Sombre", "Nécromancie", "L'appel de Vanhel", 6, "FM mètres", "Spécial", "Instantanée", "Revigore des morts-vivants ciblés en leur accordant un mouvement ou une action libre.");
        Add("Chaos", "Slaanesh", "Acquiescement", 5, "FM mètres", "1", "Bonus FM rounds", "Submerge la cible de regrets et de désirs brisés, réduisant son instinct et limitant ses actions.");
        Add("Chaos", "Tzeentch", "Trahison de Tzeentch", 6, "FM mètres", "1", "Bonus FM rounds", "Déforme les motivations de la cible, l'empêchant d'utiliser talents et avances de compétences.");
        Add("Chaos", "Nurgle", "Flux de Corruption", 9, "Spécial", "Spécial", "Instantanée", "Déverse un souffle pestilentiel, missile magique ignorant l'armure et pouvant transmettre une maladie.");

        return sorts;
    }

    private static IEnumerable<TitreBaseReference> GetTitresBaseReference()
    {
        var libelles = new[]
        {
            "Adversaire", "Alchimiste", "Aristocrate", "Armurière", "Arpenteuse", "Aventurière", "Bottière", "Brigande",
            "Briseuse", "Charognarde", "Chèvre", "Combattante", "Crainte", "Créature", "Criminelle", "Écorcheuse",
            "Envoûteuse", "Folle furieuse", "Forgeronne", "Fossoyeuse", "Fracasseuse", "Hors-la-loi", "Imbécile",
            "Joaillière", "Malice", "Malotrue", "Mineuse", "Moissonneuse", "Nomade", "Nullité", "Ordure",
            "Randonneuse", "Ravageuse", "Recycleuse", "Résonance", "Roturière", "Souveraine", "Tireuse d'élite",
            "Traqueuse", "Abomination", "Acolyte", "Adepte", "Adjuratrice", "Amasseuse", "Âme", "Ancêtre",
            "Antagoniste", "Apothicaire", "Apparition", "Araignée", "Arnaqueuse", "Aspirante", "Assassin",
            "Autocrate", "Barbare", "Baronne", "Batailleuse", "Bénédiction", "Bergère", "Bête", "Bouchère",
            "Brasseuse", "Bricoleuse", "Brigadière", "Brume", "Brute", "Calamité", "Camarade", "Canaille",
            "Candidate", "Catastrophe", "Cavalière", "Chamane", "Championne", "Chasseuse", "Châtelaine",
            "Cheffe", "Chimère", "Chorale", "Cogneuse", "Colère", "Collectionneuse", "Complice", "Connaisseuse",
            "Conquérante", "Coordinatrice", "Coupable", "Crapule", "Dame", "Danseuse", "Démone", "Désolation",
            "Destructrice", "Diablesse", "Dirigeante", "Domination", "Druidesse", "Égide", "Égorgeuse", "Élue",
            "Enchanteresse", "Énigme", "Ennemie", "Ensorceleuse", "Épouvante", "Équipe", "Exorciste",
            "Exploratrice", "Extase", "Fanatique", "Fanfaronne", "Faucheuse", "Fleuriste", "Fouilleuse",
            "Fripouille", "Fugitive", "Gardienne", "Gloutonne", "Griffe", "Guerrière", "Guillotine", "Hantise",
            "Harponneuse", "Herboriste", "Hérétique", "Héroïne", "Horadrim", "Horreur", "Icône", "Iconoclaste",
            "Idole", "Illusion", "Immortelle", "Impostrice", "Incendiaire", "Inspectrice", "Lauréate", "Légende",
            "Légion", "Louve", "Magnate", "Maîtresse", "Maîtresse brasseuse", "Malédiction", "Mangeuse",
            "Maraudeuse", "Marchande d'armes", "Menace", "Meneuse", "Messagère", "Métamorphe", "Meurtrière",
            "Misérable", "Miséricorde", "Navigatrice", "Nécromancienne", "Nuisance", "Offensive", "Offrande",
            "Opportuniste", "Pagaille", "Parfumeuse", "Parieuse", "Partisane", "Patronne", "Pêcheresse",
            "Perdante", "Phobie", "Pirate", "Pisteuse", "Pâlie", "Poursuivante", "Prédatrice", "Prime",
            "Protectrice", "Puissance", "Rate", "Relation", "Renverseuse", "Ruine", "Saccageuse", "Sage",
            "Séductrice", "Sorcière", "Tempête", "Terreur", "Tête de mort", "Théaturge", "Tortionnaire",
            "Triomphatrice", "Troupe", "Tueuse", "Vagabonde", "Vandale", "Virtuose", "Vision", "Vitalité",
            "Voix", "Voleuse", "Voyageuse",
        };

        return libelles.Select((libelle, index) => new TitreBaseReference
        {
            Code = $"BASE_{index + 1:000}",
            Libelle = libelle,
            Ordre = index + 1,
            NiveauMaitrise = GetNiveauMaitriseTitreBase(libelle),
        });
    }

    private static IEnumerable<TitreQualificatifReference> GetTitresQualificatifReference()
    {
        var libelles = new[]
        {
            "Acharnée", "Aguerrie", "Ambrée", "Appliquée", "Apprentie", "Arriviste", "Astucieuse", "Belliqueuse",
            "Bleue", "Chanceuse", "Couper", "Cupide", "D'exception", "De fer", "De pierre", "Débutante", "Dorée",
            "Esseulée", "Fatale", "Fétide", "Fluette", "Hâtive", "Inébranlable", "Ingénieuse", "Légendaire",
            "Livide", "Magique", "Malpropre", "Noctambule", "Perfide", "Potentielle", "Préparée", "Pyromane",
            "Ranimée", "Rapide", "Rare", "Rusée", "Sinistre", "Solennelle", "Trépignante", "À longs crocs",
            "Accomplie", "Affamée", "Agile", "Aigrie", "Ambitieuse", "Anxieuse", "Argentée", "Aromatique",
            "Assiégée", "Audacieuse", "Autoritaire", "Aveugle", "Balafrée", "Baroudeuse", "Bien équipée",
            "Blafarde", "Blême", "Brisée", "Brûlante", "Brutale", "Charmeuse", "Charnelle", "Chevronnée",
            "Chuchotante", "Cinglante", "Complète", "Consacrée", "Contrariée", "Cornue", "Cramoisie",
            "Cristalline", "De la Haine", "De la Triade", "De marée", "Déchaînée", "Déchirante", "Déchue",
            "Démente", "Déserte", "Désespérée", "Dévouée", "Distillée", "Distraite", "Dominatrice",
            "Éblouissante", "Écrasante", "Effrontée", "Élémentaire", "Émérite", "Endolorie", "Enragée",
            "Ensanglantée", "Envoûtante", "Équestre", "Essentielle", "Exaltée", "Exemplaire", "Expérimentée",
            "Féroce", "Fidèle", "Fracturée", "Friable", "Fringante", "Furieuse", "Furtive", "Géante",
            "Gémissante", "Givrée", "Gourmande", "Hermétique", "Honorée", "Hurleuse", "Illuminée",
            "Imperceptible", "Impie", "Implacable", "Imprégnée", "Inaperçue", "Indéfectible", "Indomptée",
            "Infâme", "Infatigable", "Infernale", "Informe", "Insatiable", "Insensible", "Intelligente",
            "Intemporelle", "Intense", "Investie", "Invétérée", "Irrépressible", "Itinérante", "Létale",
            "Libre", "Locale", "Loyale", "Malicieuse", "Méthodique", "Minutieuse", "Moite", "Moqueuse",
            "Murmurante", "Naturelle", "Nécrophage", "Nécrotique", "Néfaste", "Noyée", "Ondulante",
            "Ornementée", "Palpitante", "Palustre", "Parfumée", "Pernicieuse", "Pieuse", "Pourpre",
            "Précieuse", "Précoce", "Profane", "Putride", "Renouvelée", "Ricanante", "Ridicule", "Rouée",
            "Rouillée", "Ruineuse", "Sacralisée", "Sacrée", "Sanctifiée", "Sanglante", "Sanguinaire",
            "Sans égale", "Saoule", "Sèche", "Sournoise", "Soyeuse", "Tempérée", "Ténébreuse", "Terrifiante",
            "Titanesque", "Titubante", "Torturée", "Tourmentée", "Toute-puissante", "Transcendée", "Vaillante",
            "Vallonnée", "Vaniteuse", "Vengeresse", "Venimeuse", "Vicieuse", "Virulente", "Vitreuse",
            "Voltaïque", "Vorace",
        };

        return libelles.Select((libelle, index) => new TitreQualificatifReference
        {
            Code = $"QUAL_{index + 1:000}",
            Libelle = libelle,
            Ordre = index + 1,
            NiveauMaitrise = GetNiveauMaitriseTitreQualificatif(libelle),
        });
    }

    private static int GetNiveauMaitriseTitreBase(string libelle)
    {
        var niveau1 = new HashSet<string>
        {
            "Acolyte", "Amasseuse", "Apothicaire", "Arpenteuse", "Aspirante", "Aventurière", "Bergère", "Bottière",
            "Brasseuse", "Bricoleuse", "Camarade", "Candidate", "Chèvre", "Complice", "Fleuriste", "Fouilleuse",
            "Fossoyeuse", "Herboriste", "Imbécile", "Malotrue", "Mineuse", "Misérable", "Nomade", "Nullité",
            "Ordure", "Parfumeuse", "Parieuse", "Perdante", "Rate", "Relation", "Roturière", "Vagabonde",
            "Voleuse", "Voyageuse",
        };

        var niveau4 = new HashSet<string>
        {
            "Ancêtre", "Autocrate", "Championne", "Conquérante", "Domination", "Élue", "Héroïne", "Horadrim",
            "Icône", "Idole", "Immortelle", "Légende", "Légion", "Nécromancienne", "Puissance", "Souveraine",
            "Tempête", "Terreur", "Triomphatrice", "Vitalité", "Voix",
        };

        var niveau3 = new HashSet<string>
        {
            "Abomination", "Adjuratrice", "Antagoniste", "Apparition", "Assassin", "Barbare", "Baronne", "Batailleuse",
            "Bénédiction", "Bête", "Bouchère", "Brume", "Brute", "Calamité", "Catastrophe", "Chamane", "Châtelaine",
            "Cheffe", "Chimère", "Colère", "Connaisseuse", "Crainte", "Dame", "Démone", "Désolation", "Destructrice",
            "Diablesse", "Dirigeante", "Druidesse", "Égide", "Égorgeuse", "Enchanteresse", "Énigme", "Ensorceleuse",
            "Épouvante", "Exorciste", "Extase", "Fanatique", "Faucheuse", "Gardienne", "Gloutonne", "Griffe",
            "Guerrière", "Guillotine", "Hantise", "Hérétique", "Horreur", "Iconoclaste", "Illusion", "Incendiaire",
            "Lauréate", "Louve", "Magnate", "Maîtresse", "Malédiction", "Maraudeuse", "Menace", "Métamorphe",
            "Meurtrière", "Miséricorde", "Offensive", "Phobie", "Prédatrice", "Prime", "Protectrice", "Renverseuse",
            "Ruine", "Saccageuse", "Sage", "Séductrice", "Sorcière", "Tête de mort", "Théaturge", "Tortionnaire",
            "Tueuse", "Vandale", "Virtuose", "Vision",
        };

        if (niveau1.Contains(libelle)) return 1;
        if (niveau4.Contains(libelle)) return 4;
        if (niveau3.Contains(libelle)) return 3;
        return 2;
    }

    private static int GetNiveauMaitriseTitreQualificatif(string libelle)
    {
        var niveau1 = new HashSet<string>
        {
            "Appliquée", "Apprentie", "Arriviste", "Bleue", "Chanceuse", "Couper", "Débutante", "Esseulée",
            "Fluette", "Hâtive", "Malpropre", "Noctambule", "Potentielle", "Précoce", "Ricanante", "Ridicule",
            "Saoule", "Titubante", "Vallonnée",
        };

        var niveau4 = new HashSet<string>
        {
            "D'exception", "Inébranlable", "Légendaire", "Sans égale", "Titanesque", "Toute-puissante",
            "Transcendée",
        };

        var niveau3 = new HashSet<string>
        {
            "À longs crocs", "Accomplie", "Aguerrie", "Audacieuse", "Autoritaire", "Belliqueuse", "Bien équipée",
            "Brûlante", "Brutale", "Cinglante", "Consacrée", "Cornue", "Cramoisie", "Cristalline", "De la Haine",
            "De la Triade", "De marée", "De fer", "De pierre", "Déchaînée", "Déchirante", "Déchue", "Démente",
            "Dévouée", "Dominatrice", "Éblouissante", "Écrasante", "Élémentaire", "Émérite", "Enragée",
            "Ensanglantée", "Envoûtante", "Exaltée", "Exemplaire", "Expérimentée", "Fatale", "Féroce", "Furieuse",
            "Géante", "Hermétique", "Honorée", "Hurleuse", "Illuminée", "Imperceptible", "Impie", "Implacable",
            "Imprégnée", "Indéfectible", "Indomptée", "Infâme", "Infatigable", "Infernale", "Insatiable",
            "Intemporelle", "Intense", "Invétérée", "Irrépressible", "Létale", "Nécrophage", "Nécrotique",
            "Néfaste", "Pernicieuse", "Pourpre", "Précieuse", "Profane", "Putride", "Ruineuse", "Sacralisée",
            "Sacrée", "Sanctifiée", "Sanglante", "Sanguinaire", "Ténébreuse", "Terrifiante", "Torturée",
            "Tourmentée", "Vaillante", "Vengeresse", "Venimeuse", "Vicieuse", "Virulente", "Voltaïque", "Vorace",
        };

        if (niveau1.Contains(libelle)) return 1;
        if (niveau4.Contains(libelle)) return 4;
        if (niveau3.Contains(libelle)) return 3;
        return 2;
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

    private static IEnumerable<CreatureReference> GetCreaturesReference()
    {
        var creatures = new List<CreatureReference>();

        void Add(string categorie, string nom, int page, int m, int cc, int ct, int f, int e, int i, int ag, int dex, int intel, int fm, int soc, int b, string traits, string? optionnels = null)
        {
            creatures.Add(new CreatureReference
            {
                Code = $"{categorie}:{nom}",
                Nom = nom,
                Categorie = categorie,
                Page = page,
                M = m, CC = cc, CT = ct, F = f, E = e, I = i, Ag = ag, Dex = dex, Int = intel, FM = fm, Soc = soc, B = b,
                Traits = traits,
                TraitsOptionnels = optionnels,
            });
        }

        // --- Peuples du Reikland ---
        Add("Peuples du Reikland", "Humains", 311, 4, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 12,
            "Préjudice (choisir un), Arme+7",
            "Maladie, Distance+8 (50), Lanceur de Sorts");
        Add("Peuples du Reikland", "Nains", 311, 3, 40, 30, 30, 40, 30, 20, 40, 30, 50, 20, 16,
            "Animosité (choisir un), Haine (Peaux-Verte), Résistance Magique (1), Vision Nocturne, Préjudice (choisir un), Arme+7",
            "Fureur, Distance+8 (50)");
        Add("Peuples du Reikland", "Halflings", 311, 3, 20, 40, 20, 30, 30, 30, 40, 30, 40, 40, 10,
            "Vision Nocturne, Taille (Petit), Arme+5",
            "Distance+7 (25), Discret");
        Add("Peuples du Reikland", "Elfes", 311, 5, 40, 40, 30, 30, 50, 40, 40, 40, 40, 30, 13,
            "Animosité (choisir un), Préjudice (choisir deux), Vision Nocturne, Arme+7",
            "Arboricole, Magique, Résistance Magique, Distance+9 (150), Discret, Lanceur de Sorts (n'importe lequel), Traqueur");
        Add("Peuples du Reikland", "Ogres", 312, 6, 30, 20, 45, 45, 10, 25, 20, 20, 30, 20, 30,
            "Armure 1, Affamé, Préjudice (Peuple Mince), Vision Nocturne, Taille (Grand), Arme+8",
            "Belliqueux, Infecté, Traqueur");

        // --- Exemples de PNJ ---
        Add("Exemples de PNJ", "Pol Dankels", 312, 4, 24, 26, 27, 46, 49, 26, 34, 65, 47, 44, 14,
            "Ruse, Intelligent, Préjudice (Sigmarites), Lanceur de Sorts (Sorcellerie), Dure, Arme+5");
        Add("Exemples de PNJ", "Bella la Noir", 313, 3, 32, 35, 41, 45, 30, 25, 29, 27, 34, 33, 19,
            "Animosité (les Riches, Hommes-Bêtes), Arboricole, Armure (Légère 2), Préjudice (Huissiers, Avocats), Distance+8 (50), Brute, Robuste, Arme+8");

        // --- Bêtes du Reikland ---
        Add("Bêtes du Reikland", "Sanglier", 314, 7, 35, 0, 33, 35, 33, 35, 0, 10, 10, 0, 10,
            "Armure 1, Bestial, Cornes (Défenses), Vision Nocturne, Nerveux, Foulée, Arme+6",
            "Belliqueux, Frénésie, Infecté, Infesté, Taille (Grand), Territorial, Entraîné (Brisé, Magique, Monture, Guerre)");
        Add("Bêtes du Reikland", "Ours", 314, 4, 35, 0, 55, 45, 20, 25, 15, 10, 15, 0, 28,
            "Armure 1, Bestial, Morsure+9, Vision Nocturne, Taille (Grand), Nerveux, Foulée, Arme+8",
            "Affamé, Infecté, Infestation, Taille (Énorme), Territorial, Entraîné (Brisé, Divertir, Guerre)");
        Add("Bêtes du Reikland", "Chiens", 315, 4, 25, 0, 20, 20, 35, 30, 0, 15, 10, 15, 5,
            "Bestial, Vision Nocturne, Nerveux, Taille (Petit), Foulée, Arme+5",
            "Armure 1, Frénésie, Infecté, Taille (Petit à Moyen), Territorial, Traqueur, Entraîné (Brisé, Divertir, Chercher, Garde, Magie, Guerre)");
        Add("Bêtes du Reikland", "Rats Géants", 315, 4, 25, 0, 30, 25, 25, 35, 0, 15, 15, 0, 5,
            "Bestial, Infecté, Vision Nocturne, Taille (Petite), Nerveux, Foulée, Arme+4",
            "Armure 1, Maladie (fièvre de rat ou peste noire), Taille (Petite à Moyenne), Essaim, Entraîné (Guerre)");
        Add("Bêtes du Reikland", "Araignées Géantes", 315, 5, 35, 25, 15, 25, 10, 35, 30, 5, 25, 0, 2,
            "Bestial, Vision Nocturne, Taille (Petite), Marcher sur les Murs, Toile 40, Arme+3",
            "Armure 1, Arboricole, Morsure, Taille (Petite à Énorme), Essaim, Venin (Moyen), Entraîné (Brisé, Garde, Magie, Monture, Guerre)");
        Add("Bêtes du Reikland", "Chevaux", 316, 7, 25, 0, 45, 35, 15, 30, 0, 10, 10, 10, 22,
            "Bestial, Taille (Large), Nerveux, Foulée, Arme+7",
            "Armure, Entraîné (Brisé, Conduire, Divertir, Magie, Monture, Guerre)");
        Add("Bêtes du Reikland", "Pigeons", 316, 2, 15, 0, 5, 15, 25, 40, 0, 10, 20, 10, 1,
            "Bestial, Vol 100, Taille (Minuscule), Nerveux, Arme+0",
            "Taille (Petite), Entraîné (Brisé, À la maison)");
        Add("Bêtes du Reikland", "Serpents", 316, 3, 40, 0, 30, 25, 25, 40, 0, 5, 45, 0, 8,
            "Armure 1, Bestial, Sang-Froid, Rapide, Taille (Petite), Arme+5",
            "Constricteur, Taille (Minuscule à Énorme), Arpenteur de Marécage, Essaim, Venin (Très facile à Très difficile)");
        Add("Bêtes du Reikland", "Loups", 316, 4, 35, 0, 35, 30, 35, 30, 0, 15, 15, 0, 10,
            "Armure 1, Bestial, Vision Nocturne, Nerveux, Foulée, Traqueur, Arme+6",
            "Frénésie, Infecté, Taille (Grande), Territorial, Entraîné (Brisé, Conduit, Va chercher, Garde, Magie, Monture, Guerre)");

        // --- Bêtes Monstrueuses du Reikland ---
        Add("Bêtes Monstrueuses", "Basilic", 317, 4, 45, 35, 55, 55, 25, 15, 0, 15, 15, 0, 64,
            "Armure 2, Bestial, Morsure+9, Sang-Froid, Immunité (poison), Infecté, Vision Nocturne, Regard Pétrifiant, Taille (Énorme), Foulée, Queue+8, Venin, Arme+9",
            "Mutant, Territorial");
        Add("Bêtes Monstrueuses", "Pieuvre des Marais", 318, 3, 35, 0, 80, 75, 15, 55, 0, 5, 65, 0, 56,
            "Amphibie, Bestial, Constricteur, Taille (Grande), Discret, Arpenteur de Marécage, 8×Tentacules+9",
            "Taille (Énorme à Monstrueux), Territorial");
        Add("Bêtes Monstrueuses", "Squigs des Cavernes", 318, 4, 45, 0, 50, 30, 10, 40, 0, 5, 15, 0, 12,
            "Bestial, Rebond, Infecté, Vision Nocturne, Arme+9",
            "Aquatique, Souffle (acide ou gaz), Vision Obscure, Frénésie, Fureur, Cornes, Taille (Mini à Énorme)");
        Add("Bêtes Monstrueuses", "Demigryphes", 318, 7, 35, 0, 55, 40, 30, 45, 0, 15, 25, 0, 30,
            "Armure 1, Bestial, Morsure+9, Vision Nocturne, Taille (Large), Foulée, Arme+9",
            "Entraîné (Brisé, Conduite, Garde, Monture, Guerre)");
        Add("Bêtes Monstrueuses", "Dragons", 319, 6, 65, 60, 65, 65, 60, 25, 15, 45, 85, 25, 104,
            "Armure 5, Morsure+10, Souffle+15 (divers), Vol 80, Vision Nocturne, Taille (Énorme), Queue+9, Arme+10",
            "Arboricole, Immunité (en choisir une), Infesté, Magie, Corruption mentale, Mutation, Taille (Monstrueux), Lanceur de Sorts (divers), Arpenteur de Marécage, Entraîné (Monture), Morts-vivants, Venin");
        Add("Bêtes Monstrueuses", "Bêtes des Marais", 319, 5, 35, 0, 50, 55, 10, 15, 10, 0, 0, 0, 40,
            "Fabriquer, Vision Obscure, Dure à Tuer, Infecté, Régénérer, Taille (Grande), Stupide, Arpenteur de Marécage, Instable, Arme+8",
            "Frénésie, Affamé, Infesté, Territorial");
        Add("Bêtes Monstrueuses", "Fimir", 320, 6, 35, 20, 45, 40, 30, 20, 20, 30, 30, 15, 30,
            "Armure 2, Sang-Froid, Vision Nocturne, Taille (Large), Arpenteur de Marécage, Arme+8",
            "Queue+7, Magie (Démonologie)");
        Add("Bêtes Monstrueuses", "Géants", 320, 6, 30, 30, 65, 55, 30, 20, 15, 25, 25, 20, 72,
            "Armure 1, Vision Nocturne, Taille (Énorme), Foulée, Solide, Arme+10",
            "Bestial, Souffle (Vomissement ivre), Affamé, Infecté, Infesté, Taille (Monstrueux), Stupide");
        Add("Bêtes Monstrueuses", "Griffons", 321, 6, 50, 0, 50, 50, 45, 60, 0, 20, 40, 0, 76,
            "Armure 1, Bestial, Morsure+9, Vol 80, Vision Nocturne, Taille (Énorme), Arme+9",
            "Entraîné (Brisé, Garde, Magie, Monture, Guerre)");
        Add("Bêtes Monstrueuses", "Hippogryphes", 321, 7, 45, 0, 55, 50, 20, 55, 0, 5, 35, 0, 72,
            "Animosité (Tout), Belliqueux, Bestial, Morsure+9, Vol 120, Vision Nocturne, Taille (Large), Foulée, Territorial, Arme+9",
            "Brisé, Frénésie, Fureur, Haine (Tout), Entraîné (Brisé, Monture)");
        Add("Bêtes Monstrueuses", "Hydre", 322, 6, 45, 0, 50, 55, 15, 35, 0, 15, 25, 0, 68,
            "Armure 3, Bestial, Souffle+10 (Feu), Constricteur, Vision Nocturne, Régénération, Taille (Énorme), Discret, Foulée, Traqueur, Arme+9",
            "Belliqueux, Territorial, Venin");
        Add("Bêtes Monstrueuses", "Jabberslythe", 322, 7, 45, 40, 55, 50, 20, 35, 0, 10, 20, 0, 68,
            "Armure 3, Bestial, Morsure+9, Rebond, Sang Corrosif, Distraction, Infecté, Vision Nocturne, Taille (Énorme), Queue+8, Langue+5 (12), Venin, Arme+9",
            "Mutant, Territorial");
        Add("Bêtes Monstrueuses", "Manticores", 323, 6, 55, 0, 55, 55, 50, 65, 0, 10, 35, 0, 72,
            "Armure 2, Bestial, Morsure+9, Vol 80, Taille (Énorme), Queue+8, Territorial, Venin, Arme+9",
            "Haine (Prédateurs), Mutant, Entraîné (Brisé, Magique, Monture)");
        Add("Bêtes Monstrueuses", "Pégase", 323, 8, 35, 0, 45, 40, 30, 45, 0, 20, 25, 0, 28,
            "Vol 100, Taille (Large), Foulée, Arme+7",
            "Entraîné (Brisé, Conduit, Magique, Monture, Guerre)");
        Add("Bêtes Monstrueuses", "Trolls", 324, 6, 30, 15, 55, 45, 10, 15, 15, 10, 20, 5, 30,
            "Armure 2, Morsure+8, Dure à Tuer, Infecté, Régénérer, Taille (Grande), Stupide, Solide, Vomi, Arme+9",
            "Aquatique, Bestial, Frénésie, Affamé, Infesté, Résistance Magique, Mutation, Vision Nocturne, Insensible, Discret, Arpenteur de Marécage");
        Add("Bêtes Monstrueuses", "Wyvernes", 324, 4, 55, 0, 60, 55, 15, 45, 0, 10, 50, 0, 84,
            "Armure 2, Bestial, Vol 90, Taille (Énorme), Venin, Arme+10",
            "Souffle (Venin), Cornes, Queue+9, Entraîné (Brisé, Garde, Magie, Monture, Guerre)");

        // --- Hordes des Peaux-Vertes ---
        Add("Hordes des Peaux-Vertes", "Orques", 325, 4, 35, 30, 35, 45, 20, 25, 20, 25, 35, 20, 14,
            "Armure 3, Animosité (Peaux-Vertes), Belliqueux, Dure à Tuer, Infecté, Vision Nocturne, Arme+8",
            "Insensible, Distance+8 (50), Taille (Grand)");
        Add("Hordes des Peaux-Vertes", "Gobelins", 326, 4, 25, 35, 30, 30, 20, 35, 30, 30, 20, 20, 11,
            "Animosité (Peaux-Vertes), Armure 1, Peur (Elfes), Infecté, Vision Obscure, Arme+7",
            "Arboricole, Haine (Nains), Vision Nocturne, Distance+7 (25), Venin");
        Add("Hordes des Peaux-Vertes", "Snotlings", 326, 4, 25, 15, 25, 20, 20, 30, 0, 15, 30, 0, 7,
            "Bestial, Infecté, Vision Nocturne, Taille (Petite), Arme+4",
            "Brisé, Essaim, Entraîné (Brisé, Ramassé, Garde), Venin");

        // --- La Mort Agitée ---
        Add("La Mort Agitée", "Squelettes", 327, 4, 25, 25, 30, 30, 20, 20, 25, 0, 0, 0, 12,
            "Armure 2, Fabriquer, Vision Obscure, Peur 2, Insensible, Morts-vivants, Instable, Arme+7",
            "Corruption (Mineure), Infecté, Territorial");
        Add("La Mort Agitée", "Zombies", 328, 4, 15, 0, 30, 30, 5, 10, 15, 0, 0, 0, 12,
            "Fabriquer, Vision Obscure, Peur 2, Insensible, Morts-vivants, Instable, Arme+7",
            "Armures, Corruption (Mineure), Malade, Distrayant, Infecté, Infesté, Territorial");
        Add("La Mort Agitée", "Loups Épouvantable", 328, 9, 30, 0, 35, 35, 30, 30, 0, 0, 0, 0, 24,
            "Armure 1, Fabriquer, Vision Obscure, Peur 2, Taille (Large), Foulée, Traqueur, Morts-vivants, Instable, Arme+6",
            "Corruption (Mineure), Distrayant, Infecté, Insensible, Territorial");
        Add("La Mort Agitée", "Ghouls de Crypte", 329, 4, 30, 0, 35, 30, 30, 35, 25, 20, 20, 5, 11,
            "Morsure+5, Infecté, Vision Nocturne, Arme+6",
            "Bestial, Insensible, Venin");
        Add("La Mort Agitée", "Varghouls", 329, 8, 55, 0, 55, 55, 30, 50, 20, 10, 60, 0, 42,
            "Armure 1, Bestial, Morsure+8, Peur 4, Vision Obscure, Haine (Vivant), Affamé, Régénération, Taille (Grande), Morts-vivants, Vampirique, Arme+9",
            "Corruption (Mineure), Fuite, Fureur, Terreur 3, Territorial, Traqueur");
        Add("La Mort Agitée", "Spectre de Cairn", 329, 6, 35, 0, 35, 30, 15, 30, 25, 25, 50, 15, 14,
            "Étreinte Glaciale, Vision Obscure, Éthéré, Terreur 3, Morts-vivants, Instable, Arme+9",
            "Bestial, Champion, Insensible, Territorial");
        Add("La Mort Agitée", "Banshees de Tombes", 330, 6, 30, 0, 30, 30, 20, 30, 30, 25, 40, 20, 13,
            "Vision Obscure, Éthéré, Hurlement Fantomatique, Terreur 3, Morts-vivants, Instable, Arme+7",
            "Bestial, Fuite, Fureur, Insensible, Territorial");
        Add("La Mort Agitée", "Vampires", 330, 6, 60, 40, 50, 40, 50, 70, 40, 40, 60, 40, 19,
            "Morsure+8, Vision Nocturne, Morts-vivants, Vampirique, Arme+9",
            "Bestial, Champion, Corruption (Mineure), Vision Obscure, Dure à Tuer, Distrayant, Peur, Vol, Frénésie, Fureur, Affamé, Corruption mentale, Insensible, Pétrifiant, Génération, Magie (Mort ou Nécromancie), Traqueur, Marcher sur les Murs");
        Add("La Mort Agitée", "Fantômes", 330, 6, 30, 0, 30, 30, 10, 30, 20, 15, 15, 0, 10,
            "Vision Obscure, Éthéré, Peur 2, Morts-vivants, Instable, Arme+6",
            "Bestial, Fureur, Haine, Essaim, Territorial");

        // --- Esclaves des Ténèbres ---
        Add("Esclaves des Ténèbres", "Gors", 331, 4, 45, 30, 35, 45, 30, 35, 25, 25, 30, 25, 14,
            "Arboricole, Armure 1, Fureur, Cornes+6, Vision Nocturne, Arme+7",
            "Armure 2, Corruption (Mineure), Maladie (variole de la meute), Infecté, Infesté, Mutation, Taille (Grande), Lanceur de Sorts (Bêtes)");
        Add("Esclaves des Ténèbres", "Ungors", 332, 4, 35, 30, 30, 35, 30, 35, 25, 25, 35, 25, 12,
            "Arboricole, Vision Nocturne, Arme+6",
            "Armure 1, Corruption (Mineure), Maladie (variole de la meute), Infecté, Infesté, Mutation, Distance+7 (25), Taille (Petite)");
        Add("Esclaves des Ténèbres", "Minotaures", 332, 6, 45, 25, 44, 45, 20, 35, 25, 20, 30, 15, 30,
            "Cornes+9, Affamé, Vision Nocturne, Taille (Grand), Arme+9",
            "Arboricole, Belliqueux, Corruption (Mineure), Maladie (variole de la meute), Fureur, Infecté, Infesté, Mutation");
        Add("Esclaves des Ténèbres", "Bray-Shaman", 333, 4, 40, 30, 30, 45, 40, 35, 25, 30, 50, 30, 16,
            "Arboricole, Corruption (Mineure), Fureur, Cornes+6, Vision Nocturne, Magie (Bêtes, Tout Chaos, Mort ou Ombre), Arme+7",
            "Maladie (variole de la meute), Infecté, Infesté, Mutation, Taille (Grande)");
        Add("Esclaves des Ténèbres", "Mutants", 333, 4, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 12,
            "Corruption (Mineure), Mutation, Arme+7",
            "Tous les Traits de Créatures");
        Add("Esclaves des Ténèbres", "Cultistes", 333, 4, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 12,
            "Arme+6",
            "Armure 1, Corruption (Mineure), Mutation, Magie (Chaos)");
        Add("Esclaves des Ténèbres", "Guerriers du Chaos", 334, 4, 55, 30, 45, 45, 45, 55, 30, 35, 55, 25, 17,
            "Armure 5, Champion, Corruption (Mineure), Arme+8",
            "Belliqueux, Maladie, Distrayant, Frénésie, Corruption mentale, Mutation, Magie (Chaos)");
        Add("Esclaves des Ténèbres", "Sanguinaires de Khorne", 335, 5, 55, 35, 45, 35, 60, 40, 30, 25, 70, 15, 17,
            "Armure 5, Champion, Griffes, Corruption (Modérée), Démoniaque 8+, Peur 3, Frénésie, Cornes+8, Insensible, Instable, Arme+9");
        Add("Esclaves des Ténèbres", "Demonettes de Slaanesh", 335, 4, 60, 50, 40, 30, 65, 60, 35, 30, 70, 45, 17,
            "Champion, Corruption (Modérée), Démoniaque 8+, Distrayant, Peur 2, Vision Nocturne, Instable, Arme+9");
        Add("Esclaves des Ténèbres", "Slenderthigh Fouetlangue", 336, 6, 95, 110, 115, 120, 100, 95, 40, 70, 85, 85, 86,
            "Armure 1, Champion, Corruption (Majeure), Démoniaque 8+, Distrayant, Cornes+15, Vision Nocturne, Taille (Grand), Magie (Slaanesh), Terreur 3, Instable, Arme+16");
        Add("Esclaves des Ténèbres", "Fr'hough Bouchesoufle", 336, 4, 70, 35, 120, 150, 50, 20, 30, 85, 120, 50, 108,
            "Armure 4, Souffle+12 (Corrosion), Corruption (Majeure), Démoniaque 7+, Vision Obscure, Maladie (Variole purulente), Cornes+14, Infecté, Infesté, Taille (Grand), Magie (Nurgle), Terreur 3, Instable, Arme+15");
        Add("Esclaves des Ténèbres", "Guerrier des Clans", 337, 5, 30, 30, 30, 30, 40, 35, 30, 30, 20, 20, 11,
            "Armure 2, Infecté, Vision Nocturne, Arme+7",
            "Maladie (fièvre de rat), Mutation, Craintif, Discrétion, Pisteur");
        Add("Esclaves des Ténèbres", "Vermines de Choc", 337, 5, 45, 35, 35, 35, 55, 50, 30, 30, 25, 20, 11,
            "Armure 4, Infecté, Vision Nocturne, Arme+8",
            "Maladie (fièvre de rat), Mutation, Pisteur");
        Add("Esclaves des Ténèbres", "Rat Ogres", 337, 5, 35, 10, 55, 45, 35, 45, 25, 10, 25, 15, 30,
            "Armure 1, Infecté, Vision Nocturne, Taille (Grand), Stupide, Arme+9",
            "Corruption (Mineure), Vision Obscure, Maladie (fièvre de rat), Infesté, Mutation, Queue+8, Traqueur, Entraîné (Brisé, Garde, Monture, Guerre)");

        return creatures;
    }
}
