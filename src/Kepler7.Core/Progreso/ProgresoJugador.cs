namespace Kepler7.Core.Progreso;

/// <summary>Todo lo que se guarda entre partidas.</summary>
public class ProgresoJugador
{
    public const int XpPorNivel = 100;
    public const int OleadasTotales = 6;

    private readonly HashSet<Patron> _insignias = [];

    public int Xp { get; private set; }
    public int Nivel => 1 + Xp / XpPorNivel;
    public int XpEnNivelActual => Xp % XpPorNivel;
    public int Creditos { get; private set; }
    public int OleadasSuperadas { get; private set; }
    public bool EstacionSalvada => OleadasSuperadas == OleadasTotales;
    public IReadOnlyCollection<Patron> Insignias => _insignias;

    /// <summary>Para reconstruir un progreso leído de la base.</summary>
    public static ProgresoJugador Restaurar(int xp, int creditos, int oleadasSuperadas, IEnumerable<Patron> insignias)
    {
        var progreso = new ProgresoJugador
        {
            Xp = xp,
            Creditos = creditos,
            OleadasSuperadas = oleadasSuperadas,
        };
        progreso._insignias.UnionWith(insignias);
        return progreso;
    }

    /// <returns>true si con esta XP subió de nivel.</returns>
    public bool GanarXp(int cantidad)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        var nivelAnterior = Nivel;
        Xp += cantidad;
        return Nivel > nivelAnterior;
    }

    public void GanarCreditos(int cantidad)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        Creditos += cantidad;
    }

    /// <returns>false si no alcanzan los créditos (y no descuenta nada).</returns>
    public bool GastarCreditos(int cantidad)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        if (cantidad > Creditos)
            return false;

        Creditos -= cantidad;
        return true;
    }

    public void SuperarOleada()
    {
        if (OleadasSuperadas < OleadasTotales)
            OleadasSuperadas++;
    }

    /// <returns>true si es una insignia nueva.</returns>
    public bool DesbloquearInsignia(Patron patron) => _insignias.Add(patron);

    public bool TieneInsignia(Patron patron) => _insignias.Contains(patron);
}
