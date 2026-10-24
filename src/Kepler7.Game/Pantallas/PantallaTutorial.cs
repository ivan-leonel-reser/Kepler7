using Kepler7.Core.Combate;
using Kepler7.Core.Juego;

namespace Kepler7.Game.Pantallas;

/// <summary>Las tres pantallas de "cómo se juega". El texto viene de Core; acá solo se pagina.</summary>
internal class PantallaTutorial : Pantalla
{
    private readonly Action _alTerminar;
    private readonly Label _paso = Tema.CrearEtiqueta("", Tema.Chica, Tema.TextoTenue);
    private readonly Label _titulo = Tema.CrearEtiqueta("", Tema.Encabezado, Tema.Acento);
    private readonly FlowLayoutPanel _parrafos = new()
    {
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        BackColor = Color.Transparent,
    };
    private readonly Button _anterior = Tema.CrearBoton("Anterior", 160);
    private readonly Button _siguiente = Tema.CrearBoton("Siguiente", 160);
    private readonly Button _saltar = Tema.CrearBoton("Saltar", 160);
    private int _pagina;

    public PantallaTutorial(FormPrincipal principal, Action alTerminar) : base(principal)
    {
        ConCielo = true;
        _alTerminar = alTerminar;

        _titulo.Margin = new Padding(3, 0, 3, 14);
        var botones = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 20, 0, 0), BackColor = Color.Transparent };
        botones.Controls.AddRange([_anterior, _siguiente, _saltar]);
        _anterior.Click += (_, _) => Mostrar(_pagina - 1);
        _siguiente.Click += (_, _) =>
        {
            if (_pagina == Tutorial.Paginas.Count - 1)
                _alTerminar();
            else
                Mostrar(_pagina + 1);
        };
        _saltar.Click += (_, _) => _alTerminar();

        var columna = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
        };
        columna.Controls.AddRange([_paso, _titulo, _parrafos, botones]);

        Controls.Add(columna);
        MantenerCentrado(columna);
        Mostrar(0);
    }

    private void Mostrar(int pagina)
    {
        _pagina = Math.Clamp(pagina, 0, Tutorial.Paginas.Count - 1);
        var actual = Tutorial.Paginas[_pagina];

        _paso.Text = $"CÓMO SE JUEGA · {_pagina + 1} DE {Tutorial.Paginas.Count}";
        _titulo.Text = actual.Titulo;
        _parrafos.SuspendLayout();
        _parrafos.Controls.Clear();
        foreach (var texto in actual.Parrafos)
        {
            var parrafo = Tema.CrearEtiqueta(texto, Tema.Subtitulo, Tema.Texto);
            parrafo.MaximumSize = new Size(820, 0);
            parrafo.Margin = new Padding(3, 0, 3, 12);
            _parrafos.Controls.Add(parrafo);
        }
        // La última página lleva el esquema de las dos filas.
        if (_pagina == Tutorial.Paginas.Count - 1)
            _parrafos.Controls.Add(new EsquemaFilas { Margin = new Padding(3, 6, 3, 0) });
        _parrafos.ResumeLayout();

        _anterior.Enabled = _pagina > 0;
        _siguiente.Text = _pagina == Tutorial.Paginas.Count - 1 ? "Empezar" : "Siguiente";
        _saltar.Visible = _pagina < Tutorial.Paginas.Count - 1;
    }

    // Dibuja las dos filas de cada bando, como se ven en la oleada.
    private sealed class EsquemaFilas : Control
    {
        public EsquemaFilas()
        {
            Size = new Size(820, 96);
            BackColor = Tema.Fondo;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            const int ancho = 92, alto = 26, separacion = 6;
            var columnas = new (int x, string texto, Color color)[]
            {
                (0, "ATRÁS", Tema.Acento),
                (ancho + 16, "ADELANTE", Tema.Acento),
                (2 * ancho + 16 + 80, "ADELANTE", Tema.Peligro),
                (3 * ancho + 32 + 80, "ATRÁS", Tema.Peligro),
            };
            TextRenderer.DrawText(g, "ESCUADRÓN", Tema.Chica, new Rectangle(0, 0, 2 * ancho + 16, 16), Tema.Acento, TextFormatFlags.HorizontalCenter);
            TextRenderer.DrawText(g, "ENJAMBRE", Tema.Chica, new Rectangle(2 * ancho + 96, 0, 2 * ancho + 16, 16), Tema.Peligro, TextFormatFlags.HorizontalCenter);
            using var divisoria = new Pen(Tema.Borde) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            g.DrawLine(divisoria, 2 * ancho + 56, 0, 2 * ancho + 56, Height);

            foreach (var (x, texto, color) in columnas)
            {
                using var lapiz = new Pen(color);
                for (var i = 0; i < CampoBatalla.LugaresPorFila; i++)
                    g.DrawRectangle(lapiz, x, 18 + i * (alto - 10 + separacion), ancho, alto - 10);
                TextRenderer.DrawText(g, texto, Tema.Chica, new Rectangle(x, Height - 16, ancho, 16), Tema.TextoTenue, TextFormatFlags.HorizontalCenter);
            }
        }
    }
}
