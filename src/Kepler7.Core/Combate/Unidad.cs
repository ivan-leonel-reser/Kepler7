namespace Kepler7.Core.Combate;

/// <summary>Cualquier unidad del campo de batalla, del escuadrón o del Enjambre.</summary>
public abstract class Unidad
{
    protected Unidad(string nombre, Bando bando, int vidaMaxima, int ataque, bool atacaADistancia)
    {
        if (vidaMaxima <= 0)
            throw new ArgumentOutOfRangeException(nameof(vidaMaxima));

        Nombre = nombre;
        Bando = bando;
        VidaMaxima = vidaMaxima;
        Vida = vidaMaxima;
        Ataque = ataque;
        AtacaADistancia = atacaADistancia;
    }

    public string Nombre { get; }
    public Bando Bando { get; }
    public int VidaMaxima { get; }
    public int Vida { get; private set; }
    public int Ataque { get; set; }
    public bool AtacaADistancia { get; }

    // La asigna el CampoBatalla al agregar o mover la unidad.
    public Fila Fila { get; internal set; }

    public bool EstaViva => Vida > 0;

    /// <summary>Cuerpo a cuerpo solo se ataca desde adelante; a distancia, desde cualquier fila.</summary>
    public bool PuedeAtacar => EstaViva && (AtacaADistancia || Fila == Fila.Vanguardia);

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
