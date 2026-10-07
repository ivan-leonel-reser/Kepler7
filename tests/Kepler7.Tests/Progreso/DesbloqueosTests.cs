using Kepler7.Core.Juego;
using Kepler7.Core.Progreso;

namespace Kepler7.Tests.Progreso;

[TestClass]
public class DesbloqueosTests
{
    [TestMethod]
    public void EnNivelUnoNoHayNadaDesbloqueadoYElProximoEsElNivelDos()
    {
        Assert.IsEmpty(Desbloqueos.Alcanzados(1));
        Assert.AreEqual(2, Desbloqueos.Proximo(1)!.Nivel);
        Assert.AreEqual(0, Desbloqueos.HackeosExtra(1));
        Assert.AreEqual(0, Desbloqueos.BarricadasExtra(1));
    }

    [TestMethod]
    public void LasVentajasSeAcumulanConElNivel()
    {
        Assert.AreEqual(1, Desbloqueos.HackeosExtra(2));
        Assert.AreEqual(1, Desbloqueos.BarricadasExtra(3));
        Assert.AreEqual(25, Desbloqueos.CreditosExtraPorOleada(4));
        Assert.AreEqual(2, Desbloqueos.BarricadasExtra(5));
        Assert.AreEqual(2, Desbloqueos.HackeosExtra(6));
        Assert.IsNull(Desbloqueos.Proximo(6));
    }

    [TestMethod]
    public void LosNivelesVanEnOrdenYSinRepetir()
    {
        var niveles = Desbloqueos.Todos.Select(d => d.Nivel).ToList();

        CollectionAssert.AreEqual(niveles.OrderBy(n => n).ToList(), niveles);
        Assert.HasCount(niveles.Count, niveles.Distinct().ToList());
    }

    [TestMethod]
    public void ElTutorialTieneTresPaginasConTexto()
    {
        Assert.HasCount(3, Tutorial.Paginas);
        Assert.IsTrue(Tutorial.Paginas.All(p => p.Parrafos.Count >= 3 && p.Parrafos.All(t => t.Length > 40)));
        Assert.Contains("dos filas", Tutorial.Paginas[2].Titulo);
    }
}
