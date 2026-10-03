using Kepler7.Core.Patrones.Comportamiento.Visitor;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class VisitorTests
{
    private Inventario _inventario = null!;
    private Arma _rifle = null!;
    private Arma _cuchillo = null!;
    private Pocion _botiquin = null!;
    private Armadura _chaleco = null!;

    [TestInitialize]
    public void Preparar()
    {
        _rifle = new Arma("Rifle", danio: 4, precio: 30);
        _cuchillo = new Arma("Cuchillo", danio: 2, precio: 10);
        _botiquin = new Pocion("Botiquín", curacion: 10, precio: 15);
        _chaleco = new Armadura("Chaleco", defensa: 2, precio: 25);

        _inventario = new Inventario();
        _inventario.AgregarTodos([_rifle, _botiquin, _cuchillo, _chaleco]);
    }

    [TestMethod]
    public void CadaObjetoLlamaAlMetodoDeSuTipo()
    {
        var visitante = new VisitanteQueAnota();

        _inventario.Aplicar(visitante);

        CollectionAssert.AreEqual(
            new[] { "arma:Rifle", "pocion:Botiquín", "arma:Cuchillo", "armadura:Chaleco" },
            visitante.Visitas);
    }

    [TestMethod]
    public void CalculadorPrecio_SumaElValorDeTodoElInventario()
    {
        var calculador = new CalculadorPrecio();

        _inventario.Aplicar(calculador);

        Assert.AreEqual(30 + 15 + 10 + 25, calculador.Total);
        Assert.HasCount(4, calculador.Detalle);
    }

    [TestMethod]
    public void CalculadorPrecio_UnArmaEnvenenadaValeLaMitadMas()
    {
        _rifle.Envenenada = true;
        var calculador = new CalculadorPrecio();

        _inventario.Aplicar(calculador);

        Assert.AreEqual(45 + 15 + 10 + 25, calculador.Total);
    }

    [TestMethod]
    public void ContadorArmas_SoloLeInteresanLasArmas()
    {
        var contador = new ContadorArmas();

        _inventario.Aplicar(contador);

        Assert.AreEqual(2, contador.Cantidad);
        Assert.AreEqual(6, contador.DanioTotal);
        Assert.HasCount(2, contador.Detalle);
    }

    [TestMethod]
    public void AplicadorVeneno_EnvenenaLasArmasYNoTocaLoDemas()
    {
        var veneno = new AplicadorVeneno();

        _inventario.Aplicar(veneno);

        Assert.AreEqual(2, veneno.Envenenadas);
        Assert.IsTrue(_rifle.Envenenada && _cuchillo.Envenenada);
        Assert.AreEqual("Poción · cura 10", _botiquin.Descripcion);
        Assert.AreEqual("Armadura · defensa 2", _chaleco.Descripcion);
    }

    [TestMethod]
    public void AplicadorVeneno_LaSegundaPasadaNoEnvenenaDeNuevo()
    {
        _inventario.Aplicar(new AplicadorVeneno());
        var segunda = new AplicadorVeneno();

        _inventario.Aplicar(segunda);

        Assert.AreEqual(0, segunda.Envenenadas);
        StringAssert.Contains(segunda.Resultado, "No quedaba");
    }

    [TestMethod]
    public void UnVisitanteVeLoQueDejoElAnterior()
    {
        _inventario.Aplicar(new AplicadorVeneno());
        var contador = new ContadorArmas();

        _inventario.Aplicar(contador);

        Assert.AreEqual(2, contador.Envenenadas);
    }

    private class VisitanteQueAnota : IVisitor
    {
        public List<string> Visitas { get; } = [];
        public string Nombre => "Prueba";
        public IReadOnlyList<string> Detalle => Visitas;
        public string Resultado => "";

        public void Visitar(Arma arma) => Visitas.Add($"arma:{arma.Nombre}");
        public void Visitar(Pocion pocion) => Visitas.Add($"pocion:{pocion.Nombre}");
        public void Visitar(Armadura armadura) => Visitas.Add($"armadura:{armadura.Nombre}");
    }
}
