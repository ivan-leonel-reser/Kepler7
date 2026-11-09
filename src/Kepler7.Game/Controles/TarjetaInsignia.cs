using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Core.Progreso;

namespace Kepler7.Game.Controles;

/// <summary>Una insignia del panel de logros: dominada, o pendiente con lo que le falta.</summary>
internal class TarjetaInsignia : Control
{
    private readonly Insignia _insignia;
    private readonly bool _obtenida;
    private readonly AvanceInsignia _avance;

    public TarjetaInsignia(Insignia insignia, bool obtenida, AvanceInsignia avance)
    {
        _insignia = insignia;
        _obtenida = obtenida;
        _avance = avance;
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
        TextRenderer.DrawText(g, $"{NombresVisibles.De(_insignia.Patron).ToUpperInvariant()} · {_insignia.Familia.ToString().ToUpperInvariant()}",
            Tema.Chica, new Rectangle(16, 38, Width - 32, 16), _obtenida ? Tema.Acento : Tema.TextoTenue, TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g, _insignia.Descripcion, Tema.Normal,
            new Rectangle(16, 60, Width - 32, Height - 104), _obtenida ? Tema.Texto : Tema.TextoTenue, TextFormatFlags.WordBreak);

        if (!_obtenida)
            DibujarAvance(g);

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

    // Una barra finita y el texto "1 de 3 recetas usadas" arriba de la medalla.
    private void DibujarAvance(Graphics g)
    {
        var colorAvance = _avance.Actual > 0 ? Tema.Advertencia : Tema.TextoTenue;
        TextRenderer.DrawText(g, $"{_avance.Actual} de {_avance.Requerido} · {_avance.Detalle}", Tema.Chica,
            new Rectangle(16, Height - 58, Width - 32, 16), colorAvance, TextFormatFlags.EndEllipsis);

        var barra = new Rectangle(16, Height - 38, Width - 32, 4);
        using (var fondo = new SolidBrush(Tema.Borde))
            g.FillRectangle(fondo, barra);
        var ancho = barra.Width * _avance.Actual / _avance.Requerido;
        if (ancho > 0)
        {
            using var relleno = new SolidBrush(Tema.Advertencia);
            g.FillRectangle(relleno, barra with { Width = ancho });
        }
    }
}
