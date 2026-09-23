using Kepler7.Core.Combate;
using Kepler7.Core.Progreso;

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
}
