using Kepler7.Core.Combate;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Comportamiento.Strategy;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Juego;

[TestClass]
public class RegistroOleadasTests
{
    private RepositorioProgresoFalso _repositorio = null!;
    private Partida _partida = null!;

    [TestInitialize]
    public void Preparar()
    {
        _repositorio = new RepositorioProgresoFalso();
        _partida = Partida.Nueva(_repositorio, new AleatorioFijo());
        _partida.ControlCentral.IniciarTurno();
    }

    [TestMethod]
    public void AlEmpezarNoHayOleadasEnElResumen()
    {
        Assert.IsEmpty(_partida.Resumen().Oleadas);
    }

    [TestMethod]
    public void UnaOleadaGanadaQuedaConSuRecompensaYSusNumeros()
    {
        var oleada = _partida.EmpezarOleada();
        var enemigos = oleada.Campo.UnidadesVivasDe(Bando.Enjambre).Count();
        oleada.Hackear(oleada.Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>().First(), new IADefensiva());
        GanarRapido(oleada);

        var resultado = _partida.CerrarOleada();

        var registro = _partida.Resumen().Oleadas.Single();
        Assert.AreEqual(1, registro.Numero);
        Assert.IsTrue(registro.Victoria);
        Assert.AreEqual(enemigos, registro.Enemigos);
        Assert.AreEqual(enemigos, registro.EnemigosDerrotados);
        Assert.AreEqual(0, registro.Bajas);
        Assert.AreEqual(1, registro.Hackeos);
        Assert.AreEqual(resultado.Xp, registro.Xp);
        Assert.AreEqual(resultado.Creditos, registro.Creditos);
    }

    [TestMethod]
    public void LasOleadasQuedanEnOrden()
    {
        for (var i = 0; i < 2; i++)
        {
            GanarRapido(_partida.EmpezarOleada());
            _partida.CerrarOleada();
        }

        CollectionAssert.AreEqual(new[] { 1, 2 }, _partida.Resumen().Oleadas.Select(o => o.Numero).ToList());
    }

    [TestMethod]
    public void UnaDerrotaQuedaRegistradaConLasBajasYSinRecompensa()
    {
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

        var registro = _partida.Resumen().Oleadas.Single();
        Assert.IsFalse(registro.Victoria);
        Assert.AreEqual(_partida.Escuadron.Count, registro.Bajas);
        Assert.AreEqual(0, registro.Xp);
        Assert.AreEqual(0, registro.Creditos);
        Assert.IsGreaterThan(1, registro.Turnos);
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
}
