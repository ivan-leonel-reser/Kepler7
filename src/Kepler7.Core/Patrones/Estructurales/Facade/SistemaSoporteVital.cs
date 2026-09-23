namespace Kepler7.Core.Patrones.Estructurales.Facade;

internal class SistemaSoporteVital : ISubsistema
{
    public string Nombre => "Soporte vital";
    public EstadoOperativo Estado { get; private set; }
    public int Oxigeno { get; private set; } = 30;
    public string Detalle => $"Oxígeno al {Oxigeno}%";

    public string Activar()
    {
        Oxigeno = 100;
        Estado = EstadoOperativo.Operativo;
        return "Oxígeno y presión estables en los seis sectores.";
    }

    public string SellarSectores()
    {
        Oxigeno = 80;
        Estado = EstadoOperativo.Alerta;
        return "Sectores comprometidos sellados con compuertas estancas.";
    }

    public string ModoMinimo()
    {
        Oxigeno = 30;
        Estado = EstadoOperativo.Apagado;
        return "Soporte vital en modo mínimo para la guardia.";
    }
}
