namespace Kepler7.Core.Patrones.Estructurales.Facade;

internal class SistemaDefensa : ISubsistema
{
    public string Nombre => "Defensa";
    public EstadoOperativo Estado { get; private set; }
    public int Escudos { get; private set; }
    public string Detalle => $"Escudos al {Escudos}%";

    public string ModoVigilancia()
    {
        Escudos = 50;
        Estado = EstadoOperativo.Operativo;
        return "Torretas en vigilancia, escudos al 50%.";
    }

    public string CombateTotal()
    {
        Escudos = 100;
        Estado = EstadoOperativo.Alerta;
        return "Escudos al 100% y torretas armadas.";
    }

    public string Desarmar()
    {
        Escudos = 0;
        Estado = EstadoOperativo.Apagado;
        return "Torretas desarmadas y escudos bajos.";
    }
}
