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
