using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Juego;

[TestClass]
public class PartidaGuardadoTests
{
    [TestMethod]
    public void SiLaBaseFallaAlGuardarLaPartidaSigueYLoAvisa()
    {
        var repositorio = new RepositorioQueFalla();
        var partida = new Partida(repositorio, new ProgresoJugador(), new AleatorioFijo(), new RegistroAccesosFalso());

        partida.Guardar();
        partida.ControlCentral.IniciarTurno();
        partida.RevisarLogros();

        Assert.IsGreaterThanOrEqualTo(1, repositorio.IntentosDeGuardar);
        Assert.IsTrue(partida.Bitacora.Entradas.Any(e => e.Origen == "Guardado" && e.Tipo == TipoEntrada.Peligro));
        Assert.IsFalse(partida.Terminada);
    }

    [TestMethod]
    public void UnaPartidaTerminadaNoIntentaGuardar()
    {
        var repositorio = new RepositorioProgresoFalso();
        var partida = Partida.Nueva(repositorio, new AleatorioFijo(), new RegistroAccesosFalso());
        var guardadas = repositorio.VecesGuardado;
        foreach (var capa in partida.Armeria.Capas.ToList())
            partida.Armeria.Quitar(capa);
        partida.ControlCentral.IniciarTurno();
        var oleada = partida.EmpezarOleada();
        foreach (var unidad in partida.Escuadron)
            unidad.PerderVida(unidad.Vida - 1);
        while (!oleada.Terminada)
        {
            oleada.TerminarTurno();
            oleada.JugarTurnoEnjambre();
        }
        partida.CerrarOleada();
        var trasDerrota = repositorio.VecesGuardado;

        partida.Guardar();

        Assert.IsTrue(partida.Terminada);
        Assert.IsGreaterThanOrEqualTo(guardadas, trasDerrota);
        Assert.AreEqual(trasDerrota, repositorio.VecesGuardado);
        Assert.IsNull(repositorio.Guardado);
    }

    [TestMethod]
    public void SiLaBaseFallaAlBorrarLaPartidaNuevaArrancaIgual()
    {
        var partida = Partida.Nueva(new RepositorioQueFalla(), new AleatorioFijo(), new RegistroAccesosFalso(), new HistorialQueFalla());

        Assert.IsFalse(partida.Terminada);
        Assert.IsTrue(partida.Bitacora.Entradas.Any(e => e.Origen == "Guardado" && e.Mensaje.Contains("borrar")));
    }

    [TestMethod]
    public void SiLaBaseFallaAlPerderLaPartidaTerminaYSeAnotaEnElHistorial()
    {
        var historial = new Kepler7.DAL.Memoria.HistorialPartidasEnMemoria();
        var partida = new Partida(new RepositorioQueFalla(), new ProgresoJugador(), new AleatorioFijo(), new RegistroAccesosFalso(), historial);
        foreach (var capa in partida.Armeria.Capas.ToList())
            partida.Armeria.Quitar(capa);
        partida.ControlCentral.IniciarTurno();
        var oleada = partida.EmpezarOleada();
        foreach (var unidad in partida.Escuadron)
            unidad.PerderVida(unidad.Vida - 1);
        while (!oleada.Terminada)
        {
            oleada.TerminarTurno();
            oleada.JugarTurnoEnjambre();
        }

        var resultado = partida.CerrarOleada();

        Assert.IsFalse(resultado.Victoria);
        Assert.IsTrue(partida.Terminada);
        Assert.HasCount(1, historial.Ultimas(10));
    }
}
