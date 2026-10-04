using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>Un eslabón de la cadena de defensas: frena lo que puede del golpe y decide si le pasa el resto al siguiente.</summary>
public interface IManejadorDanio
{
    string Nombre { get; }

    IManejadorDanio? Siguiente { get; set; }

    void Manejar(Danio danio);
}
