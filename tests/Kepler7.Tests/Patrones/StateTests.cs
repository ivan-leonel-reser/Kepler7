using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;
using Kepler7.Core.Patrones.Comportamiento.State;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class StateTests
{
    // Soldado: 30 de vida y 8 de ataque.
    private UnidadEscuadron _soldado = null!;

    [TestInitialize]
    public void Preparar()
    {
        _soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);
    }

    private static Danio Golpe(int cantidad, EfectoGolpe efecto = EfectoGolpe.Ninguno) =>
        new(cantidad, TipoDanio.Fisico, new Enemigo("Enemigo", 20, cantidad) { EfectoAlGolpear = efecto });

    [TestMethod]
    public void UnaUnidadEmpiezaEnEstadoNormal()
    {
        Assert.IsInstanceOfType<Normal>(_soldado.Estado);
        Assert.IsTrue(_soldado.Estado.PuedeActuar);
        Assert.IsTrue(_soldado.Estado.Mover(_soldado));
        Assert.AreEqual(8, _soldado.DanioDeAtaque());
        Assert.AreEqual(10, _soldado.DanioDeAtaque(bonus: 2));
    }

    [TestMethod]
    public void UnGolpeComunNoCambiaElEstado()
    {
        Assert.AreEqual(4, _soldado.RecibirGolpe(Golpe(4)));

        Assert.IsInstanceOfType<Normal>(_soldado.Estado);
    }

    [TestMethod]
    public void UnGolpeConVenenoLaDejaEnvenenada()
    {
        _soldado.RecibirGolpe(Golpe(4, EfectoGolpe.Veneno));

        Assert.IsInstanceOfType<Envenenado>(_soldado.Estado);
        Assert.AreEqual(26, _soldado.Vida);
        Assert.AreEqual(8 - Envenenado.AtaquePerdido, _soldado.DanioDeAtaque());
        Assert.IsTrue(_soldado.Estado.PuedeActuar);
    }

    [TestMethod]
    public void SiLasDefensasFrenanElGolpeNoHayCambioDeEstado()
    {
        new Armeria().Equipar(_soldado, new AleatorioFijo());

        // Un golpe de 3 lo para entero el bloqueo.
        Assert.AreEqual(0, _soldado.RecibirGolpe(Golpe(Bloqueo.GolpeMaximo, EfectoGolpe.Veneno)));

        Assert.IsInstanceOfType<Normal>(_soldado.Estado);
    }

    [TestMethod]
    public void ElVenenoRestaVidaCadaTurnoYDespuesSePasa()
    {
        _soldado.RecibirGolpe(Golpe(4, EfectoGolpe.Veneno));

        for (var turno = 1; turno < Envenenado.Duracion; turno++)
        {
            Assert.IsNotNull(_soldado.Estado.TerminarTurno(_soldado));
            Assert.AreEqual(26 - turno * Envenenado.DanioPorTurno, _soldado.Vida);
            Assert.IsInstanceOfType<Envenenado>(_soldado.Estado);
        }
        _soldado.Estado.TerminarTurno(_soldado);

        Assert.AreEqual(26 - Envenenado.Duracion * Envenenado.DanioPorTurno, _soldado.Vida);
        Assert.IsInstanceOfType<Normal>(_soldado.Estado);
    }

    [TestMethod]
    public void ElVenenoPuedeDerribarALaUnidad()
    {
        _soldado.CambiarEstado(new Envenenado());
        _soldado.PerderVida(_soldado.Vida - 1);

        var aviso = _soldado.Estado.TerminarTurno(_soldado);

        Assert.IsFalse(_soldado.EstaViva);
        Assert.Contains("cae", aviso!);
    }

    [TestMethod]
    public void UnGolpeQueAturdeLaDejaSinTurno()
    {
        _soldado.RecibirGolpe(Golpe(4, EfectoGolpe.Aturdimiento));

        Assert.IsInstanceOfType<Aturdido>(_soldado.Estado);
        Assert.IsFalse(_soldado.Estado.PuedeActuar);
        Assert.IsFalse(_soldado.Estado.Mover(_soldado));
        Assert.AreEqual(0, _soldado.DanioDeAtaque());
    }

    [TestMethod]
    public void AturdidaPierdeUnTurnoYNoLaAturdenDosVecesSeguidas()
    {
        _soldado.RecibirGolpe(Golpe(2, EfectoGolpe.Aturdimiento));

        _soldado.Estado.TerminarTurno(_soldado);
        Assert.IsInstanceOfType<Normal>(_soldado.Estado);

        // Recién recuperada aguanta el golpe sin volver a quedar aturdida.
        _soldado.RecibirGolpe(Golpe(2, EfectoGolpe.Aturdimiento));
        Assert.IsInstanceOfType<Normal>(_soldado.Estado);

        // Pasado su turno, la pueden aturdir de nuevo.
        _soldado.Estado.TerminarTurno(_soldado);
        _soldado.RecibirGolpe(Golpe(2, EfectoGolpe.Aturdimiento));
        Assert.IsInstanceOfType<Aturdido>(_soldado.Estado);
    }

    [TestMethod]
    public void MuyHeridaEntraEnBerserkYPegaMasFuerte()
    {
        // Queda en 9 de 30: menos de un tercio.
        _soldado.RecibirGolpe(Golpe(21));

        Assert.IsInstanceOfType<Berserk>(_soldado.Estado);
        Assert.AreEqual(8 + Berserk.BonusAtaque, _soldado.DanioDeAtaque());
        Assert.IsTrue(_soldado.Estado.Mover(_soldado));
    }

    [TestMethod]
    public void EnBerserkRecibeMasDanioYQuedaEnElRecorridoDelGolpe()
    {
        _soldado.RecibirGolpe(Golpe(21));
        var golpe = Golpe(2);

        Assert.AreEqual(2 + Berserk.DanioExtra, _soldado.RecibirGolpe(golpe));

        Assert.AreEqual(5, _soldado.Vida);
        Assert.AreEqual("Berserk", golpe.Recorrido[0].Capa);
        Assert.Contains("Berserk 4", golpe.DescribirRecorrido());
    }

    [TestMethod]
    public void EnBerserkSeCalmaCuandoRecuperaVida()
    {
        _soldado.RecibirGolpe(Golpe(21));

        Assert.IsNull(_soldado.Estado.TerminarTurno(_soldado));
        Assert.IsInstanceOfType<Berserk>(_soldado.Estado);

        _soldado.Curar(10);
        Assert.IsNotNull(_soldado.Estado.TerminarTurno(_soldado));
        Assert.IsInstanceOfType<Normal>(_soldado.Estado);
    }

    [TestMethod]
    public void SoloDesdeNormalSePasaAOtroEstado()
    {
        _soldado.RecibirGolpe(Golpe(4, EfectoGolpe.Veneno));

        _soldado.RecibirGolpe(Golpe(4, EfectoGolpe.Aturdimiento));

        Assert.IsInstanceOfType<Envenenado>(_soldado.Estado);
    }

    [TestMethod]
    public void UnaBarricadaNoCambiaDeEstado()
    {
        var barricada = new Barricada("Barricada");

        barricada.RecibirGolpe(Golpe(4, EfectoGolpe.Veneno));
        barricada.CambiarEstado(new Aturdido());

        Assert.IsInstanceOfType<Normal>(barricada.Estado);
    }

    [TestMethod]
    public void CadaEstadoSeDistinguePorNombreYColor()
    {
        IEstadoPersonaje[] estados = [new Normal(), new Envenenado(), new Aturdido(), new Berserk()];

        Assert.HasCount(4, estados.Select(e => e.Nombre).Distinct().ToList());
        Assert.HasCount(4, estados.Select(e => e.Color).Distinct().ToList());
        Assert.IsTrue(estados.All(e => e.Color.StartsWith('#') && e.Color.Length == 7));
    }
}
