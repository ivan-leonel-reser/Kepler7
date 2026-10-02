namespace Kepler7.Core.Comun;

/// <summary>Fuente de azar del juego. Se inyecta para poder fijar los resultados en los tests.</summary>
public interface IAleatorio
{
    /// <summary>Entero entre <paramref name="minimo"/> (incluido) y <paramref name="maximo"/> (excluido).</summary>
    int Siguiente(int minimo, int maximo);

    /// <summary>Devuelve true con la probabilidad indicada (0 a 100).</summary>
    bool Probabilidad(int porcentaje);
}
