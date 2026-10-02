namespace Kepler7.Core.Patrones.Creacionales.Builder;

/// <summary>Nave armada y blindada: da un disparo de apoyo al empezar la oleada.</summary>
public class RecetaCombate : INaveBuilder
{
    private Nave _nave = new("Combate");

    public string Nombre => "Combate";

    public string Descripcion => "Blindada y armada. Abre la oleada con un disparo sobre la vanguardia enemiga.";

    public void Reiniciar() => _nave = new Nave(Nombre);

    public void ConstruirCasco()
    {
        _nave.Casco = "Blindado de titanio";
        _nave.Montar("Casco", _nave.Casco);
    }

    public void InstalarMotor()
    {
        _nave.Motor = "De plasma, corto y potente";
        _nave.Montar("Motor", _nave.Motor);
    }

    public void InstalarEscudo()
    {
        _nave.Escudo = 60;
        _nave.Montar("Escudo", "Deflector pesado de 60 puntos");
    }

    public void InstalarArmas()
    {
        _nave.Armas = 6;
        _nave.Montar("Armas", "Cañones gemelos (6 de daño)");
    }

    // Apunta con mira óptica: no lleva radar.
    public void InstalarRadar()
    {
    }

    public void InstalarIA()
    {
        _nave.IA = "De tiro";
        _nave.Montar("IA de a bordo", _nave.IA);
    }

    public void CargarTanques()
    {
        _nave.Tanques = 2;
        _nave.Montar("Combustible", "2 tanques, lo justo para ir y volver");
    }

    public void MontarBodega()
    {
    }

    public Nave ObtenerNave() => _nave;
}
