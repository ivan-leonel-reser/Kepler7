namespace Kepler7.Core.Patrones.Creacionales.Prototype;

/// <summary>Algo que sabe hacer una copia independiente de sí mismo.</summary>
public interface IPrototipo
{
    IPrototipo Clonar();
}
