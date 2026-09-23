using Kepler7.Core.Comun;
using Kepler7.Core.Patrones.Estructurales.Facade;
using Kepler7.Core.Persistencia;
using Kepler7.Core.Progreso;

namespace Kepler7.Core.Juego;

/// <summary>La partida en curso. Es lo único que la interfaz necesita tener a mano.</summary>
public class Partida
{
    public const int XpPorInsignia = 50;
    public const int CreditosPorInsignia = 100;

    private readonly IRepositorioProgreso _repositorio;

    public Partida(IRepositorioProgreso repositorio, ProgresoJugador progreso, IAleatorio aleatorio)
    {
        _repositorio = repositorio;
        Progreso = progreso;
        Aleatorio = aleatorio;
        ControlCentral = new ControlCentral(Bitacora);
    }

    public ProgresoJugador Progreso { get; }
    public IAleatorio Aleatorio { get; }
    public Bitacora Bitacora { get; } = new();
    public ControlCentral ControlCentral { get; }

    public static Partida Nueva(IRepositorioProgreso repositorio, IAleatorio aleatorio)
    {
        repositorio.Borrar();
        var partida = new Partida(repositorio, new ProgresoJugador(), aleatorio);
        partida.Bitacora.Registrar("Consejo", "El Enjambre infiltró Kepler-7. Asumís el mando de la estación.", TipoEntrada.Advertencia);
        partida.AvisarEstacionApagada();
        partida.Guardar();
        return partida;
    }

    public static Partida Continuar(IRepositorioProgreso repositorio, IAleatorio aleatorio)
    {
        var progreso = repositorio.Cargar()
            ?? throw new InvalidOperationException("No hay una partida guardada.");

        var partida = new Partida(repositorio, progreso, aleatorio);
        partida.Bitacora.Registrar("Consejo", $"Bienvenido de nuevo, Comandante. Nivel {progreso.Nivel}.");
        partida.AvisarEstacionApagada();
        return partida;
    }

    public void Guardar() => _repositorio.Guardar(Progreso);

    /// <summary>Se llama después de cada acción del jugador. Devuelve las insignias que acaba de ganar.</summary>
    public IReadOnlyList<Insignia> RevisarLogros()
    {
        var nuevas = new List<Insignia>();

        if (ControlCentral.TurnosIniciados > 0 && ControlCentral.EmergenciasDeclaradas > 0 && ControlCentral.VecesApagada > 0)
            Sumar(nuevas, OtorgarInsignia(Patron.Facade));

        return nuevas;
    }

    /// <summary>Da la insignia con su recompensa y guarda. Devuelve null si ya la tenía.</summary>
    public Insignia? OtorgarInsignia(Patron patron)
    {
        if (!Progreso.DesbloquearInsignia(patron))
            return null;

        var insignia = CatalogoInsignias.Obtener(patron);
        Progreso.GanarCreditos(CreditosPorInsignia);
        var subioNivel = Progreso.GanarXp(XpPorInsignia);

        Bitacora.Registrar("Logros", $"Insignia desbloqueada: {insignia.Nombre} ({patron}).", TipoEntrada.Exito);
        if (subioNivel)
            Bitacora.Registrar("Logros", $"Subiste a nivel {Progreso.Nivel}.", TipoEntrada.Exito);

        Guardar();
        return insignia;
    }

    private void AvisarEstacionApagada() =>
        Bitacora.Registrar("Control central", "La estación está apagada. Iniciá un turno desde el Control Central.", TipoEntrada.Advertencia);

    private static void Sumar(List<Insignia> nuevas, Insignia? insignia)
    {
        if (insignia is not null)
            nuevas.Add(insignia);
    }
}
