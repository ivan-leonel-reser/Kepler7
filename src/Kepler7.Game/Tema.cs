using Kepler7.Core.Comun;

namespace Kepler7.Game;

/// <summary>Colores y fuentes de la estación, para que todas las pantallas se vean igual.</summary>
internal static class Tema
{
    public static readonly Color Fondo = Color.FromArgb(9, 13, 22);
    public static readonly Color Panel = Color.FromArgb(18, 26, 41);
    public static readonly Color PanelResaltado = Color.FromArgb(30, 46, 70);
    public static readonly Color Borde = Color.FromArgb(42, 60, 90);
    public static readonly Color Texto = Color.FromArgb(222, 231, 242);
    public static readonly Color TextoTenue = Color.FromArgb(122, 142, 168);
    public static readonly Color Acento = Color.FromArgb(0, 214, 178);
    public static readonly Color Exito = Color.FromArgb(94, 226, 120);
    public static readonly Color Advertencia = Color.FromArgb(255, 186, 64);
    public static readonly Color Peligro = Color.FromArgb(255, 84, 84);

    public static readonly Font Titulo = new("Segoe UI", 30, FontStyle.Bold);
    public static readonly Font Subtitulo = new("Segoe UI", 13);
    public static readonly Font Encabezado = new("Segoe UI", 18, FontStyle.Bold);
    public static readonly Font Normal = new("Segoe UI", 10);
    public static readonly Font Negrita = new("Segoe UI", 10, FontStyle.Bold);
    public static readonly Font Chica = new("Segoe UI", 9);
    public static readonly Font Consola = new("Consolas", 10);

    public static Color ColorDe(TipoEntrada tipo) => tipo switch
    {
        TipoEntrada.Exito => Exito,
        TipoEntrada.Advertencia => Advertencia,
        TipoEntrada.Peligro => Peligro,
        _ => Texto,
    };

    public static Button CrearBoton(string texto, int ancho = 220)
    {
        var boton = new Button
        {
            Text = texto,
            Width = ancho,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = Panel,
            ForeColor = Texto,
            Font = Negrita,
            Cursor = Cursors.Hand,
            Margin = new Padding(4),
        };
        boton.FlatAppearance.BorderColor = Acento;
        boton.FlatAppearance.MouseOverBackColor = PanelResaltado;
        boton.FlatAppearance.MouseDownBackColor = Borde;
        boton.EnabledChanged += (_, _) =>
            boton.FlatAppearance.BorderColor = boton.Enabled ? Acento : Borde;

        // Windows pinta el texto deshabilitado casi negro y en fondo oscuro no se lee.
        boton.Paint += (_, e) =>
        {
            if (boton.Enabled)
                return;
            using var fondo = new SolidBrush(boton.BackColor);
            e.Graphics.FillRectangle(fondo, 1, 1, boton.Width - 2, boton.Height - 2);
            TextRenderer.DrawText(e.Graphics, boton.Text, boton.Font, boton.ClientRectangle, TextoTenue,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        };
        return boton;
    }

    public static Label CrearEtiqueta(string texto, Font? fuente = null, Color? color = null) => new()
    {
        Text = texto,
        AutoSize = true,
        Font = fuente ?? Normal,
        ForeColor = color ?? Texto,
        BackColor = Color.Transparent,
    };
}
