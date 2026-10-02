using Kepler7.BE;

namespace Kepler7.Data.Memoria;

/// <summary>Guarda el progreso solo mientras el juego está abierto. Se reemplaza por la versión con SQL Server.</summary>
public class RepositorioProgresoEnMemoria : IRepositorioProgreso
{
    private ProgresoJugador? _guardado;

    public ProgresoJugador? Cargar() => _guardado is null ? null : Copiar(_guardado);

    // Se guarda una copia para que se comporte como una base: cambiar la partida no toca lo guardado.
    public void Guardar(ProgresoJugador progreso) => _guardado = Copiar(progreso);

    public void Borrar() => _guardado = null;

    private static ProgresoJugador Copiar(ProgresoJugador p) =>
        ProgresoJugador.Restaurar(p.Xp, p.Creditos, p.OleadasSuperadas, p.Insignias);
}
