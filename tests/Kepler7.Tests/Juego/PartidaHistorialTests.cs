using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Juego;
using Kepler7.DAL.Memoria;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Juego;

[TestClass]
public class PartidaHistorialTests
{
    private HistorialPartidasEnMemoria _historial = null!;
    private Partida _partida = null!;

    [TestInitialize]
    public void Preparar()
    {
        _historial = new HistorialPartidasEnMemoria();
        _partida = Partida.Nueva(new RepositorioProgresoFalso(), new AleatorioFijo(), historial: _historial);
        _partida.ControlCentral.IniciarTurno();
    }

    [TestMethod]
    public void GanarUnaOleadaQueNoEsLaUltimaNoAnotaNada()
    {
        GanarRapido(_partida.EmpezarOleada());
        _partida.CerrarOleada();

        Assert.IsEmpty(_historial.Ultimas(10));
    }

    [TestMethod]
    public void SalvarLaEstacionAnotaUnaPartidaGanada()
    {
        for (var i = 0; i < ProgresoJugador.OleadasTotales; i++)
        {
            GanarRapido(_partida.EmpezarOleada());
            _partida.CerrarOleada();
        }

        var anotada = _historial.Ultimas(10).Single();
        Assert.IsTrue(anotada.EstacionSalvada);
        Assert.AreEqual(6, anotada.OleadasSuperadas);
        Assert.AreEqual(_partida.Progreso.Xp, anotada.Xp);
    }

    [TestMethod]
    public void PerderAnotaLaPartidaComoNoSalvada()
    {
        Perder();

        var anotada = _historial.Ultimas(10).Single();
        Assert.IsFalse(anotada.EstacionSalvada);
        Assert.AreEqual(0, anotada.OleadasSuperadas);
    }

    [TestMethod]
    public void SiElHistorialFallaLaPartidaTerminaIgualYAvisa()
    {
        _partida = Partida.Nueva(new RepositorioProgresoFalso(), new AleatorioFijo(), historial: new HistorialQueFalla());
        _partida.ControlCentral.IniciarTurno();

        Perder();

        Assert.IsTrue(_partida.Terminada);
        Assert.IsTrue(_partida.Bitacora.Entradas.Any(e => e.Origen == "Historial" && e.Tipo == TipoEntrada.Peligro));
    }

    private void Perder()
    {
        // Sin capas de defensa, para que el escuadrón caiga en dos turnos.
        foreach (var capa in _partida.Armeria.Capas.ToList())
            _partida.Armeria.Quitar(capa);
        var oleada = _partida.EmpezarOleada();
        foreach (var unidad in _partida.Escuadron)
            unidad.PerderVida(unidad.Vida - 1);

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();
        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();
        _partida.CerrarOleada();
    }

    // En las últimas oleadas hay más enemigos que atacantes: se deja uno solo con 1 de vida y se lo remata.
    private static void GanarRapido(Oleada oleada)
    {
        var enemigos = oleada.Campo.UnidadesVivasDe(Bando.Enjambre).ToList();
        foreach (var enemigo in enemigos.Skip(1))
            enemigo.PerderVida(enemigo.Vida);
        enemigos[0].PerderVida(enemigos[0].Vida - 1);

        var atacante = oleada.Escuadron.First(u => oleada.ObjetivosDeAtaque(u).Count > 0);
        oleada.Atacar(atacante, enemigos[0]);
    }
}
