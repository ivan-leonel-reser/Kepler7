using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Juego;

namespace Kepler7.Tests.Juego;

[TestClass]
public class NombresVisiblesTests
{
    [TestMethod]
    public void LosPatronesDeDosPalabrasSeMuestranSeparados()
    {
        Assert.AreEqual("Chain of Responsibility", NombresVisibles.De(Patron.ChainOfResponsibility));
        Assert.AreEqual("Template Method", NombresVisibles.De(Patron.TemplateMethod));
        Assert.AreEqual("Builder", NombresVisibles.De(Patron.Builder));
    }

    [TestMethod]
    public void ElMedicoSeMuestraConTilde()
    {
        Assert.AreEqual("Médico", NombresVisibles.De(ClaseUnidad.Medico));
        Assert.AreEqual("Soldado", NombresVisibles.De(ClaseUnidad.Soldado));
    }

    [TestMethod]
    public void LaBitacoraNombraElPatronDeLaInsigniaComoSeLee()
    {
        var partida = Partida.Nueva(new Dobles.RepositorioProgresoFalso(), new Dobles.AleatorioFijo());

        partida.OtorgarInsignia(Patron.ChainOfResponsibility);

        Assert.IsTrue(partida.Bitacora.Entradas.Any(e => e.Mensaje.Contains("(Chain of Responsibility)")));
    }
}
