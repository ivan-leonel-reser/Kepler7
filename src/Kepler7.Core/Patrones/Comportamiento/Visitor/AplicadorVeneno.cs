namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

/// <summary>Envenena las armas que todavía no lo están. Es el único visitante que modifica los objetos.</summary>
public class AplicadorVeneno : IVisitor
{
    private readonly List<string> _detalle = [];

    public string Nombre => "Aplicador de veneno";

    /// <summary>Cuántas armas envenenó en esta pasada.</summary>
    public int Envenenadas { get; private set; }

    public IReadOnlyList<string> Detalle => _detalle;

    public string Resultado => Envenenadas == 0
        ? "No quedaba ninguna arma sin envenenar."
        : $"{Envenenadas} armas envenenadas.";

    public void Visitar(Arma arma)
    {
        if (arma.Envenenada)
        {
            _detalle.Add($"{arma.Nombre}: ya estaba envenenada");
            return;
        }

        arma.Envenenada = true;
        Envenenadas++;
        _detalle.Add($"{arma.Nombre}: queda envenenada");
    }

    // El veneno solo se le puede poner a un arma.
    public void Visitar(Pocion pocion)
    {
    }

    public void Visitar(Armadura armadura)
    {
    }
}
