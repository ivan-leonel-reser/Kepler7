namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

/// <summary>Una armadura guardada en el Almacén. No es la capa de defensa de la Armería, que se llama igual.</summary>
public class Armadura : IItem
{
    public Armadura(string nombre, int defensa, int precio)
    {
        Nombre = nombre;
        Defensa = defensa;
        Precio = precio;
    }

    public string Nombre { get; }
    public int Defensa { get; }
    public int Precio { get; }

    public string Descripcion => $"Armadura · defensa {Defensa}";

    public void Aceptar(IVisitor visitante) => visitante.Visitar(this);
}
