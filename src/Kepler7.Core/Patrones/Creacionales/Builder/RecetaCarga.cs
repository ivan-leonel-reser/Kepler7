namespace Kepler7.Core.Patrones.Creacionales.Builder;

/// <summary>Nave de carga: sin armas ni radar, pero con bodega para traer más botín.</summary>
public class RecetaCarga : INaveBuilder
{
    private Nave _nave = new("Carga");

    public string Nombre => "Carga";

    public string Descripcion => "Lenta y con bodega. Si se gana la oleada, trae 50% más de créditos.";

    public void Reiniciar() => _nave = new Nave(Nombre);

    public void ConstruirCasco()
    {
        _nave.Casco = "Reforzado para carga pesada";
        _nave.Montar("Casco", _nave.Casco);
    }

    public void InstalarMotor()
    {
        _nave.Motor = "De arrastre";
        _nave.Montar("Motor", _nave.Motor);
    }

    public void InstalarEscudo()
    {
        _nave.Escudo = 8;
        _nave.Montar("Escudo", "Deflector de 8 puntos");
    }

    public void InstalarArmas()
    {
    }

    public void InstalarRadar()
    {
    }

    // Se pilotea a mano: no lleva IA de a bordo.
    public void InstalarIA()
    {
    }

    public void CargarTanques()
    {
        _nave.Tanques = 6;
        _nave.Montar("Combustible", "6 tanques para mover el peso");
    }

    public void MontarBodega()
    {
        _nave.Bodega = 40;
        _nave.Montar("Bodega", "40 toneladas de carga");
    }

    public Nave ObtenerNave() => _nave;
}
