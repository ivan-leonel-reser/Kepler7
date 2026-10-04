namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

/// <summary>
/// Una operación sobre el inventario. Tiene un método por tipo de objeto, así se pueden sumar
/// operaciones nuevas sin tocar <see cref="Arma"/>, <see cref="Pocion"/> ni <see cref="Armadura"/>.
/// </summary>
public interface IVisitor
{
    string Nombre { get; }

    void Visitar(Arma arma);

    void Visitar(Pocion pocion);

    void Visitar(Armadura armadura);

    /// <summary>Qué hizo con cada objeto que le interesó, en el orden en que los recorrió.</summary>
    IReadOnlyList<string> Detalle { get; }

    string Resultado { get; }
}
