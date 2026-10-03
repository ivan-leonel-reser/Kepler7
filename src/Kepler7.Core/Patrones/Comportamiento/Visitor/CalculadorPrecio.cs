namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

/// <summary>Suma cuánto vale todo el inventario. Un arma envenenada vale la mitad más.</summary>
public class CalculadorPrecio : IVisitor
{
    private readonly List<string> _detalle = [];

    public string Nombre => "Calculador de precio";

    public int Total { get; private set; }

    public IReadOnlyList<string> Detalle => _detalle;

    public string Resultado => $"Valor total del inventario: {Total} créditos.";

    public void Visitar(Arma arma)
    {
        var valor = arma.Envenenada ? arma.Precio + arma.Precio / 2 : arma.Precio;
        Sumar(arma, valor, arma.Envenenada ? " (envenenada, vale la mitad más)" : "");
    }

    public void Visitar(Pocion pocion) => Sumar(pocion, pocion.Precio);

    public void Visitar(Armadura armadura) => Sumar(armadura, armadura.Precio);

    private void Sumar(IItem item, int valor, string nota = "")
    {
        Total += valor;
        _detalle.Add($"{item.Nombre}: {valor} créditos{nota}");
    }
}
