using System.Drawing.Drawing2D;
using Kepler7.Core.Combate;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Comportamiento.State;

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

    // Los estados que se explican debajo del campo. El normal no lleva marca.
    private static readonly IEstadoPersonaje[] Leyenda = [new Envenenado(), new Aturdido(), new Berserk()];

    private readonly Oleada _oleada;
    private readonly Dictionary<Unidad, Rectangle> _zonas = [];
    private readonly System.Windows.Forms.Timer _parpadeo = new() { Interval = 450 };
    private readonly System.Windows.Forms.Timer _flotantes = new() { Interval = 30 };
    private readonly List<NumeroFlotante> _numeros = [];
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
        _flotantes.Tick += (_, _) => AvanzarFlotantes();
    }

    public event EventHandler<Unidad>? UnidadClickeada;

    public void Mostrar(Unidad? seleccionada, IReadOnlyCollection<Unidad> objetivos)
    {
        _seleccionada = seleccionada;
        _objetivos = objetivos;
        Invalidate();
    }

    /// <summary>Pinta de rojo por un momento a la unidad que recibió un golpe y, si perdió vida, lo muestra como un número que sube.</summary>
    public void MarcarGolpe(Unidad unidad, int vidaPerdida = 0)
    {
        _golpeada = unidad;
        _parpadeo.Stop();
        _parpadeo.Start();
        if (vidaPerdida > 0)
            MostrarNumero(unidad, $"-{vidaPerdida}", Tema.Peligro);
        else if (vidaPerdida == 0 && _zonas.ContainsKey(unidad))
            MostrarNumero(unidad, "bloqueado", Tema.TextoTenue);
        Invalidate();
    }

    /// <summary>Un número que flota sobre la tarjeta y se desvanece: daño, curación o estado.</summary>
    public void MostrarNumero(Unidad unidad, string texto, Color color)
    {
        if (!_zonas.TryGetValue(unidad, out var zona))
            return;

        _numeros.Add(new NumeroFlotante(texto, color, new Point(zona.X + zona.Width / 2, zona.Y + 30)));
        _flotantes.Start();
    }

    private void AvanzarFlotantes()
    {
        foreach (var numero in _numeros)
            numero.Avanzar();
        _numeros.RemoveAll(n => n.Terminado);
        if (_numeros.Count == 0)
            _flotantes.Stop();
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

        var abajo = arriba + altoColumna;
        if (Height - abajo >= 34)
            DibujarLeyenda(g, abajo + 12);

        foreach (var numero in _numeros)
            numero.Dibujar(g);
    }

    private void DibujarLeyenda(Graphics g, int y)
    {
        var textos = Leyenda.Select(e => $"{e.Nombre}: {e.Efecto}").ToList();
        var anchos = textos.Select(t => TextRenderer.MeasureText(g, t, Tema.Chica).Width).ToList();
        var x = (Width - anchos.Sum() - Leyenda.Length * 16 - (Leyenda.Length - 1) * 20) / 2;

        for (var i = 0; i < Leyenda.Length; i++)
        {
            var color = ColorTranslator.FromHtml(Leyenda[i].Color);
            IconoEstado(g, Leyenda[i], new Rectangle(x, y + 3, 11, 11), color);
            TextRenderer.DrawText(g, textos[i], Tema.Chica, new Point(x + 16, y), color);
            x += 16 + anchos[i] + 20;
        }
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
        var estado = unidad.Estado;
        var colorEstado = ColorTranslator.FromHtml(estado.Color);

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

        // El estado (State) se ve en la tarjeta: franja de color, ícono y cómo quedó el ataque.
        if (estado is Normal)
        {
            TextRenderer.DrawText(g, Detalle(unidad), Tema.Chica, new Point(zona.X + 10, zona.Y + 29), Tema.TextoTenue);
        }
        else
        {
            using (var franja = new SolidBrush(colorEstado))
                g.FillRectangle(franja, zona.X + 1, zona.Y + 1, 4, zona.Height - 1);
            IconoEstado(g, estado, new Rectangle(zona.X + 12, zona.Y + 32, 11, 11), colorEstado);
            TextRenderer.DrawText(g, DetalleEstado(unidad), Tema.Chica, new Point(zona.X + 26, zona.Y + 29), colorEstado);
        }

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
        else if (unidad is UnidadEscuadron && !estado.PuedeActuar)
            TextRenderer.DrawText(g, "PIERDE EL TURNO", Tema.Chica, new Rectangle(zona.X, zona.Y + 64, zona.Width - 10, 18),
                colorEstado, TextFormatFlags.Right);
        // La IA de un enemigo no se ve hasta que se la revela (por ejemplo, al hackearlo).
        if (unidad is Enemigo enemigo)
            TextRenderer.DrawText(g, enemigo.IARevelada ? $"IA {enemigo.Estrategia.Nombre}" : "IA ???", Tema.Chica,
                new Rectangle(zona.X, zona.Y + 64, zona.Width - 10, 18),
                enemigo.IARevelada ? Tema.Acento : Tema.TextoTenue, TextFormatFlags.Right);
    }

    private static string Detalle(Unidad unidad) => unidad switch
    {
        UnidadEscuadron u => $"{NombresVisibles.De(u.Clase)} · atq {u.Ataque}",
        Barricada => "No ataca, cubre",
        _ => $"Atq {unidad.Ataque}" + (unidad.AtacaADistancia ? " · a distancia" : ""),
    };

    private static string DetalleEstado(Unidad unidad) => unidad.Estado.PuedeActuar
        ? $"{unidad.Estado.Nombre} · atq {unidad.DanioDeAtaque()}"
        : $"{unidad.Estado.Nombre} · no actúa";

    // Un ícono por estado, dibujado a mano: gota para el veneno, destello para el aturdimiento y punta para el berserk.
    private static void IconoEstado(Graphics g, IEstadoPersonaje estado, Rectangle zona, Color color)
    {
        using var pincel = new SolidBrush(color);
        using var lapiz = new Pen(color, 2f);
        var medioX = zona.X + zona.Width / 2;
        var medioY = zona.Y + zona.Height / 2;

        switch (estado)
        {
            case Envenenado:
                g.FillEllipse(pincel, zona.X + 1, zona.Y + 3, zona.Width - 2, zona.Height - 3);
                g.FillPolygon(pincel, new Point[] { new Point(medioX, zona.Y - 2), new Point(zona.X + 1, zona.Y + 7), new Point(zona.Right - 1, zona.Y + 7) });
                break;
            case Aturdido:
                g.DrawLine(lapiz, zona.Left + 1, zona.Top + 1, zona.Right - 1, zona.Bottom - 1);
                g.DrawLine(lapiz, zona.Left + 1, zona.Bottom - 1, zona.Right - 1, zona.Top + 1);
                g.DrawLine(lapiz, medioX, zona.Top - 1, medioX, zona.Bottom + 1);
                g.DrawLine(lapiz, zona.Left - 1, medioY, zona.Right + 1, medioY);
                break;
            default:
                g.FillPolygon(pincel, new Point[] { new Point(medioX, zona.Y - 1), new Point(zona.X, zona.Bottom), new Point(zona.Right, zona.Bottom) });
                break;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _parpadeo.Dispose();
            _flotantes.Dispose();
        }
        base.Dispose(disposing);
    }

    // Sube unos píxeles por cuadro y se va apagando; dura unos 25 cuadros.
    private sealed class NumeroFlotante(string texto, Color color, Point origen)
    {
        private const int Cuadros = 26;
        private int _cuadro;

        public bool Terminado => _cuadro >= Cuadros;

        public void Avanzar() => _cuadro++;

        public void Dibujar(Graphics g)
        {
            var alfa = 255 * (Cuadros - _cuadro) / Cuadros;
            var posicion = new Point(origen.X, origen.Y - _cuadro * 2);
            var tamano = TextRenderer.MeasureText(g, texto, Tema.Destacado);
            var rectangulo = new Rectangle(posicion.X - tamano.Width / 2, posicion.Y - tamano.Height / 2, tamano.Width, tamano.Height);
            TextRenderer.DrawText(g, texto, Tema.Destacado, rectangulo, Color.FromArgb(alfa, color), TextFormatFlags.NoPadding);
        }
    }
}
