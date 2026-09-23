using Kepler7.Core.Combate;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Estructurales.Facade;
using Kepler7.Core.Progreso;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Juego;

[TestClass]
public class PartidaOleadasTests
{
    private RepositorioProgresoFalso _repositorio = null!;
    private Partida _partida = null!;

    [TestInitialize]
    public void Preparar()
    {
        _repositorio = new RepositorioProgresoFalso();
        _partida = Partida.Nueva(_repositorio, new AleatorioFijo());
    }

    private static void GanarRapido(Oleada oleada)
    {
        foreach (var enemigo in oleada.Campo.UnidadesVivasDe(Bando.Enjambre).ToList())
            enemigo.PerderVida(enemigo.Vida - 1);
        foreach (var unidad in oleada.Escuadron.ToList())
        {
            var objetivos = oleada.ObjetivosDeAtaque(unidad);
            if (objetivos.Count > 0)
                oleada.Atacar(unidad, objetivos[0]);
        }
    }

    [TestMethod]
    public void ConLaEstacionApagadaNoSePuedeDefender()
    {
        Assert.IsFalse(_partida.PuedeDefender);
        Assert.ThrowsExactly<InvalidOperationException>(() => _partida.EmpezarOleada());
    }

    [TestMethod]
    public void DefenderPoneLaEstacionEnEmergencia()
    {
        _partida.ControlCentral.IniciarTurno();

        var oleada = _partida.EmpezarOleada();

        Assert.AreEqual(ModoEstacion.Emergencia, _partida.ControlCentral.Modo);
        Assert.AreEqual(1, oleada.Numero);
        Assert.IsTrue(oleada.Bitacora.Entradas.Any(e => e.Origen == "Seguridad"));
    }

    [TestMethod]
    public void GanarDaRecompensaYVuelveLaNormalidad()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        GanarRapido(oleada);

        var resultado = _partida.CerrarOleada();

        Assert.IsTrue(resultado.Victoria);
        Assert.AreEqual(1, _partida.Progreso.OleadasSuperadas);
        Assert.AreEqual(resultado.Creditos, _partida.Progreso.Creditos);
        Assert.AreEqual(ModoEstacion.Operativa, _partida.ControlCentral.Modo);
        Assert.AreEqual(2, _partida.NumeroProximaOleada);
        Assert.AreSame(_partida.Progreso, _repositorio.Guardado);
    }

    [TestMethod]
    public void PerderApagaLaEstacionYTerminaLaPartida()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        foreach (var unidad in _partida.Escuadron)
            unidad.PerderVida(unidad.Vida - 1);

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();
        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();
        var resultado = _partida.CerrarOleada();

        Assert.IsFalse(resultado.Victoria);
        Assert.AreEqual(ModoEstacion.Apagada, _partida.ControlCentral.Modo);
        Assert.IsTrue(_partida.Terminada);
        Assert.IsNull(_repositorio.Guardado);
        Assert.IsFalse(_partida.PuedeDefender);
    }

    [TestMethod]
    public void CadaOleadaEmpiezaConElEscuadronCurado()
    {
        _partida.ControlCentral.IniciarTurno();
        _partida.Escuadron[0].PerderVida(20);

        _partida.EmpezarOleada();

        Assert.IsTrue(_partida.Escuadron.All(u => u.Vida == u.VidaMaxima));
    }

    [TestMethod]
    public void DeshacerYRehacerEnUnaOleadaDaLaInsigniaDeCommand()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        var comandante = _partida.Escuadron[0];

        oleada.Atacar(comandante, oleada.ObjetivosDeAtaque(comandante)[0]);
        oleada.Deshacer();
        oleada.Rehacer();

        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.Command));
    }
}
