using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Comportamiento.State;
using Kepler7.Core.Patrones.Comportamiento.Strategy;
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
    public void CadaAccionDejaLasCuatroEtapasEnLaBitacora()
    {
        var zangano = new Enemigo("Zangano", 30, 3);
        var oleada = CrearOleada(zangano);

        oleada.Atacar(_francotirador, zangano);

        var etapas = oleada.Bitacora.Entradas.Where(e => e.Origen.StartsWith("Luna")).Select(e => e.Origen).ToList();
        CollectionAssert.AreEqual(new[] { "Luna · inicio", "Luna · decisión", "Luna · acción", "Luna · fin" }, etapas);
        Assert.Contains(ClaseUnidad.Francotirador, oleada.ClasesQueJugaron);

        // El tiro apuntado va adentro del comando, así que deshacer devuelve toda la vida.
        oleada.Deshacer();
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
    public void CadaEnemigoJuegaLoQueDecideSuIA()
    {
        var agresivo = new Enemigo("Zangano", 30, 5, estrategia: new IAAgresiva());
        var herido = new Enemigo("Coloso", 30, 5, estrategia: new IADefensiva());
        var oleada = CrearOleada(agresivo, herido);
        herido.PerderVida(20);

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.AreEqual(_soldado.VidaMaxima - 5, _soldado.Vida);
        Assert.AreEqual(Fila.Retaguardia, herido.Fila);
        Assert.HasCount(2, oleada.UltimasJugadas);
        Assert.AreEqual("Agresiva", oleada.UltimasJugadas[0].IA);
        Assert.AreEqual(AccionIA.Retroceder, oleada.UltimasJugadas[1].Decision.Accion);
    }

    [TestMethod]
    public void SiLaIAPideAlgoQueNoSePuedeElEnemigoEspera()
    {
        var tramposo = new Enemigo("Zangano", 30, 5, estrategia: new IAQueAtacaAlDeAtras());
        var oleada = CrearOleada(tramposo);

        oleada.TerminarTurno();
        var jugada = oleada.JugarSiguienteEnemigo();

        Assert.IsNotNull(jugada);
        Assert.AreEqual(AccionIA.Esperar, jugada.Decision.Accion);
        Assert.AreEqual(_francotirador.VidaMaxima, _francotirador.Vida);
    }

    [TestMethod]
    public void HackearCambiaLoQueHaceElEnemigo()
    {
        var zangano = new Enemigo("Zangano", 30, 5, estrategia: new IAAgresiva());
        var oleada = CrearOleada(zangano);
        zangano.PerderVida(20);

        oleada.Hackear(zangano, new IADefensiva());
        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        // Con la IA agresiva le pegaba al soldado; con la defensiva, herido, se repliega.
        Assert.AreEqual(_soldado.VidaMaxima, _soldado.Vida);
        Assert.AreEqual(Fila.Retaguardia, zangano.Fila);
        Assert.IsTrue(zangano.IARevelada);
        Assert.IsTrue(oleada.Bitacora.Entradas.Any(e => e.Origen == "Hackeo"));
    }

    [TestMethod]
    public void SeHackeaDosVecesPorOleadaYSoloEnElTurnoPropio()
    {
        var zangano = new Enemigo("Zangano", 30, 1);
        var oleada = CrearOleada(zangano);

        oleada.Hackear(zangano, new IAAleatoria());
        oleada.TerminarTurno();
        Assert.IsFalse(oleada.PuedeHackear(zangano));

        oleada.JugarTurnoEnjambre();
        oleada.Hackear(zangano, new IACalculadora());

        Assert.AreEqual(0, oleada.HackeosDisponibles);
        Assert.ThrowsExactly<InvalidOperationException>(() => oleada.Hackear(zangano, new IAAgresiva()));
        Assert.IsInstanceOfType<IACalculadora>(zangano.Estrategia);
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

    [TestMethod]
    public void ElRefuerzoDeLaTierraSumaBarricadasSoloAntesDeEmpezar()
    {
        var zangano = new Enemigo("Zangano", 30, 3);
        var oleada = CrearOleada(zangano);

        oleada.RecibirRefuerzo(2);
        Assert.AreEqual(Oleada.BarricadasPorOleada + 2, oleada.BarricadasDisponibles);

        oleada.Construir(_soldado);
        Assert.AreEqual(Oleada.BarricadasPorOleada + 1, oleada.BarricadasDisponibles);
        Assert.ThrowsExactly<InvalidOperationException>(() => oleada.RecibirRefuerzo(1));
    }

    [TestMethod]
    public void UnaUnidadAturdidaPierdeSuTurnoYDespuesVuelveAJugar()
    {
        var coloso = new Enemigo("Coloso", 40, 5) { EfectoAlGolpear = EfectoGolpe.Aturdimiento };
        var oleada = CrearOleada(coloso);

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.IsInstanceOfType<Aturdido>(_soldado.Estado);
        Assert.IsFalse(oleada.PuedeActuar(_soldado));
        Assert.IsFalse(oleada.PuedeMover(_soldado));
        Assert.HasCount(0, oleada.ObjetivosDeAtaque(_soldado));
        Assert.ThrowsExactly<InvalidOperationException>(() => oleada.Atacar(_soldado, coloso));
        Assert.IsTrue(oleada.PuedeActuar(_francotirador));

        // Al terminar el turno que perdió se recupera, y el golpe siguiente no la vuelve a aturdir.
        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.IsInstanceOfType<Normal>(_soldado.Estado);
        Assert.IsTrue(oleada.PuedeActuar(_soldado));
        Assert.Contains(_soldado, oleada.SobrevivientesConEstado.ToList());
    }

    [TestMethod]
    public void ElVenenoLeSacaVidaAlEscuadronAlTerminarSuTurno()
    {
        var escupidor = new Enemigo("Escupidor", 9, 3, atacaADistancia: true) { EfectoAlGolpear = EfectoGolpe.Veneno };
        var oleada = CrearOleada(escupidor);

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();
        Assert.IsInstanceOfType<Envenenado>(_soldado.Estado);
        Assert.AreEqual(27, _soldado.Vida);

        oleada.TerminarTurno();

        Assert.AreEqual(27 - Envenenado.DanioPorTurno, _soldado.Vida);
        Assert.IsTrue(oleada.Bitacora.Entradas.Any(e => e.Origen == "Estado · Rojas" && e.Mensaje.Contains("Envenenado")));
        Assert.IsTrue(oleada.Bitacora.Entradas.Any(e => e.Origen == "Estado · Rojas" && e.Mensaje.Contains("veneno")));
    }

    [TestMethod]
    public void UnEnemigoAturdidoPierdeSuTurno()
    {
        var zangano = new Enemigo("Zangano", 30, 3);
        var oleada = CrearOleada(zangano);
        zangano.CambiarEstado(new Aturdido());

        oleada.TerminarTurno();
        var jugada = oleada.JugarSiguienteEnemigo();

        Assert.AreEqual(AccionIA.Esperar, jugada!.Decision.Accion);
        Assert.IsNull(jugada.Objetivo);
        Assert.AreEqual(30, _soldado.Vida);

        Assert.IsNull(oleada.JugarSiguienteEnemigo());
        Assert.IsInstanceOfType<Normal>(zangano.Estado);
    }

    [TestMethod]
    public void UnEnemigoMuyHeridoEntraEnBerserkYPegaMasFuerte()
    {
        var zangano = new Enemigo("Zangano", 12, 3);
        var oleada = CrearOleada(zangano);

        // El soldado carga: 8 + 2. El zángano queda en 2 de 12.
        oleada.Atacar(_soldado, zangano);
        Assert.IsInstanceOfType<Berserk>(zangano.Estado);

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.AreEqual(30 - (3 + Berserk.BonusAtaque), _soldado.Vida);
    }

    [TestMethod]
    public void DeshacerUnAtaqueDevuelveElEstadoDelObjetivo()
    {
        var zangano = new Enemigo("Zangano", 30, 3);
        var oleada = CrearOleada(zangano);
        _soldado.EfectoAlGolpear = EfectoGolpe.Veneno;

        oleada.Atacar(_soldado, zangano);
        Assert.IsInstanceOfType<Envenenado>(zangano.Estado);

        oleada.Deshacer();
        Assert.IsInstanceOfType<Normal>(zangano.Estado);
        Assert.AreEqual(30, zangano.Vida);

        oleada.Rehacer();
        Assert.IsInstanceOfType<Envenenado>(zangano.Estado);
    }

    [TestMethod]
    public void ElVenenoPuedeTerminarLaOleada()
    {
        var zangano = new Enemigo("Zangano", 30, 3);
        var oleada = CrearOleada(zangano);
        _soldado.EfectoAlGolpear = EfectoGolpe.Veneno;

        oleada.Atacar(_soldado, zangano);
        zangano.PerderVida(zangano.Vida - 1);
        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.IsFalse(zangano.EstaViva);
        Assert.AreEqual(EstadoOleada.Victoria, oleada.Estado);
    }

    // Quiere pegarle a alguien de la retaguardia aunque adelante todavía haya unidades.
    private class IAQueAtacaAlDeAtras : IEstrategiaIA
    {
        public string Nombre => "Tramposa";
        public string Descripcion => "";

        public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto) =>
            DecisionIA.Atacar(contexto.Campo.EnFila(Bando.Escuadron, Fila.Retaguardia).First(), "");
    }
}
