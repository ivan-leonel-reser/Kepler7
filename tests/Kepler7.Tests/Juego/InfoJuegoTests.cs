using Kepler7.Core.Juego;

namespace Kepler7.Tests.Juego;

[TestClass]
public class InfoJuegoTests
{
    [TestMethod]
    public void Integrantes_SonCincoYSinRepetidos()
    {
        Assert.HasCount(5, InfoJuego.Integrantes);
        Assert.HasCount(5, InfoJuego.Integrantes.Distinct().ToList());
    }
}
