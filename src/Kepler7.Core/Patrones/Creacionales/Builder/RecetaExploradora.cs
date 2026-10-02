namespace Kepler7.Core.Patrones.Creacionales.Builder;

/// <summary>Nave liviana para reconocer al enemigo: radar y alcance, sin armas ni bodega.</summary>
public class RecetaExploradora : INaveBuilder
{
    private Nave _nave = new("Exploradora");

    public string Nombre => "Exploradora";

    public string Descripcion => "Liviana y con radar. Revela la IA de los enemigos de la oleada.";

    public void Reiniciar() => _nave = new Nave(Nombre);

    public void ConstruirCasco()
    {
        _nave.Casco = "Ligero de fibra de carbono";
        _nave.Montar("Casco", _nave.Casco);
    }

    public void InstalarMotor()
    {
        _nave.Motor = "Iónico de largo alcance";
        _nave.Montar("Motor", _nave.Motor);
    }

    public void InstalarEscudo()
    {
        _nave.Escudo = 20;
        _nave.Montar("Escudo", "Deflector liviano de 20 puntos");
    }

    // Va desarmada: cada kilo se usa para sensores y combustible.
    public void InstalarArmas()
    {
    }

    public void InstalarRadar()
    {
        _nave.Radar = true;
        _nave.Montar("Radar", "Barrido profundo: lee las señales del Enjambre");
    }

    public void InstalarIA()
    {
        _nave.IA = "De reconocimiento";
        _nave.Montar("IA de a bordo", _nave.IA);
    }

    public void CargarTanques()
    {
        _nave.Tanques = 4;
        _nave.Montar("Combustible", "4 tanques para vuelos largos");
    }

    public void MontarBodega()
    {
    }

    public Nave ObtenerNave() => _nave;
}
