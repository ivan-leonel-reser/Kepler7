using Kepler7.Core.Progreso;

namespace Kepler7.Tests.Progreso;

[TestClass]
public class ProgresoTests
{
    [TestMethod]
    public void GanarXp_SubeDeNivelCadaCienPuntos()
    {
        var progreso = new ProgresoJugador();

        Assert.IsFalse(progreso.GanarXp(60));
        Assert.IsTrue(progreso.GanarXp(60));
        Assert.AreEqual(2, progreso.Nivel);
        Assert.AreEqual(20, progreso.XpEnNivelActual);
    }

    [TestMethod]
    public void GastarCreditos_NoDescuentaSiNoAlcanza()
    {
        var progreso = new ProgresoJugador();
        progreso.GanarCreditos(50);

        Assert.IsFalse(progreso.GastarCreditos(80));
        Assert.AreEqual(50, progreso.Creditos);
        Assert.IsTrue(progreso.GastarCreditos(30));
        Assert.AreEqual(20, progreso.Creditos);
    }

    [TestMethod]
    public void Insignia_SeDesbloqueaUnaSolaVez()
    {
        var progreso = new ProgresoJugador();

        Assert.IsTrue(progreso.DesbloquearInsignia(Patron.Facade));
        Assert.IsFalse(progreso.DesbloquearInsignia(Patron.Facade));
        Assert.HasCount(1, progreso.Insignias);
    }

    [TestMethod]
    public void Sectores_NoPasanDelTotal()
    {
        var progreso = new ProgresoJugador();

        for (var i = 0; i < 10; i++)
            progreso.RecuperarSector();

        Assert.AreEqual(ProgresoJugador.SectoresTotales, progreso.SectoresRecuperados);
    }

    [TestMethod]
    public void Restaurar_RecuperaTodosLosValores()
    {
        var progreso = ProgresoJugador.Restaurar(250, 40, 2, [Patron.Builder, Patron.Proxy]);

        Assert.AreEqual(3, progreso.Nivel);
        Assert.AreEqual(40, progreso.Creditos);
        Assert.AreEqual(2, progreso.SectoresRecuperados);
        Assert.IsTrue(progreso.TieneInsignia(Patron.Proxy));
    }

    [TestMethod]
    public void Catalogo_TieneUnaInsigniaPorPatron()
    {
        var patrones = Enum.GetValues<Patron>();

        Assert.HasCount(patrones.Length, CatalogoInsignias.Todas);
        foreach (var patron in patrones)
            Assert.AreEqual(patron, CatalogoInsignias.Obtener(patron).Patron);
    }

    [TestMethod]
    public void Catalogo_RespetaLasFamiliasDeLaConsigna()
    {
        Assert.HasCount(2, CatalogoInsignias.Todas.Where(i => i.Familia == FamiliaPatron.Creacional).ToList());
        Assert.HasCount(4, CatalogoInsignias.Todas.Where(i => i.Familia == FamiliaPatron.Estructural).ToList());
        Assert.HasCount(6, CatalogoInsignias.Todas.Where(i => i.Familia == FamiliaPatron.Comportamiento).ToList());
    }
}
