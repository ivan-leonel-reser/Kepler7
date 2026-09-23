using System.Drawing.Drawing2D;
using Kepler7.Core.Patrones.Estructurales.Facade;

namespace Kepler7.Game.Controles;

internal class TarjetaSubsistema : Control
{
    private EstadoSubsistema _estado;
    private bool _resaltada;

    public TarjetaSubsistema(EstadoSubsistema estado)
    {
        _estado = estado;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);
        Dock = DockStyle.Fill;
        Margin = new Padding(6);
    }

    public string Nombre => _estado.Nombre;

    public bool Resaltada
    {
        get => _resaltada;
        set
        {
            if (_resaltada == value)
                return;
            _resaltada = value;
            Invalidate();
        }
    }

    public void Mostrar(EstadoSubsistema estado)
    {
        _estado = estado;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var area = new Rectangle(1, 1, Width - 3, Height - 3);
        var color = ColorDe(_estado.Estado);

        using (var fondo = new SolidBrush(_resaltada ? Tema.PanelResaltado : Tema.Panel))
            g.FillRectangle(fondo, area);
        using (var borde = new Pen(_resaltada ? Tema.Acento : color, _resaltada ? 3 : 1))
            g.DrawRectangle(borde, area);
        using (var punto = new SolidBrush(color))
            g.FillEllipse(punto, 16, 20, 11, 11);

        TextRenderer.DrawText(g, _estado.Nombre, Tema.Destacado, new Point(34, 12), Tema.Texto);
        TextRenderer.DrawText(g, TextoDe(_estado.Estado), Tema.Chica, new Point(36, 38), color);
        TextRenderer.DrawText(g, _estado.Detalle, Tema.Normal, new Rectangle(16, 62, Width - 32, Height - 68),
            Tema.TextoTenue, TextFormatFlags.WordBreak);
    }

    private static Color ColorDe(EstadoOperativo estado) => estado switch
    {
        EstadoOperativo.Operativo => Tema.Exito,
        EstadoOperativo.Alerta => Tema.Peligro,
        _ => Tema.TextoTenue,
    };

    private static string TextoDe(EstadoOperativo estado) => estado switch
    {
        EstadoOperativo.Operativo => "OPERATIVO",
        EstadoOperativo.Alerta => "EN ALERTA",
        _ => "APAGADO",
    };
}
