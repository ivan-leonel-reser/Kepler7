using Kepler7.Core.Juego;

namespace Kepler7.Game.Controles;

/// <summary>Botón grande del puente de mando para entrar a un módulo.</summary>
internal class TarjetaModulo : Control
{
    private readonly ModuloEstacion _modulo;
    private readonly bool _insigniaObtenida;
    private bool _mouseEncima;

    public TarjetaModulo(ModuloEstacion modulo, bool insigniaObtenida)
    {
        _modulo = modulo;
        _insigniaObtenida = insigniaObtenida;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);
        Cursor = modulo.Disponible ? Cursors.Hand : Cursors.Default;
        Margin = new Padding(8);
        Dock = DockStyle.Fill;
    }

    public event EventHandler<ModuloEstacion>? Abrir;

    protected override void OnMouseEnter(EventArgs e)
    {
        _mouseEncima = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _mouseEncima = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        if (_modulo.Disponible)
            Abrir?.Invoke(this, _modulo);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var area = new Rectangle(0, 0, Width - 1, Height - 1);
        var disponible = _modulo.Disponible;
        var resaltada = disponible && _mouseEncima;

        using (var fondo = new SolidBrush(resaltada ? Tema.PanelResaltado : Tema.Panel))
            g.FillRectangle(fondo, area);
        using (var borde = new Pen(disponible ? Tema.Acento : Tema.Borde, resaltada ? 2 : 1))
            g.DrawRectangle(borde, area);

        var colorTexto = disponible ? Tema.Texto : Tema.TextoTenue;
        TextRenderer.DrawText(g, _modulo.Nombre, Tema.Encabezado, new Point(14, 12), colorTexto);
        TextRenderer.DrawText(g, NombresVisibles.De(_modulo.Patron).ToUpperInvariant(), Tema.Chica,
            new Point(16, 46), disponible ? Tema.Acento : Tema.TextoTenue);
        TextRenderer.DrawText(g, _modulo.Descripcion, Tema.Normal,
            new Rectangle(16, 70, Width - 32, Height - 100), Tema.TextoTenue, TextFormatFlags.WordBreak);

        var (estado, color) = (disponible, _insigniaObtenida) switch
        {
            (_, true) => ("INSIGNIA OBTENIDA", Tema.Exito),
            (true, false) => ("DISPONIBLE", Tema.Acento),
            _ => ("EN DESARROLLO", Tema.TextoTenue),
        };
        TextRenderer.DrawText(g, estado, Tema.Chica, new Rectangle(16, Height - 28, Width - 32, 20), color,
            TextFormatFlags.Right);
    }
}
