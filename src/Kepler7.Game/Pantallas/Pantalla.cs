using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>Base de todas las pantallas: ocupan toda la ventana y conocen al formulario principal.</summary>
internal class Pantalla : UserControl
{
    protected Pantalla(FormPrincipal principal)
    {
        Principal = principal;
        Dock = DockStyle.Fill;
        BackColor = Tema.Fondo;
        ForeColor = Tema.Texto;
        Font = Tema.Normal;
        DoubleBuffered = true;
    }

    protected FormPrincipal Principal { get; }

    /// <summary>Si la pantalla lleva de fondo el cielo de estrellas.</summary>
    protected bool ConCielo { get; set; }

    // Se dibuja en el fondo para que las etiquetas transparentes también lo muestren.
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);
        if (ConCielo)
            Cielo.Dibujar(e.Graphics, ClientSize);
    }

    /// <summary>Mantiene un control centrado cuando cambia el tamaño de la pantalla.</summary>
    protected void MantenerCentrado(Control control)
    {
        void Centrar(object? sender, EventArgs e) =>
            control.Location = new Point(
                Math.Max(0, (ClientSize.Width - control.Width) / 2),
                Math.Max(0, (ClientSize.Height - control.Height) / 2));

        SizeChanged += Centrar;
        control.SizeChanged += Centrar;
        Centrar(null, EventArgs.Empty);
    }
}
