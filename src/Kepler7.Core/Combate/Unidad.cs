namespace Kepler7.Core.Combate;

/// <summary>Cualquier unidad que ocupa una casilla de la grilla, del escuadrón o del Enjambre.</summary>
public abstract class Unidad
{
    protected Unidad(string nombre, Bando bando, int vidaMaxima, int ataque, int movimiento, int alcance)
    {
        if (vidaMaxima <= 0)
            throw new ArgumentOutOfRangeException(nameof(vidaMaxima));

        Nombre = nombre;
        Bando = bando;
        VidaMaxima = vidaMaxima;
        Vida = vidaMaxima;
        Ataque = ataque;
        Movimiento = movimiento;
        Alcance = alcance;
    }

    public string Nombre { get; }
    public Bando Bando { get; }
    public int VidaMaxima { get; }
    public int Vida { get; private set; }
    public int Ataque { get; set; }
    public int Movimiento { get; set; }
    public int Alcance { get; set; }

    // La asigna la Grilla al colocar o mover la unidad.
    public Posicion Posicion { get; internal set; }

    public bool EstaViva => Vida > 0;

    /// <summary>Resta vida sin pasar por las defensas. Devuelve cuánta vida perdió realmente.</summary>
    public int PerderVida(int cantidad)
    {
        var antes = Vida;
        Vida = Math.Max(0, Vida - Math.Max(0, cantidad));
        return antes - Vida;
    }

    /// <summary>Devuelve cuánta vida recuperó realmente (nunca pasa la máxima).</summary>
    public int Curar(int cantidad)
    {
        var antes = Vida;
        Vida = Math.Min(VidaMaxima, Vida + Math.Max(0, cantidad));
        return Vida - antes;
    }

    public override string ToString() => $"{Nombre} {Vida}/{VidaMaxima}";
}
