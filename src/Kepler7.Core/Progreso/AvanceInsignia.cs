namespace Kepler7.Core.Progreso;

/// <summary>Cuánto le falta al jugador para una insignia, por ejemplo 2 de 3 recetas usadas.</summary>
public record AvanceInsignia(int Actual, int Requerido, string Detalle)
{
    public bool Completo => Actual >= Requerido;
}
