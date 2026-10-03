namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

/// <summary>Cuenta las armas y el daño que suman. Las pociones y las armaduras las deja pasar.</summary>
public class ContadorArmas : IVisitor
{
    private readonly List<string> _detalle = [];

    public string Nombre => "Contador de armas";

    public int Cantidad { get; private set; }

    public int DanioTotal { get; private set; }

    public int Envenenadas { get; private set; }

    public IReadOnlyList<string> Detalle => _detalle;

    public string Resultado => Cantidad == 0
        ? "No hay armas en el inventario."
        : $"{Cantidad} armas, {DanioTotal} de daño en total, {Envenenadas} envenenadas.";

    public void Visitar(Arma arma)
    {
        Cantidad++;
        DanioTotal += arma.Danio;
        if (arma.Envenenada)
            Envenenadas++;
        _detalle.Add($"{arma.Nombre}: daño {arma.Danio}" + (arma.Envenenada ? ", envenenada" : ""));
    }

    public void Visitar(Pocion pocion)
    {
    }

    public void Visitar(Armadura armadura)
    {
    }
}
