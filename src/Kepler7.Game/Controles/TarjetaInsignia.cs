using Kepler7.BE;

namespace Kepler7.Game.Controles;

/// <summary>Una insignia del panel de logros: dominada o pendiente.</summary>
internal class TarjetaInsignia : Control
{
    private readonly Insignia _insignia;
    private readonly bool _obtenida;

    public TarjetaInsignia(Insignia insignia, bool obtenida)
    {
        _insignia = insignia;
        _obtenida = obtenida;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);
        Margin = new Padding(8);
        Dock = DockStyle.Fill;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var area = new Rectangle(0, 0, Width - 1, Height - 1);
        var color = _obtenida ? Tema.Exito : Tema.TextoTenue;

        using (var fondo = new SolidBrush(_obtenida ? Tema.PanelResaltado : Tema.Panel))
            g.FillRectangle(fondo, area);
        using (var borde = new Pen(_obtenida ? Tema.Exito : Tema.Borde, _obtenida ? 2 : 1))
            g.DrawRectangle(borde, area);

        TextRenderer.DrawText(g, _insignia.Nombre, Tema.Negrita, new Rectangle(14, 12, Width - 28, 24),
            _obtenida ? Tema.Texto : Tema.TextoTenue, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g, $"{ModulosEstacion.NombrePatron(_insignia.Patron).ToUpperInvariant()} · {_insignia.Familia.ToString().ToUpperInvariant()}",
            Tema.Chica, new Rectangle(16, 38, Width - 32, 16), _obtenida ? Tema.Acento : Tema.TextoTenue, TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g, _insignia.Descripcion, Tema.Normal,
            new Rectangle(16, 60, Width - 32, Height - 92), _obtenida ? Tema.Texto : Tema.TextoTenue, TextFormatFlags.WordBreak);

        // La medalla: un círculo lleno si está, vacío si falta.
        var medalla = new Rectangle(16, Height - 28, 16, 16);
        using (var lapiz = new Pen(color, 2))
            g.DrawEllipse(lapiz, medalla);
        if (_obtenida)
        {
            using var relleno = new SolidBrush(Tema.Exito);
            g.FillEllipse(relleno, Rectangle.Inflate(medalla, -4, -4));
        }
        TextRenderer.DrawText(g, _obtenida ? "DOMINADO" : "PENDIENTE", Tema.Chica,
            new Rectangle(16, Height - 28, Width - 32, 18), color, TextFormatFlags.Right);
    }
}
