namespace Kepler7.Core.Combate;

public readonly record struct Posicion(int Fila, int Columna)
{
    /// <summary>Distancia en casillas sin diagonales (la que se usa para moverse y para el alcance).</summary>
    public int DistanciaA(Posicion otra) => Math.Abs(Fila - otra.Fila) + Math.Abs(Columna - otra.Columna);

    public override string ToString() => $"({Fila}, {Columna})";
}
