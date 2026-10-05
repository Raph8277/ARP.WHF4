using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Data;

namespace Wfrp4.Server.Tests;

/// <summary>Base EF InMemory isolée : ne prouve ni contraintes, ni FK, ni migrations PostgreSQL.</summary>
internal static class TestDb
{
    public static Wfrp4DbContext Nouvelle() =>
        new(new DbContextOptionsBuilder<Wfrp4DbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
