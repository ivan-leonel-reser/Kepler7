using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Creacionales.Builder;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Combate;

[TestClass]
public class EscudoNaveTests
{
    private UnidadEscuadron _soldado = null!;

    [TestInitialize]
    public void Preparar() => _soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);

    private Oleada CrearOleada(Enemigo enemigo) =>
        new(1, [_soldado, UnidadEscuadron.Crear("Luna", ClaseUnidad.Francotirador)], [enemigo], new AleatorioFijo());

    private static Nave Armar(INaveBuilder receta) => new IngenieroJefe().Ensamblar(receta);

    [TestMethod]
    public void SinNaveNoHayEscudoYElGolpeVaALaVida()
    {
        var oleada = CrearOleada(new Enemigo("Zangano", 30, 5));

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.AreEqual(0, oleada.EscudoNave);
        Assert.AreEqual(_soldado.VidaMaxima - 5, _soldado.Vida);
    }

    [TestMethod]
    public void LaNaveDeCombateTraeSuEscudoALaOleada()
    {
        var oleada = CrearOleada(new Enemigo("Zangano", 30, 5));

        oleada.RecibirApoyo(Armar(new RecetaCombate()));

        Assert.AreEqual(15, oleada.EscudoNave);
        Assert.AreEqual(15, oleada.EscudoNaveMaximo);
    }

    [TestMethod]
    public void ElEscudoFrenaElGolpeAntesQueLaVida()
    {
        var oleada = CrearOleada(new Enemigo("Zangano", 30, 5));
        oleada.RecibirApoyo(Armar(new RecetaCombate()));

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.AreEqual(10, oleada.EscudoNave);
        Assert.AreEqual(_soldado.VidaMaxima, _soldado.Vida);
        Assert.IsTrue(oleada.Bitacora.Entradas.Any(e => e.Mensaje.Contains("frena 5")));
    }

    [TestMethod]
    public void CuandoSeAgotaLoQueSobraPasaALaVida()
    {
        // La de combate trae 15 de escudo: el primer golpe de 10 lo frena entero y del segundo frena 5.
        var oleada = CrearOleada(new Enemigo("Zangano", 60, 10));
        oleada.RecibirApoyo(Armar(new RecetaCombate()));

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();
        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.AreEqual(0, oleada.EscudoNave);
        Assert.AreEqual(_soldado.VidaMaxima - 5, _soldado.Vida);
        Assert.IsTrue(oleada.Bitacora.Entradas.Any(e => e.Mensaje.Contains("se agota")));
    }

    [TestMethod]
    public void LaFichaDeLaNaveCuentaElEscudo()
    {
        StringAssert.Contains(Armar(new RecetaCarga()).Apoyo, "8 de escudo");
    }
}
