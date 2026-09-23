using Kepler7.Core.Combate;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Combate;

[TestClass]
public class CombateTests
{
    [TestMethod]
    public void Posicion_DistanciaSinDiagonales()
    {
        Assert.AreEqual(5, new Posicion(0, 0).DistanciaA(new Posicion(2, 3)));
    }

    [TestMethod]
    public void Unidad_LaVidaNoBajaDeCeroNiPasaLaMaxima()
    {
        var soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);

        Assert.AreEqual(30, soldado.PerderVida(50));
        Assert.AreEqual(0, soldado.Vida);
        Assert.IsFalse(soldado.EstaViva);

        Assert.AreEqual(30, soldado.Curar(100));
        Assert.AreEqual(soldado.VidaMaxima, soldado.Vida);
    }

    [TestMethod]
    public void Danio_ElRestanteNuncaEsNegativo()
    {
        var danio = new Danio(10) { Restante = -5 };

        Assert.AreEqual(0, danio.Restante);
        Assert.AreEqual(10, danio.Original);
        Assert.IsTrue(danio.FueAbsorbido);
    }

    [TestMethod]
    public void Grilla_NoPermiteDosUnidadesEnLaMismaCasilla()
    {
        var grilla = new Grilla(5, 5);
        grilla.Colocar(UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado), new Posicion(1, 1));

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            grilla.Colocar(new Enemigo("Zangano", 10, 3, 2, 1), new Posicion(1, 1)));
    }

    [TestMethod]
    public void Grilla_NoPermiteMoverAfuera()
    {
        var grilla = new Grilla(5, 5);
        var soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);
        grilla.Colocar(soldado, new Posicion(0, 0));

        Assert.ThrowsExactly<InvalidOperationException>(() => grilla.Mover(soldado, new Posicion(-1, 0)));
        Assert.AreEqual(new Posicion(0, 0), soldado.Posicion);
    }

    [TestMethod]
    public void Grilla_UnaUnidadCaidaLiberaLaCasilla()
    {
        var grilla = new Grilla(5, 5);
        var enemigo = new Enemigo("Zangano", 10, 3, 2, 1);
        grilla.Colocar(enemigo, new Posicion(2, 2));

        enemigo.PerderVida(10);

        Assert.IsTrue(grilla.EstaLibre(new Posicion(2, 2)));
        Assert.IsEmpty(grilla.UnidadesVivasDe(Bando.Enjambre).ToList());
    }

    [TestMethod]
    public void Contexto_EncuentraAlRivalMasCercano()
    {
        var grilla = new Grilla(6, 6);
        var enemigo = new Enemigo("Zangano", 10, 3, 2, 1);
        var lejos = UnidadEscuadron.Crear("Lejos", ClaseUnidad.Soldado);
        var cerca = UnidadEscuadron.Crear("Cerca", ClaseUnidad.Medico);
        grilla.Colocar(enemigo, new Posicion(0, 0));
        grilla.Colocar(lejos, new Posicion(5, 5));
        grilla.Colocar(cerca, new Posicion(1, 1));

        var contexto = new Contexto(grilla, 1, new AleatorioFijo());

        Assert.AreSame(cerca, contexto.RivalMasCercano(enemigo));
        Assert.HasCount(1, contexto.AliadosDe(lejos).ToList());
    }
}
