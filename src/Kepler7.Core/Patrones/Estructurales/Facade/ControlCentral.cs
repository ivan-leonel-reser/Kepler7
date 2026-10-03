using Kepler7.BE;
using Kepler7.Services;

namespace Kepler7.Core.Patrones.Estructurales.Facade;

/// <summary>Fachada de la estación: con una sola orden coordina los seis subsistemas en el orden correcto.</summary>
public class ControlCentral
{
    private const string Origen = "Control central";

    private readonly SistemaEnergia _energia = new();
    private readonly SistemaSoporteVital _soporteVital = new();
    private readonly SistemaNavegacion _navegacion = new();
    private readonly SistemaComunicaciones _comunicaciones = new();
    private readonly SistemaSeguridad _seguridad = new();
    private readonly SistemaDefensa _defensa = new();
    private readonly ISubsistema[] _subsistemas;
    private readonly Bitacora? _bitacoraEstacion;

    /// <param name="bitacoraEstacion">Si se pasa, cada operación deja ahí un resumen de una línea.</param>
    public ControlCentral(Bitacora? bitacoraEstacion = null)
    {
        _bitacoraEstacion = bitacoraEstacion;
        _subsistemas = [_energia, _soporteVital, _navegacion, _comunicaciones, _seguridad, _defensa];
    }

    public ModoEstacion Modo { get; private set; } = ModoEstacion.Apagada;
    public int TurnosIniciados { get; private set; }
    public int EmergenciasDeclaradas { get; private set; }
    public int VecesApagada { get; private set; }

    public IReadOnlyList<EstadoSubsistema> Subsistemas =>
        _subsistemas.Select(s => new EstadoSubsistema(s.Nombre, s.Estado, s.Detalle)).ToList();

    public IReadOnlyList<EntradaBitacora> IniciarTurno()
    {
        // Sin energía no arranca nada, por eso el reactor va primero.
        var secuencia = new List<EntradaBitacora>
        {
            Paso(_energia, _energia.EncenderReactor()),
            Paso(_soporteVital, _soporteVital.Activar()),
            Paso(_navegacion, _navegacion.EstabilizarOrbita()),
            Paso(_comunicaciones, _comunicaciones.AbrirEnlace()),
            Paso(_seguridad, _seguridad.HabilitarAccesos()),
            Paso(_defensa, _defensa.ModoVigilancia()),
        };

        Modo = ModoEstacion.Operativa;
        TurnosIniciados++;
        return Cerrar(secuencia, $"Turno {TurnosIniciados} iniciado. Estación operativa.", TipoEntrada.Exito);
    }

    public IReadOnlyList<EntradaBitacora> EntrarEnEmergencia()
    {
        if (Modo == ModoEstacion.Apagada)
            return [new(Origen, "La estación está apagada: primero iniciá un turno.", TipoEntrada.Advertencia)];
        if (Modo == ModoEstacion.Emergencia)
            return [new(Origen, "La estación ya está en modo emergencia.", TipoEntrada.Advertencia)];

        // Primero se cierra y se protege la estación, después se reacomoda el resto.
        var secuencia = new List<EntradaBitacora>
        {
            Paso(_seguridad, _seguridad.BloqueoTotal()),
            Paso(_defensa, _defensa.CombateTotal()),
            Paso(_energia, _energia.DesviarAReserva()),
            Paso(_soporteVital, _soporteVital.SellarSectores()),
            Paso(_navegacion, _navegacion.ManiobraEvasiva()),
            Paso(_comunicaciones, _comunicaciones.DifundirAlerta()),
        };

        Modo = ModoEstacion.Emergencia;
        EmergenciasDeclaradas++;
        return Cerrar(secuencia, "Modo emergencia activo.", TipoEntrada.Peligro);
    }

    public IReadOnlyList<EntradaBitacora> ApagarEstacion()
    {
        if (Modo == ModoEstacion.Apagada)
            return [new(Origen, "La estación ya está apagada.", TipoEntrada.Advertencia)];

        // El reactor se apaga último para que el resto cierre con energía.
        var secuencia = new List<EntradaBitacora>
        {
            Paso(_defensa, _defensa.Desarmar()),
            Paso(_seguridad, _seguridad.SellarEstacion()),
            Paso(_comunicaciones, _comunicaciones.CerrarEnlace()),
            Paso(_navegacion, _navegacion.ApagarPropulsores()),
            Paso(_soporteVital, _soporteVital.ModoMinimo()),
            Paso(_energia, _energia.Apagar()),
        };

        Modo = ModoEstacion.Apagada;
        VecesApagada++;
        return Cerrar(secuencia, "Estación apagada.", TipoEntrada.Info);
    }

    private static EntradaBitacora Paso(ISubsistema subsistema, string accion)
    {
        var tipo = subsistema.Estado switch
        {
            EstadoOperativo.Operativo => TipoEntrada.Exito,
            EstadoOperativo.Alerta => TipoEntrada.Advertencia,
            _ => TipoEntrada.Info,
        };
        return new EntradaBitacora(subsistema.Nombre, accion, tipo);
    }

    private List<EntradaBitacora> Cerrar(List<EntradaBitacora> secuencia, string resumen, TipoEntrada tipo)
    {
        secuencia.Add(new EntradaBitacora(Origen, resumen, tipo));
        _bitacoraEstacion?.Registrar(Origen, resumen, tipo);
        return secuencia;
    }
}
