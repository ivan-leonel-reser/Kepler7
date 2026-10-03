using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Comportamiento.Strategy;

namespace Kepler7.Tests.Combate;

[TestClass]
public class GeneradorOleadasTests
{
    [TestMethod]
    public void CadaOleadaEntraEnElCampo()
    {
        for (var numero = 1; numero <= ProgresoJugador.OleadasTotales; numero++)
        {
            var enemigos = GeneradorOleadas.Crear(numero);

            Assert.IsLessThanOrEqualTo(CampoBatalla.LugaresPorFila, enemigos.Count(e => !e.AtacaADistancia), $"oleada {numero}");
            Assert.IsLessThanOrEqualTo(CampoBatalla.LugaresPorFila, enemigos.Count(e => e.AtacaADistancia), $"oleada {numero}");
        }
    }

    [TestMethod]
    public void LasOleadasSeVuelvenMasDuras()
    {
        var vidaPrimera = GeneradorOleadas.Crear(1).Sum(e => e.VidaMaxima);
        var vidaUltima = GeneradorOleadas.Crear(ProgresoJugador.OleadasTotales).Sum(e => e.VidaMaxima);

        Assert.IsGreaterThan(vidaPrimera * 3, vidaUltima);
    }

    [TestMethod]
    public void LaUltimaOleadaTraeALaReina()
    {
        Assert.IsTrue(GeneradorOleadas.Crear(6).Any(e => e.Nombre == "Reina del Enjambre"));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => GeneradorOleadas.Crear(7));
    }

    [TestMethod]
    public void CadaTipoDeEnemigoLlegaConSuIA()
    {
        var todos = Enumerable.Range(1, ProgresoJugador.OleadasTotales).SelectMany(GeneradorOleadas.Crear).ToList();

        Assert.IsTrue(todos.Where(e => e.Nombre.StartsWith("Zángano")).All(e => e.Estrategia is IAAgresiva));
        Assert.IsTrue(todos.Where(e => e.Nombre.StartsWith("Escupidor")).All(e => e.Estrategia is IAAleatoria));
        Assert.IsTrue(todos.Where(e => e.Nombre.StartsWith("Coloso")).All(e => e.Estrategia is IADefensiva));
        Assert.IsInstanceOfType<IACalculadora>(todos.Single(e => e.Nombre.StartsWith("Reina")).Estrategia);
    }
}
