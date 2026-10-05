using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;

namespace Wfrp4.Infrastructure.Tests;

public class UnitTest1
{
    [Fact]
    public async Task SeedAsync_populates_reference_data_once()
    {
        var options = new DbContextOptionsBuilder<Wfrp4DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new Wfrp4DbContext(options);

        await Wfrp4DataSeeder.SeedAsync(db);
        await Wfrp4DataSeeder.SeedAsync(db);

        Assert.Equal(5, await db.Especes.CountAsync());
        Assert.True(await db.Classes.AnyAsync());
        Assert.True(await db.Carrieres.AnyAsync());
        Assert.True(await db.Competences.AnyAsync());
        Assert.True(await db.Talents.AnyAsync());
        Assert.True(await db.SortsReference.AnyAsync());
        Assert.All(db.Especes, espece => Assert.False(string.IsNullOrWhiteSpace(espece.CaracInitiales)));
    }
}
