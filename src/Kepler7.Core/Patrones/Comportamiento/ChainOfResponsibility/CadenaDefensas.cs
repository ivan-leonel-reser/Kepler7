using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>
/// La cadena de una unidad. Se arma en ejecución y se le pueden agregar, quitar y mover eslabones
/// en cualquier momento: cada cambio vuelve a enlazar cada eslabón con el que le sigue.
/// </summary>
public class CadenaDefensas
{
    private readonly List<IManejadorDanio> _eslabones = [];

    public IReadOnlyList<IManejadorDanio> Eslabones => _eslabones;

    public void Agregar(IManejadorDanio eslabon)
    {
        _eslabones.Add(eslabon);
        Enlazar();
    }

    public bool Quitar(IManejadorDanio eslabon)
    {
        if (!_eslabones.Remove(eslabon))
            return false;

        eslabon.Siguiente = null;
        Enlazar();
        return true;
    }

    /// <param name="posicion">Lugar que pasa a ocupar, contando desde 0.</param>
    public void Mover(IManejadorDanio eslabon, int posicion)
    {
        if (!_eslabones.Remove(eslabon))
            throw new InvalidOperationException($"{eslabon.Nombre} no está en la cadena.");

        _eslabones.Insert(Math.Clamp(posicion, 0, _eslabones.Count), eslabon);
        Enlazar();
    }

    public void Vaciar()
    {
        foreach (var eslabon in _eslabones)
            eslabon.Siguiente = null;
        _eslabones.Clear();
    }

    /// <summary>El golpe entra por el primer eslabón y cada uno decide si sigue viaje.</summary>
    public void Manejar(Danio danio) => _eslabones.FirstOrDefault()?.Manejar(danio);

    private void Enlazar()
    {
        for (var i = 0; i < _eslabones.Count; i++)
            _eslabones[i].Siguiente = i + 1 < _eslabones.Count ? _eslabones[i + 1] : null;
    }
}
