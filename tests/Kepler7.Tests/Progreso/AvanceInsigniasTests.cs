using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Comportamiento.Strategy;
using Kepler7.Core.Patrones.Creacionales.Builder;
using Kepler7.Core.Progreso;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Progreso;

[TestClass]
public class AvanceInsigniasTests
{
    private Partida _partida = null!;

    [TestInitialize]
    public void Preparar() => _partida = Partida.Nueva(new RepositorioProgresoFalso(), new AleatorioFijo());

    [TestMethod]
    public void EnUnaPartidaNuevaNingunaInsigniaEstaCompleta()
    {
        foreach (var insignia in CatalogoInsignias.Todas)
        {
            var avance = _partida.AvanceDe(insignia.Patron);
            Assert.IsFalse(avance.Completo, insignia.Nombre);
            Assert.IsGreaterThan(0, avance.Requerido);
            Assert.IsFalse(string.IsNullOrWhiteSpace(avance.Detalle));
        }
    }

    [TestMethod]
    public void CadaRecetaDistintaSumaUnPasoDelBuilder()
    {
        _partida.EnsamblarNave(new RecetaCombate());
        _partida.EnsamblarNave(new RecetaCombate());

        var avance = _partida.AvanceDe(Patron.Builder);

        Assert.AreEqual(1, avance.Actual);
        Assert.AreEqual(3, avance.Requerido);
    }

    [TestMethod]
    public void ElControlCentralCuentaCadaOperacionUnaSolaVez()
    {
        _partida.ControlCentral.IniciarTurno();
        _partida.ControlCentral.IniciarTurno();
        Assert.AreEqual(1, _partida.AvanceDe(Patron.Facade).Actual);

        _partida.ControlCentral.EntrarEnEmergencia();
        Assert.AreEqual(2, _partida.AvanceDe(Patron.Facade).Actual);
    }

    [TestMethod]
    public void AlCompletarElAvanceSeDaLaInsignia()
    {
        _partida.EnsamblarNave(new RecetaExploradora());
        _partida.EnsamblarNave(new RecetaCombate());
        _partida.EnsamblarNave(new RecetaCarga());

        Assert.IsTrue(_partida.AvanceDe(Patron.Builder).Completo);
        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.Builder));
        Assert.IsEmpty(_partida.RevisarLogros());
    }

    [TestMethod]
    public void UnaInsigniaDeOleadaSigueCompletaEnLaOleadaSiguiente()
    {
        _partida.ControlCentral.IniciarTurno();
        var primera = _partida.EmpezarOleada();
        primera.Hackear(primera.Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>().First(), new IADefensiva());
        _partida.RevisarLogros();
        GanarRapido(primera);
        _partida.CerrarOleada();

        _partida.EmpezarOleada();

        Assert.IsTrue(_partida.AvanceDe(Patron.Strategy).Completo);
        Assert.AreEqual(0, _partida.AvanceDe(Patron.Command).Actual);
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
