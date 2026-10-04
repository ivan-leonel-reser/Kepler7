namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

/// <summary>Lo que hay en el Almacén. No sabe de precios ni de venenos: solo pasea al visitante por cada objeto.</summary>
public class Inventario
{
    private readonly List<IItem> _items = [];

    public IReadOnlyList<IItem> Items => _items;

    public void Agregar(IItem item) => _items.Add(item);

    public void AgregarTodos(IEnumerable<IItem> items) => _items.AddRange(items);

    public void Aplicar(IVisitor visitante)
    {
        foreach (var item in _items)
            item.Aceptar(visitante);
    }

    public void Vaciar() => _items.Clear();
}
