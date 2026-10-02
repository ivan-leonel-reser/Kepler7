namespace Kepler7.Services;

public class Aleatorio : IAleatorio
{
    private readonly Random _random;

    public Aleatorio()
    {
        _random = new Random();
    }

    public Aleatorio(int semilla)
    {
        _random = new Random(semilla);
    }

    public int Siguiente(int minimo, int maximo) => _random.Next(minimo, maximo);

    public bool Probabilidad(int porcentaje) => _random.Next(100) < porcentaje;
}
