using Kepler7.BE;
using Kepler7.DAL.Memoria;

namespace Kepler7.Tests.Persistencia;

[TestClass]
public class HistorialEnMemoriaTests
{
    [TestMethod]
    public void SinPartidasElHistorialEstaVacio()
    {
        Assert.IsEmpty(new HistorialPartidasEnMemoria().Ultimas(10));
    }

    [TestMethod]
    public void DevuelveLasMasNuevasPrimeroYRespetaLaCantidad()
    {
        var historial = new HistorialPartidasEnMemoria();
        historial.Registrar(new PartidaJugada(new DateTime(2026, 10, 20), false, 2, 2, 150, 3));
        historial.Registrar(new PartidaJugada(new DateTime(2026, 10, 22), true, 6, 7, 640, 9));
        historial.Registrar(new PartidaJugada(new DateTime(2026, 10, 21), false, 4, 4, 380, 5));

        var ultimas = historial.Ultimas(2);

        Assert.HasCount(2, ultimas);
        Assert.AreEqual(new DateTime(2026, 10, 22), ultimas[0].Fecha);
        Assert.AreEqual(new DateTime(2026, 10, 21), ultimas[1].Fecha);
    }
}
