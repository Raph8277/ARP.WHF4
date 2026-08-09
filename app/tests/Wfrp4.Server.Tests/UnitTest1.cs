using Wfrp4.Server.Services;

namespace Wfrp4.Server.Tests;

public class UnitTest1
{
    [Fact]
    public void CalculerCoutCaracteristique_respecte_les_paliers()
    {
        var service = new XPService();

        Assert.Equal(25, service.CalculerCoutCaracteristique(0));
        Assert.Equal(25, service.CalculerCoutCaracteristique(4));
        Assert.Equal(30, service.CalculerCoutCaracteristique(5));
        Assert.Equal(230, service.CalculerCoutCaracteristique(49));
    }

    [Fact]
    public void CalculerCoutCompetence_respecte_les_paliers()
    {
        var service = new XPService();

        Assert.Equal(10, service.CalculerCoutCompetence(0));
        Assert.Equal(15, service.CalculerCoutCompetence(5));
        Assert.Equal(180, service.CalculerCoutCompetence(49));
    }
}
