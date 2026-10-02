namespace Kepler7.Game.Controles;

internal class BarraProgreso : Control
{
    private int _valor;
    private int _maximo = 100;

    public BarraProgreso()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);
        Size = new Size(180, 12);
    }

    public Color ColorRelleno { get; set; } = Tema.Acento;

    public int Valor
    {
        get => _valor;
        set { _valor = Math.Clamp(value, 0, _maximo); Invalidate(); }
    }

    public int Maximo
    {
        get => _maximo;
        set { _maximo = Math.Max(1, value); _valor = Math.Min(_valor, _maximo); Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var area = new Rectangle(0, 0, Width - 1, Height - 1);
        using var fondo = new SolidBrush(Tema.Panel);
        using var relleno = new SolidBrush(ColorRelleno);
        using var borde = new Pen(Tema.Borde);

        e.Graphics.FillRectangle(fondo, area);
        var ancho = (int)Math.Round(area.Width * (_valor / (double)_maximo));
        if (ancho > 0)
            e.Graphics.FillRectangle(relleno, new Rectangle(area.X, area.Y, ancho, area.Height));
        e.Graphics.DrawRectangle(borde, area);
    }
}
