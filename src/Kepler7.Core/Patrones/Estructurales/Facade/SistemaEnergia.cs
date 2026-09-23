namespace Kepler7.Core.Patrones.Estructurales.Facade;

internal class SistemaEnergia : ISubsistema
{
    public string Nombre => "Energía";
    public EstadoOperativo Estado { get; private set; }
    public int Potencia { get; private set; }
    public string Detalle => Potencia == 0 ? "Reactor detenido" : $"Reactor al {Potencia}%";

    public string EncenderReactor()
    {
        Potencia = 100;
        Estado = EstadoOperativo.Operativo;
        return "Reactor de fusión encendido al 100%.";
    }

    public string DesviarAReserva()
    {
        Potencia = 60;
        Estado = EstadoOperativo.Alerta;
        return "Energía desviada a sistemas críticos: reactor al 60%.";
    }

    public string Apagar()
    {
        Potencia = 0;
        Estado = EstadoOperativo.Apagado;
        return "Reactor en parada segura.";
    }
}
