using Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;
using Kepler7.Core.Patrones.Comportamiento.State;

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
        Defensas.Agregar(new Vida(this));
    }

    public string Nombre { get; }
    public Bando Bando { get; }
    public int VidaMaxima { get; }
    public int Vida { get; private set; }
    public int Ataque { get; set; }
    public bool AtacaADistancia { get; }

    // La asigna el CampoBatalla al agregar o mover la unidad.
    public Fila Fila { get; internal set; }

    /// <summary>Las capas que atraviesa cada golpe. Sin equipo, la cadena tiene solo la vida.</summary>
    public CadenaDefensas Defensas { get; } = new();

    /// <summary>El estado en que está la unidad. Cambia cómo ataca, si se mueve y cómo recibe los golpes.</summary>
    public IEstadoPersonaje Estado { get; private set; } = new Normal();

    /// <summary>Lo que le deja a quien golpea, si llega a sacarle vida.</summary>
    public EfectoGolpe EfectoAlGolpear { get; set; }

    /// <summary>Una barricada no se envenena ni se aturde.</summary>
    protected virtual bool AdmiteEstados => true;

    public bool EstaViva => Vida > 0;

    /// <summary>Cuerpo a cuerpo solo se ataca desde adelante; a distancia, desde cualquier fila.</summary>
    public bool PuedeAtacar => EstaViva && (AtacaADistancia || Fila == Fila.Vanguardia);

    public void CambiarEstado(IEstadoPersonaje nuevo)
    {
        if (AdmiteEstados)
            Estado = nuevo;
    }

    /// <summary>El daño con el que pega en este momento. Lo decide su estado (State).</summary>
    public int DanioDeAtaque(int bonus = 0) => Estado.Atacar(this, Ataque + bonus);

    /// <summary>
    /// Todo golpe en combate entra por acá. La unidad se lo pasa a su estado (State),
    /// que lo hace recorrer la cadena de defensas (Chain of Responsibility).
    /// </summary>
    /// <returns>Cuánta vida perdió realmente.</returns>
    public virtual int RecibirGolpe(Danio danio) => Estado.RecibirDanio(this, danio);

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
