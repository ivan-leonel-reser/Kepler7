using Kepler7.Core.Combate;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Combate;

[TestClass]
public class CombateTests
{
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
    public void CuerpoACuerpo_SoloAtacaDesdeAdelante()
    {
        var campo = new CampoBatalla();
        var soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);
        var francotirador = UnidadEscuadron.Crear("Luna", ClaseUnidad.Francotirador);
        campo.Agregar(soldado, Fila.Retaguardia);
        campo.Agregar(francotirador, Fila.Retaguardia);

        Assert.IsFalse(soldado.PuedeAtacar);
        Assert.IsTrue(francotirador.PuedeAtacar);

        campo.Mover(soldado, Fila.Vanguardia);
        Assert.IsTrue(soldado.PuedeAtacar);
    }

    [TestMethod]
    public void NoEntranMasDeTresPorFila()
    {
        var campo = new CampoBatalla();
        for (var i = 0; i < CampoBatalla.LugaresPorFila; i++)
            campo.Agregar(new Enemigo($"Zangano {i}", 10, 3), Fila.Vanguardia);

        Assert.IsFalse(campo.HayLugar(Bando.Enjambre, Fila.Vanguardia));
        Assert.IsTrue(campo.HayLugar(Bando.Escuadron, Fila.Vanguardia));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            campo.Agregar(new Enemigo("Zangano extra", 10, 3), Fila.Vanguardia));
    }

    [TestMethod]
    public void MoverAUnaFilaLlenaNoCambiaNada()
    {
        var campo = new CampoBatalla();
        for (var i = 0; i < CampoBatalla.LugaresPorFila; i++)
            campo.Agregar(UnidadEscuadron.Crear($"Soldado {i}", ClaseUnidad.Soldado), Fila.Vanguardia);
        var medico = UnidadEscuadron.Crear("Paz", ClaseUnidad.Medico);
        campo.Agregar(medico, Fila.Retaguardia);

        Assert.ThrowsExactly<InvalidOperationException>(() => campo.Mover(medico, Fila.Vanguardia));
        Assert.AreEqual(Fila.Retaguardia, medico.Fila);
    }

    [TestMethod]
    public void SoloSeAtacaAdelante_HastaQueQuedaVacio()
    {
        var campo = new CampoBatalla();
        var adelante = new Enemigo("Coloso", 10, 5);
        var atras = new Enemigo("Escupidor", 8, 4, atacaADistancia: true);
        campo.Agregar(adelante, Fila.Vanguardia);
        campo.Agregar(atras, Fila.Retaguardia);

        CollectionAssert.AreEqual(new[] { adelante }, campo.Atacables(Bando.Escuadron).ToArray());

        adelante.PerderVida(10);

        CollectionAssert.AreEqual(new[] { atras }, campo.Atacables(Bando.Escuadron).ToArray());
    }

    [TestMethod]
    public void Contexto_EncuentraAlAtacableMasDebil()
    {
        var campo = new CampoBatalla();
        var enemigo = new Enemigo("Zangano", 10, 3);
        var fuerte = UnidadEscuadron.Crear("Fuerte", ClaseUnidad.Comandante);
        var debil = UnidadEscuadron.Crear("Debil", ClaseUnidad.Soldado);
        debil.PerderVida(20);
        var escondido = UnidadEscuadron.Crear("Escondido", ClaseUnidad.Francotirador);
        escondido.PerderVida(15);
        campo.Agregar(enemigo, Fila.Vanguardia);
        campo.Agregar(fuerte, Fila.Vanguardia);
        campo.Agregar(debil, Fila.Vanguardia);
        campo.Agregar(escondido, Fila.Retaguardia);

        var contexto = new Contexto(campo, 1, new AleatorioFijo());

        // El de atrás tiene menos vida, pero no se lo puede atacar mientras haya alguien adelante.
        Assert.AreSame(debil, contexto.AtacableMasDebil(enemigo));
        Assert.HasCount(2, contexto.AliadosDe(fuerte).ToList());
    }
}
