namespace Kepler7.Core.Combate;

/// <summary>Unidad del Enjambre. Su forma de decidir la agrega el patrón Strategy.</summary>
public class Enemigo : Unidad
{
    public Enemigo(string nombre, int vidaMaxima, int ataque, int movimiento, int alcance)
        : base(nombre, Bando.Enjambre, vidaMaxima, ataque, movimiento, alcance)
    {
    }
}
