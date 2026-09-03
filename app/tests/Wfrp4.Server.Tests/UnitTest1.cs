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

    [Fact]
    public void CalculerCoutCaracteristiqueTotal_rembourse_les_avances_retires()
    {
        var service = new XPService();

        Assert.Equal(-25, service.CalculerCoutCaracteristiqueTotal(0, -1));
        Assert.Equal(-25, service.CalculerCoutCaracteristiqueTotal(1, -1));
        Assert.Equal(-55, service.CalculerCoutCaracteristiqueTotal(6, -2));
    }

    [Fact]
    public void CalculerCoutCompetenceTotal_rembourse_les_avances_retires()
    {
        var service = new XPService();

        Assert.Equal(-10, service.CalculerCoutCompetenceTotal(0, -1));
        Assert.Equal(-10, service.CalculerCoutCompetenceTotal(1, -1));
        Assert.Equal(-25, service.CalculerCoutCompetenceTotal(6, -2));
    }

    [Fact]
    public void CalculerCoutTalentTotal_rembourse_les_occurrences_retires()
    {
        var service = new XPService();

        Assert.Equal(-100, service.CalculerCoutTalentTotal(0, -1));
        Assert.Equal(-100, service.CalculerCoutTalentTotal(1, -1));
        Assert.Equal(-500, service.CalculerCoutTalentTotal(3, -2));
    }
}
