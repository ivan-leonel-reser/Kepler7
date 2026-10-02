using System.Drawing.Drawing2D;
using Kepler7.Core.Combate;

namespace Kepler7.Game.Controles;

/// <summary>Dibuja las dos filas de cada bando y avisa qué unidad se clickeó.</summary>
internal class VistaCampo : Control
{
    private const int AnchoMaximo = 176;
    private const int AltoTarjeta = 88;
    private const int Separacion = 14;
    private const int EntreColumnas = 16;
    private const int EntreBandos = 60;
    private const int Margen = 24;

    private readonly Oleada _oleada;
    private readonly Dictionary<Unidad, Rectangle> _zonas = [];
    private readonly System.Windows.Forms.Timer _parpadeo = new() { Interval = 450 };
    private Unidad? _golpeada;
    private Unidad? _seleccionada;
    private IReadOnlyCollection<Unidad> _objetivos = [];

    public VistaCampo(Oleada oleada)
    {
        _oleada = oleada;
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);
        Dock = DockStyle.Fill;
        BackColor = Tema.Fondo;
        _parpadeo.Tick += (_, _) =>
        {
            _parpadeo.Stop();
            _golpeada = null;
            Invalidate();
        };
    }

    public event EventHandler<Unidad>? UnidadClickeada;

    public void Mostrar(Unidad? seleccionada, IReadOnlyCollection<Unidad> objetivos)
    {
        _seleccionada = seleccionada;
        _objetivos = objetivos;
        Invalidate();
    }

    /// <summary>Pinta de rojo por un momento a la unidad que recibió un golpe.</summary>
    public void MarcarGolpe(Unidad unidad)
    {
        _golpeada = unidad;
        _parpadeo.Stop();
        _parpadeo.Start();
        Invalidate();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        foreach (var (unidad, zona) in _zonas)
        {
            if (zona.Contains(e.Location))
            {
                UnidadClickeada?.Invoke(this, unidad);
                return;
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        _zonas.Clear();

        // Las cuatro columnas tienen que entrar en el ancho que haya.
        var ancho = Math.Min(AnchoMaximo, (Width - 2 * Margen - 2 * EntreColumnas - EntreBandos) / 4);
        var centro = Width / 2;
        var xEscuadronAdelante = centro - EntreBandos / 2 - ancho;
        var xEscuadronAtras = xEscuadronAdelante - EntreColumnas - ancho;
        var xEnjambreAdelante = centro + EntreBandos / 2;
        var xEnjambreAtras = xEnjambreAdelante + ancho + EntreColumnas;
        var altoColumna = CampoBatalla.LugaresPorFila * (AltoTarjeta + Separacion) - Separacion;
        var arriba = Math.Max(70, (Height - altoColumna) / 2 + 12);

        using (var divisoria = new Pen(Tema.Borde) { DashStyle = DashStyle.Dash })
            g.DrawLine(divisoria, centro, 20, centro, Height - 20);

        Titulo(g, "ESCUADRÓN", xEscuadronAtras, xEscuadronAdelante + ancho, arriba - 56, Tema.Acento);
        Titulo(g, "ENJAMBRE", xEnjambreAdelante, xEnjambreAtras + ancho, arriba - 56, Tema.Peligro);

        Columna(g, Bando.Escuadron, Fila.Retaguardia, xEscuadronAtras, arriba, ancho);
        Columna(g, Bando.Escuadron, Fila.Vanguardia, xEscuadronAdelante, arriba, ancho);
        Columna(g, Bando.Enjambre, Fila.Vanguardia, xEnjambreAdelante, arriba, ancho);
        Columna(g, Bando.Enjambre, Fila.Retaguardia, xEnjambreAtras, arriba, ancho);
    }

    private static void Titulo(Graphics g, string texto, int desde, int hasta, int y, Color color) =>
        TextRenderer.DrawText(g, texto, Tema.Destacado, new Rectangle(desde, y, hasta - desde, 26), color,
            TextFormatFlags.HorizontalCenter);

    private void Columna(Graphics g, Bando bando, Fila fila, int x, int arriba, int ancho)
    {
        TextRenderer.DrawText(g, fila == Fila.Vanguardia ? "ADELANTE" : "ATRÁS", Tema.Chica,
            new Rectangle(x, arriba - 24, ancho, 18), Tema.TextoTenue, TextFormatFlags.HorizontalCenter);

        var unidades = _oleada.Campo.EnFila(bando, fila).ToList();
        for (var i = 0; i < CampoBatalla.LugaresPorFila; i++)
        {
            var zona = new Rectangle(x, arriba + i * (AltoTarjeta + Separacion), ancho, AltoTarjeta);
            if (i < unidades.Count)
            {
                _zonas[unidades[i]] = zona;
                Tarjeta(g, unidades[i], zona);
            }
            else
            {
                using var vacio = new Pen(Tema.Panel) { DashStyle = DashStyle.Dot };
                g.DrawRectangle(vacio, zona);
            }
        }
    }

    private void Tarjeta(Graphics g, Unidad unidad, Rectangle zona)
    {
        var golpeada = unidad == _golpeada;
        var seleccionada = unidad == _seleccionada;
        var esObjetivo = _objetivos.Contains(unidad);
        var yaActuo = unidad is UnidadEscuadron && _oleada.YaActuo(unidad);

        var fondo = golpeada ? Color.FromArgb(74, 28, 34) : seleccionada ? Tema.PanelResaltado : Tema.Panel;
        var (colorBorde, grosor) = golpeada ? (Tema.Peligro, 3f)
            : seleccionada ? (Tema.Acento, 3f)
            : esObjetivo ? (Tema.Advertencia, 2f)
            : (Tema.Borde, 1f);

        using (var pincel = new SolidBrush(fondo))
            g.FillRectangle(pincel, zona);
        using (var borde = new Pen(colorBorde, grosor))
            g.DrawRectangle(borde, zona);

        var colorNombre = yaActuo ? Tema.TextoTenue : unidad.Bando == Bando.Enjambre ? Tema.Peligro : Tema.Texto;
        TextRenderer.DrawText(g, unidad.Nombre, Tema.Negrita, new Point(zona.X + 10, zona.Y + 8), colorNombre);
        TextRenderer.DrawText(g, Detalle(unidad), Tema.Chica, new Point(zona.X + 10, zona.Y + 29), Tema.TextoTenue);

        var barra = new Rectangle(zona.X + 10, zona.Y + 52, zona.Width - 20, 9);
        var proporcion = unidad.Vida / (double)unidad.VidaMaxima;
        var colorVida = proporcion > 0.5 ? Tema.Exito : proporcion > 0.25 ? Tema.Advertencia : Tema.Peligro;
        using (var vacio = new SolidBrush(Tema.Fondo))
            g.FillRectangle(vacio, barra);
        using (var lleno = new SolidBrush(colorVida))
            g.FillRectangle(lleno, barra.X, barra.Y, (int)(barra.Width * proporcion), barra.Height);

        TextRenderer.DrawText(g, $"{unidad.Vida}/{unidad.VidaMaxima}", Tema.Chica, new Point(zona.X + 10, zona.Y + 64), Tema.Texto);
        if (yaActuo)
            TextRenderer.DrawText(g, "YA ACTUÓ", Tema.Chica, new Rectangle(zona.X, zona.Y + 64, zona.Width - 10, 18),
                Tema.TextoTenue, TextFormatFlags.Right);
        // La IA de un enemigo no se ve hasta que se la revela (por ejemplo, al hackearlo).
        if (unidad is Enemigo enemigo)
            TextRenderer.DrawText(g, enemigo.IARevelada ? $"IA {enemigo.Estrategia.Nombre}" : "IA ???", Tema.Chica,
                new Rectangle(zona.X, zona.Y + 64, zona.Width - 10, 18),
                enemigo.IARevelada ? Tema.Acento : Tema.TextoTenue, TextFormatFlags.Right);
    }

    private static string Detalle(Unidad unidad) => unidad switch
    {
        UnidadEscuadron u => $"{u.Clase} · atq {u.Ataque}",
        Barricada => "No ataca, cubre",
        _ => $"Atq {unidad.Ataque}" + (unidad.AtacaADistancia ? " · a distancia" : ""),
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _parpadeo.Dispose();
        base.Dispose(disposing);
    }
}
