namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

public class Arma : IItem
{
    public Arma(string nombre, int danio, int precio)
    {
        Nombre = nombre;
        Danio = danio;
        Precio = precio;
    }

    public string Nombre { get; }
    public int Danio { get; }
    public int Precio { get; }
    public bool Envenenada { get; set; }

    public string Descripcion => $"Arma · daño {Danio}" + (Envenenada ? " · envenenada" : "");

    // El arma solo sabe a qué método llamar; qué se hace con ella lo decide el visitante.
    public void Aceptar(IVisitor visitante) => visitante.Visitar(this);
}
