namespace Kepler7.Core.Patrones.Estructurales.Facade;

internal class SistemaSeguridad : ISubsistema
{
    public string Nombre => "Seguridad";
    public EstadoOperativo Estado { get; private set; }
    public string Detalle => Estado switch
    {
        EstadoOperativo.Operativo => "Accesos por credencial",
        EstadoOperativo.Alerta => "Bloqueo total",
        _ => "Guardia mínima",
    };

    public string HabilitarAccesos()
    {
        Estado = EstadoOperativo.Operativo;
        return "Accesos por credencial habilitados.";
    }

    public string BloqueoTotal()
    {
        Estado = EstadoOperativo.Alerta;
        return "Bloqueo total: compuertas internas y bóveda selladas.";
    }

    public string SellarEstacion()
    {
        Estado = EstadoOperativo.Apagado;
        return "Estación sellada, queda la guardia mínima.";
    }
}
