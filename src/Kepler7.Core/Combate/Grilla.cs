namespace Kepler7.Core.Combate;

/// <summary>Tablero de la misión. Una casilla la ocupa como mucho una unidad viva.</summary>
public class Grilla
{
    private readonly List<Unidad> _unidades = [];

    public Grilla(int filas, int columnas)
    {
        if (filas <= 0 || columnas <= 0)
            throw new ArgumentOutOfRangeException(filas <= 0 ? nameof(filas) : nameof(columnas));

        Filas = filas;
        Columnas = columnas;
    }

    public int Filas { get; }
    public int Columnas { get; }

    // Incluye a las unidades caídas, que ya no ocupan casilla.
    public IReadOnlyList<Unidad> Unidades => _unidades;

    public bool EstaDentro(Posicion posicion) =>
        posicion.Fila >= 0 && posicion.Fila < Filas &&
        posicion.Columna >= 0 && posicion.Columna < Columnas;

    public Unidad? UnidadEn(Posicion posicion) =>
        _unidades.FirstOrDefault(u => u.EstaViva && u.Posicion == posicion);

    public bool EstaLibre(Posicion posicion) => EstaDentro(posicion) && UnidadEn(posicion) is null;

    public IEnumerable<Unidad> UnidadesVivasDe(Bando bando) =>
        _unidades.Where(u => u.Bando == bando && u.EstaViva);

    public void Colocar(Unidad unidad, Posicion posicion)
    {
        if (_unidades.Contains(unidad))
            throw new InvalidOperationException($"{unidad.Nombre} ya está en la grilla.");
        if (!EstaLibre(posicion))
            throw new InvalidOperationException($"La casilla {posicion} no está libre.");

        unidad.Posicion = posicion;
        _unidades.Add(unidad);
    }

    public void Mover(Unidad unidad, Posicion destino)
    {
        if (!_unidades.Contains(unidad))
            throw new InvalidOperationException($"{unidad.Nombre} no está en la grilla.");
        if (!EstaLibre(destino))
            throw new InvalidOperationException($"La casilla {destino} no está libre.");

        unidad.Posicion = destino;
    }
}
