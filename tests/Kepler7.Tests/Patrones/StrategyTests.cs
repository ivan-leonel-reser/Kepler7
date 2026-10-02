using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Comportamiento.Strategy;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class StrategyTests
{
    private CampoBatalla _campo = null!;
    private UnidadEscuadron _soldado = null!;
    private UnidadEscuadron _comandante = null!;
    private UnidadEscuadron _francotirador = null!;
    private Enemigo _zangano = null!;

    [TestInitialize]
    public void Preparar()
    {
        _campo = new CampoBatalla();
        _soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);
        _comandante = UnidadEscuadron.Crear("Vega", ClaseUnidad.Comandante);
        _francotirador = UnidadEscuadron.Crear("Luna", ClaseUnidad.Francotirador);
        _zangano = new Enemigo("Zangano", 20, 5);
        _campo.Agregar(_soldado, Fila.Vanguardia);
        _campo.Agregar(_comandante, Fila.Vanguardia);
        _campo.Agregar(_francotirador, Fila.Retaguardia);
        _campo.Agregar(_zangano, Fila.Vanguardia);
    }

    private DecisionIA Decidir(IEstrategiaIA estrategia, params int[] azar) =>
        estrategia.DecidirAccion(_zangano, new Contexto(_campo, 1, new AleatorioFijo(azar)));

    [TestMethod]
    public void Agresiva_AtacaAlQueMasPega()
    {
        var decision = Decidir(new IAAgresiva());

        // El soldado pega 8 y el comandante 7. El francotirador pega más, pero está cubierto atrás.
        Assert.AreEqual(AccionIA.Atacar, decision.Accion);
        Assert.AreSame(_soldado, decision.Objetivo);
    }

    [TestMethod]
    public void Agresiva_DesdeAtrasAvanza()
    {
        _campo.Mover(_zangano, Fila.Retaguardia);

        Assert.AreEqual(AccionIA.Avanzar, Decidir(new IAAgresiva()).Accion);
    }

    [TestMethod]
    public void Defensiva_EnteraAtacaAlMasDebil()
    {
        _comandante.PerderVida(20);

        var decision = Decidir(new IADefensiva());

        Assert.AreEqual(AccionIA.Atacar, decision.Accion);
        Assert.AreSame(_comandante, decision.Objetivo);
    }

    [TestMethod]
    public void Defensiva_HeridaSeRepliegaYNoVuelve()
    {
        _zangano.PerderVida(11);

        Assert.AreEqual(AccionIA.Retroceder, Decidir(new IADefensiva()).Accion);

        _campo.Mover(_zangano, Fila.Retaguardia);
        Assert.AreEqual(AccionIA.Esperar, Decidir(new IADefensiva()).Accion);
    }

    [TestMethod]
    public void Aleatoria_EligeLaOpcionQueSaleEnElSorteo()
    {
        // Opciones del zángano: atacar a Rojas, atacar a Vega, pasar atrás o esperar.
        var segunda = Decidir(new IAAleatoria(), 1);
        var ultima = Decidir(new IAAleatoria(), 3);

        Assert.AreSame(_comandante, segunda.Objetivo);
        Assert.AreEqual(AccionIA.Esperar, ultima.Accion);
        StringAssert.Contains(ultima.Motivo, "4 de 4");
    }

    [TestMethod]
    public void Calculadora_RemataAlQueCaeDeUnGolpe()
    {
        _comandante.PerderVida(31);

        var decision = Decidir(new IACalculadora());

        Assert.AreSame(_comandante, decision.Objetivo);
        StringAssert.Contains(decision.Motivo, "de un golpe");
    }

    [TestMethod]
    public void Calculadora_SinRemateVaPorElQueCaeAntes()
    {
        _comandante.PerderVida(20);

        // A Vega le quedan 15 (3 golpes de 5); a Rojas 30 (6 golpes).
        var decision = Decidir(new IACalculadora());

        Assert.AreSame(_comandante, decision.Objetivo);
        StringAssert.Contains(decision.Motivo, "3 golpes");
    }

    [TestMethod]
    public void LaMismaSituacionSeResuelveDistintoSegunLaIA()
    {
        _zangano.PerderVida(11);
        _comandante.PerderVida(31);

        Assert.AreSame(_soldado, Decidir(new IAAgresiva()).Objetivo);
        Assert.AreEqual(AccionIA.Retroceder, Decidir(new IADefensiva()).Accion);
        Assert.AreSame(_comandante, Decidir(new IACalculadora()).Objetivo);
    }

    [TestMethod]
    public void DecidirNoCambiaNadaEnElCampo()
    {
        IEstrategiaIA[] estrategias = [new IAAgresiva(), new IADefensiva(), new IAAleatoria(), new IACalculadora()];
        _zangano.PerderVida(11);

        foreach (var estrategia in estrategias)
            Decidir(estrategia);

        Assert.AreEqual(Fila.Vanguardia, _zangano.Fila);
        Assert.AreEqual(_soldado.VidaMaxima, _soldado.Vida);
        Assert.AreEqual(_comandante.VidaMaxima, _comandante.Vida);
    }
}
