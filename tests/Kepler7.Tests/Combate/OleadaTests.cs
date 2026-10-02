using Kepler7.Core.Combate;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Combate;

[TestClass]
public class OleadaTests
{
    private UnidadEscuadron _soldado = null!;
    private UnidadEscuadron _medico = null!;
    private UnidadEscuadron _francotirador = null!;

    [TestInitialize]
    public void Preparar()
    {
        _soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);
        _medico = UnidadEscuadron.Crear("Paz", ClaseUnidad.Medico);
        _francotirador = UnidadEscuadron.Crear("Luna", ClaseUnidad.Francotirador);
    }

    private Oleada CrearOleada(params Enemigo[] enemigos) =>
        new(1, [_soldado, _medico, _francotirador], enemigos, new AleatorioFijo());

    [TestMethod]
    public void CadaUnidadVaASuFilaSegunComoAtaca()
    {
        var oleada = CrearOleada(new Enemigo("Zangano", 12, 3), new Enemigo("Escupidor", 9, 3, atacaADistancia: true));

        Assert.AreEqual(Fila.Vanguardia, _soldado.Fila);
        Assert.AreEqual(Fila.Retaguardia, _francotirador.Fila);
        Assert.HasCount(1, oleada.Campo.EnFila(Bando.Enjambre, Fila.Retaguardia).ToList());
    }

    [TestMethod]
    public void CadaUnidadActuaUnaVezYDeshacerLaLibera()
    {
        var zangano = new Enemigo("Zangano", 30, 3);
        var oleada = CrearOleada(zangano);

        oleada.Atacar(_soldado, zangano);
        Assert.IsFalse(oleada.PuedeActuar(_soldado));
        Assert.ThrowsExactly<InvalidOperationException>(() => oleada.Atacar(_soldado, zangano));

        oleada.Deshacer();
        Assert.IsTrue(oleada.PuedeActuar(_soldado));
        Assert.AreEqual(30, zangano.Vida);
    }

    [TestMethod]
    public void SoloElMedicoCura()
    {
        var oleada = CrearOleada(new Enemigo("Zangano", 30, 3));
        _soldado.PerderVida(15);

        Assert.IsEmpty(oleada.ObjetivosDeCuracion(_soldado));
        Assert.Contains(_soldado, oleada.ObjetivosDeCuracion(_medico));

        oleada.Curar(_medico, _soldado);
        Assert.AreEqual(_soldado.VidaMaxima - 15 + Oleada.Curacion, _soldado.Vida);
    }

    [TestMethod]
    public void SeConstruyenComoMuchoDosBarricadas()
    {
        var oleada = CrearOleada(new Enemigo("Zangano", 30, 1));
        oleada.Mover(_soldado);
        oleada.Construir(_medico);
        oleada.Construir(_francotirador);
        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        // Adelante queda un lugar libre, pero ya no quedan barricadas.
        Assert.IsTrue(oleada.Campo.HayLugar(Bando.Escuadron, Fila.Vanguardia));
        Assert.AreEqual(0, oleada.BarricadasDisponibles);
        Assert.IsFalse(oleada.PuedeConstruir(_soldado));
    }

    [TestMethod]
    public void DerribarAlUltimoEnemigoGanaLaOleada()
    {
        var zangano = new Enemigo("Zangano", 8, 3);
        var oleada = CrearOleada(zangano);

        oleada.Atacar(_soldado, zangano);

        Assert.AreEqual(EstadoOleada.Victoria, oleada.Estado);
        Assert.IsNull(oleada.Deshacer());
    }

    [TestMethod]
    public void ElEnjambreLePegaAlMasDebilDeAdelante()
    {
        var zangano = new Enemigo("Zangano", 30, 5);
        var oleada = CrearOleada(zangano);
        _francotirador.PerderVida(15);

        oleada.TerminarTurno();
        var jugada = oleada.JugarSiguienteEnemigo();

        // El francotirador tiene menos vida, pero está atrás y el soldado lo cubre.
        Assert.IsNotNull(jugada);
        Assert.AreSame(_soldado, jugada.Objetivo);
        Assert.AreEqual(_soldado.VidaMaxima - 5, _soldado.Vida);
        Assert.IsNull(oleada.JugarSiguienteEnemigo());
        Assert.AreEqual(EstadoOleada.TurnoJugador, oleada.Estado);
        Assert.AreEqual(2, oleada.Turno);
    }

    [TestMethod]
    public void LoConfirmadoEnUnTurnoYaNoSeDeshace()
    {
        var zangano = new Enemigo("Zangano", 30, 1);
        var oleada = CrearOleada(zangano);
        oleada.Atacar(_soldado, zangano);

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.IsNull(oleada.Deshacer());
        Assert.HasCount(1, oleada.Comandos.Historial);
    }

    [TestMethod]
    public void SiCaeTodoElEscuadronSePierde()
    {
        var reina = new Enemigo("Reina", 100, 100);
        var oleada = new Oleada(1, [_soldado], [reina], new AleatorioFijo());

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.AreEqual(EstadoOleada.Derrota, oleada.Estado);
        Assert.IsTrue(oleada.Terminada);
    }
}
