using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class ChainTests
{
    private UnidadEscuadron _soldado = null!;
    private AleatorioFijo _azar = null!;
    private CadenaDefensas _cadena = null!;
    private Esquiva _esquiva = null!;
    private Bloqueo _bloqueo = null!;
    private Armadura _armadura = null!;
    private Escudo _escudo = null!;
    private Vida _vida = null!;

    [TestInitialize]
    public void Preparar()
    {
        _soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);
        _azar = new AleatorioFijo();
        _esquiva = new Esquiva(_azar);
        _bloqueo = new Bloqueo();
        _armadura = new Armadura();
        _escudo = new Escudo();
        _vida = new Vida(_soldado);

        // El orden de la consigna: Esquiva, Bloqueo, Armadura, Escudo y Vida.
        _cadena = new CadenaDefensas();
        foreach (IManejadorDanio eslabon in new IManejadorDanio[] { _esquiva, _bloqueo, _armadura, _escudo, _vida })
            _cadena.Agregar(eslabon);
    }

    private Danio Golpear(int cantidad)
    {
        var danio = new Danio(cantidad);
        _cadena.Manejar(danio);
        return danio;
    }

    private static string[] Capas(Danio danio) => danio.Recorrido.Select(p => p.Capa).ToArray();

    [TestMethod]
    public void ElGolpeRecorreLasCapasEnOrdenHastaQueUnaLoFrena()
    {
        var danio = Golpear(5);

        // 5 es mucho para el bloqueo, la armadura frena 1 y el escudo absorbe los 4 que quedan.
        CollectionAssert.AreEqual(new[] { "Esquiva", "Bloqueo", "Armadura", "Escudo" }, Capas(danio));
        Assert.AreEqual(0, danio.VidaPerdida);
        Assert.AreEqual(_soldado.VidaMaxima, _soldado.Vida);
        Assert.AreEqual(Escudo.PuntosPorOleada - 4, _escudo.Puntos);
    }

    [TestMethod]
    public void SiNingunaCapaAlcanzaElRestoLlegaALaVida()
    {
        var danio = Golpear(10);

        Assert.AreEqual("Vida", danio.Recorrido[^1].Capa);
        Assert.AreEqual(10 - Armadura.Reduccion - Escudo.PuntosPorOleada, danio.VidaPerdida);
        Assert.AreEqual(_soldado.VidaMaxima - danio.VidaPerdida, _soldado.Vida);
    }

    [TestMethod]
    public void Esquiva_ConSuerteCortaLaCadenaEnElPrimerEslabon()
    {
        _azar.ProbabilidadFija = true;

        var danio = Golpear(8);

        CollectionAssert.AreEqual(new[] { "Esquiva" }, Capas(danio));
        Assert.AreEqual(Escudo.PuntosPorOleada, _escudo.Puntos);
        Assert.AreEqual(_soldado.VidaMaxima, _soldado.Vida);
    }

    [TestMethod]
    public void Bloqueo_ParaGolpesChicosYSeGasta()
    {
        for (var i = 0; i < Bloqueo.UsosPorOleada; i++)
            CollectionAssert.AreEqual(new[] { "Esquiva", "Bloqueo" }, Capas(Golpear(Bloqueo.GolpeMaximo)));

        var tercero = Golpear(Bloqueo.GolpeMaximo);

        Assert.AreEqual(0, _bloqueo.UsosRestantes);
        Assert.IsGreaterThan(2, tercero.Recorrido.Count);
    }

    [TestMethod]
    public void Escudo_SeAgotaYDejaPasarElResto()
    {
        _cadena.Quitar(_bloqueo);
        _cadena.Quitar(_armadura);

        Golpear(Escudo.PuntosPorOleada);
        var danio = Golpear(4);

        Assert.AreEqual(0, _escudo.Puntos);
        Assert.AreEqual(4, danio.VidaPerdida);
    }

    [TestMethod]
    public void ReordenarLosEslabonesCambiaElResultado()
    {
        _cadena.Quitar(_escudo);

        // Bloqueo antes que Armadura: un golpe de 4 es demasiado para bloquear.
        Assert.AreEqual(4 - Armadura.Reduccion, Golpear(4).VidaPerdida);

        // Armadura primero: lo baja a 3 y ahí sí el bloqueo lo para entero.
        _cadena.Mover(_armadura, 1);

        Assert.AreEqual(0, Golpear(4).VidaPerdida);
        Assert.AreSame(_bloqueo, _armadura.Siguiente);
        Assert.AreSame(_vida, _bloqueo.Siguiente);
    }

    [TestMethod]
    public void SeQuitanYAgreganEslabonesConLaCadenaYaArmada()
    {
        _cadena.Quitar(_bloqueo);
        _cadena.Quitar(_escudo);

        Assert.AreEqual(5 - Armadura.Reduccion, Golpear(5).VidaPerdida);
        Assert.IsNull(_escudo.Siguiente);
        Assert.AreSame(_vida, _armadura.Siguiente);

        // Agregar suma al final, así que después hay que volver a dejar la vida última.
        _cadena.Agregar(_escudo);
        _cadena.Mover(_vida, _cadena.Eslabones.Count - 1);

        Assert.AreEqual(0, Golpear(5).VidaPerdida);
        Assert.AreSame(_vida, _cadena.Eslabones[^1]);
    }

    [TestMethod]
    public void ElRecorridoDiceCuantoEntraYCuantoSaleDeCadaCapa()
    {
        var danio = Golpear(5);
        var armadura = danio.Recorrido.Single(p => p.Capa == "Armadura");

        Assert.AreEqual(5, armadura.Entra);
        Assert.AreEqual(4, armadura.Sale);
        Assert.AreEqual("Golpe de 5 › Esquiva 5 › Bloqueo 5 › Armadura 4 › Escudo 0 › no pierde vida", danio.DescribirRecorrido());
    }
}
