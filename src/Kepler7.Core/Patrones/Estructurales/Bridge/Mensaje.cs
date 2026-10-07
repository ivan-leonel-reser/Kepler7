namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>
/// La abstracción del puente: un mensaje para la Tierra. Cada tipo arma su encabezado y pide un refuerzo distinto;
/// cómo se cifra lo delega en el <see cref="ICifrado"/> que tenga puesto, que se puede cambiar en cualquier momento.
/// </summary>
public abstract class Mensaje
{
    protected Mensaje(string contenido, ICifrado cifrado)
    {
        Contenido = contenido.Trim();
        Cifrado = cifrado;
    }

    public string Contenido { get; }

    public ICifrado Cifrado { get; set; }

    public abstract string Tipo { get; }

    public abstract string Prioridad { get; }

    /// <summary>Lo que la Tierra manda si recibe este mensaje.</summary>
    public abstract Refuerzo Refuerzo { get; }

    protected abstract string Encabezado { get; }

    public string TextoPlano => $"{Encabezado} {Contenido}".Trim();

    public string TextoCifrado => Cifrado.Cifrar(TextoPlano);

    public Transmision Enviar() =>
        new(Tipo, Cifrado.Nombre, TextoPlano, TextoCifrado, Cifrado.Costo, Cifrado.RiesgoIntercepcion, Refuerzo);
}
