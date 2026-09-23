namespace Kepler7.Core.Patrones.Estructurales.Facade;

internal class SistemaComunicaciones : ISubsistema
{
    public string Nombre => "Comunicaciones";
    public EstadoOperativo Estado { get; private set; }
    public string Detalle => Estado switch
    {
        EstadoOperativo.Operativo => "Enlace con la Tierra abierto",
        EstadoOperativo.Alerta => "Alerta roja en todos los canales",
        _ => "Sin enlace",
    };

    public string AbrirEnlace()
    {
        Estado = EstadoOperativo.Operativo;
        return "Enlace con la Tierra abierto.";
    }

    public string DifundirAlerta()
    {
        Estado = EstadoOperativo.Alerta;
        return "Alerta roja difundida en la estación y enviada a la Tierra.";
    }

    public string CerrarEnlace()
    {
        Estado = EstadoOperativo.Apagado;
        return "Canales de comunicación cerrados.";
    }
}
