using Kepler7.BE;
using Kepler7.Core.Comun;

namespace Kepler7.Tests.Comun;

[TestClass]
public class ComunTests
{
    [TestMethod]
    public void Aleatorio_ConLaMismaSemilla_RepiteLaSecuencia()
    {
        var a = new Aleatorio(42);
        var b = new Aleatorio(42);

        for (var i = 0; i < 10; i++)
            Assert.AreEqual(a.Siguiente(0, 100), b.Siguiente(0, 100));
    }

    [TestMethod]
    public void Aleatorio_ProbabilidadEnLosExtremos()
    {
        var aleatorio = new Aleatorio(7);

        for (var i = 0; i < 50; i++)
        {
            Assert.IsFalse(aleatorio.Probabilidad(0));
            Assert.IsTrue(aleatorio.Probabilidad(100));
        }
    }

    [TestMethod]
    public void Bitacora_RegistraEnOrden()
    {
        var bitacora = new Bitacora();

        bitacora.Registrar("Energia", "Reactor encendido");
        bitacora.Registrar("Defensa", "Escudos arriba", TipoEntrada.Exito);

        Assert.HasCount(2, bitacora.Entradas);
        Assert.AreEqual("[Energia] Reactor encendido", bitacora.Entradas[0].ToString());
        Assert.AreEqual(TipoEntrada.Exito, bitacora.Entradas[1].Tipo);
    }
}
