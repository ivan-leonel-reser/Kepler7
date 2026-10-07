using System.Media;

namespace Kepler7.Game;

/// <summary>
/// Efectos cortos generados en memoria (ondas simples en formato WAV), sin archivos externos.
/// Se reproducen con <see cref="SoundPlayer"/>, que no bloquea el hilo de la ventana.
/// </summary>
internal static class Sonidos
{
    private const int Muestras = 22050;
    private static readonly Dictionary<string, SoundPlayer> Cache = [];

    public static bool Activados { get; set; } = true;

    public static void Golpe() => Tocar("golpe", (220, 70), (160, 90));

    public static void Caida() => Tocar("caida", (300, 80), (200, 100), (120, 180));

    public static void Curacion() => Tocar("curacion", (520, 70), (660, 90));

    public static void Insignia() => Tocar("insignia", (523, 90), (659, 90), (784, 160));

    public static void Victoria() => Tocar("victoria", (523, 120), (659, 120), (784, 120), (1046, 260));

    public static void Derrota() => Tocar("derrota", (330, 160), (262, 160), (196, 320));

    private static void Tocar(string nombre, params (int frecuencia, int milisegundos)[] notas)
    {
        if (!Activados)
            return;

        if (!Cache.TryGetValue(nombre, out var reproductor))
        {
            reproductor = new SoundPlayer(Generar(notas));
            Cache[nombre] = reproductor;
        }

        // Sin dispositivo de audio el juego sigue igual, solo que en silencio.
        try
        {
            reproductor.Play();
        }
        catch (Exception)
        {
            Activados = false;
        }
    }

    // Arma un WAV PCM de 8 bits mono con las notas seguidas, con un pequeño fundido para que no haga clic.
    private static MemoryStream Generar((int frecuencia, int milisegundos)[] notas)
    {
        var datos = new List<byte>();
        foreach (var (frecuencia, milisegundos) in notas)
        {
            var total = Muestras * milisegundos / 1000;
            for (var i = 0; i < total; i++)
            {
                var envolvente = Math.Min(1.0, Math.Min(i, total - i) / (Muestras * 0.008));
                var onda = Math.Sin(2 * Math.PI * frecuencia * i / Muestras);
                datos.Add((byte)(128 + 100 * onda * envolvente));
            }
        }

        var flujo = new MemoryStream();
        using (var escritor = new BinaryWriter(flujo, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            escritor.Write("RIFF"u8);
            escritor.Write(36 + datos.Count);
            escritor.Write("WAVE"u8);
            escritor.Write("fmt "u8);
            escritor.Write(16);
            escritor.Write((short)1);
            escritor.Write((short)1);
            escritor.Write(Muestras);
            escritor.Write(Muestras);
            escritor.Write((short)1);
            escritor.Write((short)8);
            escritor.Write("data"u8);
            escritor.Write(datos.Count);
            escritor.Write(datos.ToArray());
        }
        flujo.Position = 0;
        return flujo;
    }
}
