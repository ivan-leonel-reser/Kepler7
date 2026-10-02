using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Juego;

[TestClass]
public class PartidaTests
{
    [TestMethod]
    public void Nueva_BorraLoAnteriorYEmpiezaDeCero()
    {
        var repositorio = new RepositorioProgresoFalso();
        repositorio.Guardar(ProgresoJugador.Restaurar(500, 300, 3, [Patron.Builder]));

        var partida = Partida.Nueva(repositorio, new AleatorioFijo());

        Assert.AreEqual(0, partida.Progreso.Xp);
        Assert.IsEmpty(partida.Progreso.Insignias);
        Assert.AreSame(partida.Progreso, repositorio.Guardado);
    }

    [TestMethod]
    public void Continuar_SinPartidaGuardadaFalla()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            Partida.Continuar(new RepositorioProgresoFalso(), new AleatorioFijo()));
    }

    [TestMethod]
    public void OtorgarInsignia_DaRecompensaUnaSolaVezYGuarda()
    {
        var repositorio = new RepositorioProgresoFalso();
        var partida = Partida.Nueva(repositorio, new AleatorioFijo());
        var guardadosAntes = repositorio.VecesGuardado;

        var primera = partida.OtorgarInsignia(Patron.Facade);
        var segunda = partida.OtorgarInsignia(Patron.Facade);

        Assert.IsNotNull(primera);
        Assert.IsNull(segunda);
        Assert.AreEqual(Partida.XpPorInsignia, partida.Progreso.Xp);
        Assert.AreEqual(Partida.CreditosPorInsignia, partida.Progreso.Creditos);
        Assert.AreEqual(guardadosAntes + 1, repositorio.VecesGuardado);
    }

    [TestMethod]
    public void RevisarLogros_DaLaInsigniaDeFacadeRecienConLasTresOperaciones()
    {
        var partida = Partida.Nueva(new RepositorioProgresoFalso(), new AleatorioFijo());

        partida.ControlCentral.IniciarTurno();
        partida.ControlCentral.EntrarEnEmergencia();
        Assert.IsEmpty(partida.RevisarLogros());

        partida.ControlCentral.ApagarEstacion();
        var nuevas = partida.RevisarLogros();

        Assert.HasCount(1, nuevas);
        Assert.AreEqual(Patron.Facade, nuevas[0].Patron);
        Assert.IsEmpty(partida.RevisarLogros());
    }
}
