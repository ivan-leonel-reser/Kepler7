namespace Kepler7.Core.Patrones.Estructurales.Facade;

internal class SistemaNavegacion : ISubsistema
{
    private const int AltitudNormal = 420;

    public string Nombre => "Navegación";
    public EstadoOperativo Estado { get; private set; }
    public int Altitud { get; private set; } = AltitudNormal;
    public string Detalle => Estado == EstadoOperativo.Apagado ? $"Órbita pasiva a {Altitud} km" : $"Altitud {Altitud} km";

    public string EstabilizarOrbita()
    {
        Altitud = AltitudNormal;
        Estado = EstadoOperativo.Operativo;
        return $"Órbita estabilizada a {Altitud} km.";
    }

    public string ManiobraEvasiva()
    {
        Altitud = AltitudNormal + 2;
        Estado = EstadoOperativo.Alerta;
        return "Maniobra evasiva: la estación sube 2 km.";
    }

    public string ApagarPropulsores()
    {
        Estado = EstadoOperativo.Apagado;
        return "Propulsores apagados, órbita pasiva.";
    }
}
