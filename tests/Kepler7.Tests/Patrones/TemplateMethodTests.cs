using System.Reflection;
using Kepler7.Core.Combate;
using Kepler7.Core.Comun;
using Kepler7.Core.Patrones.Comportamiento.Command;
using Kepler7.Core.Patrones.Comportamiento.TemplateMethod;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class TemplateMethodTests
{
    private CampoBatalla _campo = null!;
    private GestorComandos _gestor = null!;
    private Bitacora _historial = null!;
    private UnidadEscuadron _soldado = null!;
    private UnidadEscuadron _comandante = null!;
    private UnidadEscuadron _medico = null!;
    private UnidadEscuadron _francotirador = null!;
    private Enemigo _coloso = null!;

    [TestInitialize]
    public void Preparar()
    {
        _campo = new CampoBatalla();
        _gestor = new GestorComandos();
        _historial = new Bitacora();
        _soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);
        _comandante = UnidadEscuadron.Crear("Vega", ClaseUnidad.Comandante);
        _medico = UnidadEscuadron.Crear("Paz", ClaseUnidad.Medico);
        _francotirador = UnidadEscuadron.Crear("Luna", ClaseUnidad.Francotirador);
        _coloso = new Enemigo("Coloso", 100, 3);
        _campo.Agregar(_soldado, Fila.Vanguardia);
        _campo.Agregar(_comandante, Fila.Vanguardia);
        _campo.Agregar(_medico, Fila.Retaguardia);
        _campo.Agregar(_francotirador, Fila.Retaguardia);
        _campo.Agregar(_coloso, Fila.Vanguardia);
    }

    private void Jugar(UnidadEscuadron unidad, TipoOrden tipo, Unidad? objetivo = null) =>
        TurnoUnidad.Para(unidad, _campo).EjecutarTurno(new OrdenTurno(tipo, objetivo), _gestor, _historial);

    private string Etapa(string unidad, string etapa) =>
        _historial.Entradas.Last(e => e.Origen == $"{unidad} · {etapa}").Mensaje;

    [TestMethod]
    public void EjecutarTurno_NoSePuedeRedefinir()
    {
        var plantilla = typeof(TurnoUnidad).GetMethod(nameof(TurnoUnidad.EjecutarTurno))!;
        var decidir = typeof(TurnoUnidad).GetMethod("Decidir", BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.IsFalse(plantilla.IsVirtual);
        Assert.IsTrue(decidir.IsAbstract);
    }

    [TestMethod]
    public void CadaClaseTieneSuTurno()
    {
        Assert.IsInstanceOfType<TurnoSoldado>(TurnoUnidad.Para(_soldado, _campo));
        Assert.IsInstanceOfType<TurnoMedico>(TurnoUnidad.Para(_medico, _campo));
        Assert.IsInstanceOfType<TurnoFrancotirador>(TurnoUnidad.Para(_francotirador, _campo));
        Assert.IsInstanceOfType<TurnoComandante>(TurnoUnidad.Para(_comandante, _campo));
        StringAssert.Contains(TurnoUnidad.Para(_francotirador, _campo).Habilidad, "apunta");
    }

    [TestMethod]
    public void LasEtapasSeRegistranSiempreEnElMismoOrden()
    {
        Jugar(_soldado, TipoOrden.Atacar, _coloso);
        Jugar(_medico, TipoOrden.Mover);

        string[] esperado =
        [
            "Rojas · inicio", "Rojas · decisión", "Rojas · acción", "Rojas · fin",
            "Paz · inicio", "Paz · decisión", "Paz · acción", "Paz · fin",
        ];
        CollectionAssert.AreEqual(esperado, _historial.Entradas.Select(e => e.Origen).ToArray());
    }

    [TestMethod]
    public void Soldado_AlAtacarCarga()
    {
        Jugar(_soldado, TipoOrden.Atacar, _coloso);

        Assert.AreEqual(100 - _soldado.Ataque - TurnoSoldado.BonusCarga, _coloso.Vida);
        StringAssert.Contains(Etapa("Rojas", "decisión"), "carga");
    }

    [TestMethod]
    public void Francotirador_SoloApuntaDesdeAtras()
    {
        Jugar(_francotirador, TipoOrden.Atacar, _coloso);
        Assert.AreEqual(100 - _francotirador.Ataque - TurnoFrancotirador.BonusApuntado, _coloso.Vida);

        _campo.Mover(_francotirador, Fila.Vanguardia);
        var antes = _coloso.Vida;
        Jugar(_francotirador, TipoOrden.Atacar, _coloso);

        Assert.AreEqual(antes - _francotirador.Ataque, _coloso.Vida);
        StringAssert.Contains(Etapa("Luna", "decisión"), "sin apuntar");
    }

    [TestMethod]
    public void Medico_CuraMasAlQueEstaGrave()
    {
        _soldado.PerderVida(25);
        _comandante.PerderVida(10);

        Jugar(_medico, TipoOrden.Curar, _soldado);
        Jugar(_medico, TipoOrden.Curar, _comandante);

        Assert.AreEqual(5 + Oleada.Curacion + TurnoMedico.BonusUrgencia, _soldado.Vida);
        Assert.AreEqual(_comandante.VidaMaxima, _comandante.Vida);
        StringAssert.Contains(_historial.Entradas.First(e => e.Origen == "Paz · inicio").Mensaje, "Rojas");
    }

    [TestMethod]
    public void Comandante_PegaMasConAliadosEnSuFila()
    {
        Jugar(_comandante, TipoOrden.Atacar, _coloso);
        Assert.AreEqual(100 - _comandante.Ataque - 1, _coloso.Vida);

        _campo.Mover(_soldado, Fila.Retaguardia);
        var antes = _coloso.Vida;
        Jugar(_comandante, TipoOrden.Atacar, _coloso);

        Assert.AreEqual(antes - _comandante.Ataque, _coloso.Vida);
        StringAssert.Contains(Etapa("Vega", "decisión"), "sin apoyo");
    }

    [TestMethod]
    public void LaAccionQuedaEnElGestorYSeDeshaceConElBonus()
    {
        Jugar(_francotirador, TipoOrden.Atacar, _coloso);

        Assert.HasCount(1, _gestor.AccionesDelTurno);
        _gestor.Deshacer();
        Assert.AreEqual(100, _coloso.Vida);
    }
}
