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

            Assert.IsTrue(enemigos.Count(e => !e.AtacaADistancia) <= CampoBatalla.LugaresPorFila, $"oleada {numero}");
            Assert.IsTrue(enemigos.Count(e => e.AtacaADistancia) <= CampoBatalla.LugaresPorFila, $"oleada {numero}");
        }
    }

    [TestMethod]
    public void LasOleadasSeVuelvenMasDuras()
    {
        var vidaPrimera = GeneradorOleadas.Crear(1).Sum(e => e.VidaMaxima);
        var vidaUltima = GeneradorOleadas.Crear(ProgresoJugador.OleadasTotales).Sum(e => e.VidaMaxima);

        Assert.IsTrue(vidaUltima > vidaPrimera * 3);
    }

    [TestMethod]
    public void LaUltimaOleadaTraeALaReina()
    {
        Assert.IsTrue(GeneradorOleadas.Crear(6).Any(e => e.Nombre == "Reina del Enjambre"));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => GeneradorOleadas.Crear(7));
    }
}
