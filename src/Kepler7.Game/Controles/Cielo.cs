namespace Kepler7.Game.Controles;

/// <summary>El fondo de las pantallas de inicio, tutorial y cierre: estrellas y el borde del planeta.</summary>
internal static class Cielo
{
    private const int CantidadEstrellas = 170;

    // Semilla fija: el cielo es siempre el mismo y no salta al redibujar.
    private static readonly (float X, float Y, int Brillo, int Tamano)[] Estrellas = Generar();

    public static void Dibujar(Graphics g, Size tamano)
    {
        foreach (var (x, y, brillo, lado) in Estrellas)
        {
            using var pincel = new SolidBrush(Color.FromArgb(brillo, Tema.Texto));
            g.FillRectangle(pincel, x * tamano.Width, y * tamano.Height, lado, lado);
        }

        // Kepler-7 asoma abajo a la derecha, apenas más claro que el fondo.
        var radio = tamano.Height * 9 / 10;
        var centro = new Point(tamano.Width + radio / 7, tamano.Height + radio * 11 / 20);
        var planeta = new Rectangle(centro.X - radio, centro.Y - radio, radio * 2, radio * 2);
        using (var relleno = new SolidBrush(Color.FromArgb(14, 22, 36)))
            g.FillEllipse(relleno, planeta);
        using (var borde = new Pen(Color.FromArgb(90, Tema.Acento), 2f))
            g.DrawEllipse(borde, planeta);
    }

    private static (float, float, int, int)[] Generar()
    {
        var azar = new Random(7);
        return Enumerable.Range(0, CantidadEstrellas)
            .Select(_ => ((float)azar.NextDouble(), (float)azar.NextDouble(), azar.Next(50, 190), azar.Next(10) == 0 ? 2 : 1))
            .ToArray();
    }
}
