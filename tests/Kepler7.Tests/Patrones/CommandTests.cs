using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Comportamiento.Command;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class CommandTests
{
    private CampoBatalla _campo = null!;
    private UnidadEscuadron _soldado = null!;
    private UnidadEscuadron _medico = null!;
    private Enemigo _zangano = null!;

    [TestInitialize]
    public void Preparar()
    {
        _campo = new CampoBatalla();
        _soldado = UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado);
        _medico = UnidadEscuadron.Crear("Paz", ClaseUnidad.Medico);
        _zangano = new Enemigo("Zangano", 12, 3);
        _campo.Agregar(_soldado, Fila.Vanguardia);
        _campo.Agregar(_medico, Fila.Retaguardia);
        _campo.Agregar(_zangano, Fila.Vanguardia);
    }

    [TestMethod]
    public void Mover_SeDeshaceVolviendoALaFilaDeOrigen()
    {
        var comando = new MoverComando(_campo, _soldado, Fila.Retaguardia);

        comando.Ejecutar();
        Assert.AreEqual(Fila.Retaguardia, _soldado.Fila);

        comando.Deshacer();
        Assert.AreEqual(Fila.Vanguardia, _soldado.Fila);
    }

    [TestMethod]
    public void Atacar_AlDeshacerReviveAlQueCayo()
    {
        var comando = new AtacarComando(_soldado, _zangano);
        _zangano.PerderVida(8);

        comando.Ejecutar();
        Assert.IsFalse(_zangano.EstaViva);
        StringAssert.Contains(comando.Descripcion, "derriba");

        comando.Deshacer();
        Assert.AreEqual(4, _zangano.Vida);
    }

    [TestMethod]
    public void Curar_SoloSeDeshaceLoQueRealmenteCuro()
    {
        _soldado.PerderVida(4);
        var comando = new CurarComando(_medico, _soldado, 10);

        comando.Ejecutar();
        Assert.AreEqual(_soldado.VidaMaxima, _soldado.Vida);

        comando.Deshacer();
        Assert.AreEqual(_soldado.VidaMaxima - 4, _soldado.Vida);
    }

    [TestMethod]
    public void Construir_AlRehacerVuelveLaMismaBarricada()
    {
        var comando = new ConstruirComando(_campo, _soldado);

        comando.Ejecutar();
        var barricada = _campo.EnFila(Bando.Escuadron, Fila.Vanguardia).OfType<Barricada>().Single();
        comando.Deshacer();
        Assert.IsEmpty(_campo.Unidades.OfType<Barricada>().ToList());

        comando.Ejecutar();
        Assert.AreSame(barricada, _campo.Unidades.OfType<Barricada>().Single());
    }
}
