namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

public class Pocion : IItem
{
    public Pocion(string nombre, int curacion, int precio)
    {
        Nombre = nombre;
        Curacion = curacion;
        Precio = precio;
    }

    public string Nombre { get; }
    public int Curacion { get; }
    public int Precio { get; }

    public string Descripcion => $"Poción · cura {Curacion}";

    public void Aceptar(IVisitor visitante) => visitante.Visitar(this);
}
