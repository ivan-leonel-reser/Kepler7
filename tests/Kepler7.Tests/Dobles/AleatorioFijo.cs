using Kepler7.Services;

namespace Kepler7.Tests.Dobles;

/// <summary>Devuelve siempre los valores que se le cargan, para que los tests no dependan del azar.</summary>
public class AleatorioFijo : IAleatorio
{
    private readonly Queue<int> _valores;

    public AleatorioFijo(params int[] valores)
    {
        _valores = new Queue<int>(valores);
    }

    public bool ProbabilidadFija { get; set; }

    public int Siguiente(int minimo, int maximo) =>
        _valores.Count > 0 ? Math.Clamp(_valores.Dequeue(), minimo, maximo - 1) : minimo;

    public bool Probabilidad(int porcentaje) => ProbabilidadFija;
}
