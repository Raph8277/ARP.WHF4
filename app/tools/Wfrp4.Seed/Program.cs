using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Configuration de la connexion
        var connectionString = args.Length > 0
            ? args[0]
            : "Host=localhost;Port=5432;Database=wfrp4_dev;Username=wfrp4;Password=changeme";

        var optionsBuilder = new DbContextOptionsBuilder<Wfrp4DbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        using var db = new Wfrp4DbContext(optionsBuilder.Options);

        Console.WriteLine("Application des migrations...");
        await db.Database.MigrateAsync();

        Console.WriteLine("Application du seed des référentiels...");
        await Wfrp4DataSeeder.SeedAsync(db);
        Console.WriteLine("Seed terminé !");
    }
}