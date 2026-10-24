using System.Drawing.Imaging;

namespace Kepler7.Game.Controles;

/// <summary>Fundido corto entre la pantalla que se va y la que llega. Se pone encima de las dos y se va sola.</summary>
internal class Transicion : Control
{
    private const int Cuadros = 8;

    private readonly Bitmap _anterior;
    private readonly Control _nueva;
    private readonly System.Windows.Forms.Timer _reloj = new() { Interval = 20 };
    private Bitmap? _fotoNueva;
    private int _cuadro;

    public Transicion(Bitmap anterior, Control nueva)
    {
        _anterior = anterior;
        _nueva = nueva;
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        _reloj.Tick += (_, _) => Avanzar();
    }

    /// <summary>Una foto de cómo se ve el control ahora, o null si todavía no se puede dibujar.</summary>
    public static Bitmap? Fotografiar(Control? control)
    {
        if (control is null || control.IsDisposed || !control.IsHandleCreated || control.Width <= 0 || control.Height <= 0)
            return null;

        var foto = new Bitmap(control.Width, control.Height);
        control.DrawToBitmap(foto, new Rectangle(Point.Empty, control.Size));
        return foto;
    }

    public void Empezar() => _reloj.Start();

    public void Terminar()
    {
        _reloj.Stop();
        Parent?.Controls.Remove(this);
        Dispose();
    }

    // La pantalla nueva se fotografía en el primer cuadro: recién ahí terminó de acomodarse.
    private void Avanzar()
    {
        _fotoNueva ??= Fotografiar(_nueva);
        _cuadro++;
        if (_cuadro >= Cuadros || _fotoNueva is null)
            Terminar();
        else
            Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        if (_fotoNueva is not null)
            g.DrawImageUnscaled(_fotoNueva, 0, 0);

        // La pantalla vieja encima, cada vez más transparente.
        var matriz = new ColorMatrix { Matrix33 = 1f - _cuadro / (float)Cuadros };
        using var atributos = new ImageAttributes();
        atributos.SetColorMatrix(matriz);
        g.DrawImage(_anterior, new Rectangle(0, 0, _anterior.Width, _anterior.Height),
            0, 0, _anterior.Width, _anterior.Height, GraphicsUnit.Pixel, atributos);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _reloj.Dispose();
            _anterior.Dispose();
            _fotoNueva?.Dispose();
        }
        base.Dispose(disposing);
    }
}
