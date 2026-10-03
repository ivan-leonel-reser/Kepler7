using Kepler7.BE;
using Kepler7.Core.Patrones.Creacionales.Builder;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>
/// El jugador elige una receta y la partida le pide la nave al ingeniero jefe.
/// La pantalla solo muestra las piezas en el orden en que se montaron y la ficha de la nave.
/// </summary>
internal class PantallaHangar : PantallaModulo
{
    private static readonly string[] Atributos =
        ["Modelo", "Casco", "Motor", "Escudo", "Armas", "Radar", "IA de a bordo", "Combustible", "Bodega"];

    private readonly List<Button> _botones = [];
    private readonly ConsolaBitacora _consola = new() { Dock = DockStyle.Fill };
    private readonly Queue<EntradaBitacora> _pendientes = new();
    private readonly System.Windows.Forms.Timer _animacion = new() { Interval = 350 };
    private readonly Dictionary<string, Label> _ficha = [];
    private readonly Label _apoyo = Tema.CrearEtiqueta("", Tema.Negrita, Tema.Acento);
    private Nave? _enArmado;

    public PantallaHangar(FormPrincipal principal)
        : base(principal, "Hangar", Patron.Builder,
            "Elegí una receta y el ingeniero jefe arma, paso a paso, la nave de apoyo de la próxima oleada.")
    {
        Contenido.Controls.Add(ArmarConsola());
        Contenido.Controls.Add(ArmarFicha());
        Contenido.Controls.Add(ArmarRecetas());

        _animacion.Tick += (_, _) => MostrarSiguientePieza();
        MostrarFicha(Partida.NaveDeApoyo);
    }

    private Control ArmarRecetas()
    {
        var columna = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 290, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        columna.Controls.Add(Tema.CrearEtiqueta("RECETAS", Tema.Chica, Tema.TextoTenue));

        foreach (var receta in CatalogoRecetas.Crear())
        {
            var boton = Tema.CrearBoton($"Armar nave {receta.Nombre}", 270);
            boton.Click += (_, _) => Ensamblar(receta);
            _botones.Add(boton);

            var descripcion = Tema.CrearEtiqueta(receta.Descripcion, Tema.Normal, Tema.TextoTenue);
            descripcion.MaximumSize = new Size(270, 0);
            descripcion.Margin = new Padding(6, 2, 3, 16);
            columna.Controls.AddRange([boton, descripcion]);
        }

        var aviso = Tema.CrearEtiqueta("En el hangar entra una sola nave. Sale con la próxima oleada y después hay que armar otra.",
            Tema.Chica, Tema.TextoTenue);
        aviso.MaximumSize = new Size(270, 0);
        aviso.Margin = new Padding(6, 8, 3, 0);
        columna.Controls.Add(aviso);
        return columna;
    }

    private Control ArmarConsola()
    {
        var zona = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 0) };
        zona.Controls.Add(_consola);
        zona.Controls.Add(new Label
        {
            Text = "ENSAMBLADO PASO A PASO",
            Dock = DockStyle.Top,
            Height = 22,
            Font = Tema.Chica,
            ForeColor = Tema.TextoTenue,
        });
        return zona;
    }

    private Control ArmarFicha()
    {
        var tabla = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 4, 0, 12) };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 195));
        foreach (var atributo in Atributos)
        {
            var valor = Tema.CrearEtiqueta("-");
            valor.MaximumSize = new Size(189, 0);
            valor.Margin = new Padding(3, 4, 3, 4);
            _ficha[atributo] = valor;

            var nombre = Tema.CrearEtiqueta(atributo, Tema.Normal, Tema.TextoTenue);
            nombre.Margin = new Padding(3, 4, 3, 4);
            tabla.Controls.Add(nombre);
            tabla.Controls.Add(valor);
        }

        _apoyo.MaximumSize = new Size(300, 0);
        var columna = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 310, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        columna.Controls.AddRange(
        [
            Tema.CrearEtiqueta("NAVE EN EL HANGAR", Tema.Chica, Tema.TextoTenue),
            tabla,
            Tema.CrearEtiqueta("APOYO EN LA OLEADA", Tema.Chica, Tema.TextoTenue),
            _apoyo,
        ]);
        return columna;
    }

    private void Ensamblar(INaveBuilder receta)
    {
        _enArmado = Partida.EnsamblarNave(receta);

        _consola.Items.Clear();
        _pendientes.Enqueue(new EntradaBitacora("Ingeniero", $"Arma la receta {receta.Nombre}.", TipoEntrada.Advertencia));
        foreach (var pieza in _enArmado.Piezas)
            _pendientes.Enqueue(new EntradaBitacora(pieza.Componente, pieza.Detalle, TipoEntrada.Exito));
        _pendientes.Enqueue(new EntradaBitacora("Ingeniero", $"Nave {_enArmado.Modelo} lista: {_enArmado.Piezas.Count} piezas."));

        foreach (var boton in _botones)
            boton.Enabled = false;
        MostrarFicha(null);
        _apoyo.Text = "Ensamblando...";
        MostrarSiguientePieza();
        _animacion.Start();
    }

    private void MostrarSiguientePieza()
    {
        if (_pendientes.TryDequeue(out var entrada))
        {
            _consola.Agregar(entrada);
            return;
        }

        _animacion.Stop();
        MostrarFicha(_enArmado);
        foreach (var insignia in Partida.RevisarLogros())
            _consola.Agregar(new EntradaBitacora("Logros", $"Insignia: {insignia.Nombre}", TipoEntrada.Exito));
        foreach (var boton in _botones)
            boton.Enabled = true;
    }

    private void MostrarFicha(Nave? nave)
    {
        string[] valores = nave is null
            ? [.. Atributos.Select(_ => "-")]
            :
            [
                nave.Modelo,
                nave.Casco,
                nave.Motor,
                nave.Escudo > 0 ? $"{nave.Escudo} puntos" : "No lleva",
                nave.Armas > 0 ? $"Potencia de fuego {nave.Armas}" : "No lleva",
                nave.Radar ? "Sí" : "No lleva",
                nave.IA ?? "No lleva",
                $"{nave.Tanques} tanques",
                nave.Bodega > 0 ? $"{nave.Bodega} toneladas" : "No lleva",
            ];
        for (var i = 0; i < Atributos.Length; i++)
            _ficha[Atributos[i]].Text = valores[i];

        _apoyo.Text = nave is null
            ? "El hangar está vacío: la próxima oleada se defiende sin apoyo."
            : $"La nave {nave.Modelo} {nave.Apoyo}.";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _animacion.Dispose();
        base.Dispose(disposing);
    }
}
