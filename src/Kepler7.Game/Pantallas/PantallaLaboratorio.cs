using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Creacionales.Prototype;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>
/// Original y clon lado a lado. La pantalla le pide el clon a la ficha y le pasa los cambios;
/// lo que se ve es que el original queda igual mientras el clon cambia.
/// </summary>
internal class PantallaLaboratorio : PantallaModulo
{
    private static readonly string[] Campos = ["Nombre", "Clase", "Especie", "Sale a pelear con"];

    private readonly ListBox _escuadron = Lista(110);
    private readonly Button _clonar = Tema.CrearBoton("Clonar", 246);
    private readonly Dictionary<string, Label> _fichaOriginal = [];
    private readonly Dictionary<string, Label> _fichaClon = [];
    private readonly ListBox _habilidadesOriginal = Lista(66);
    private readonly ListBox _habilidadesClon = Lista(66);
    private readonly ListBox _catalogo = Lista(92);
    private readonly Button _ensenar = Tema.CrearBoton("Enseñar al clon", 190);
    private readonly Button _quitar = Tema.CrearBoton("Quitar del clon", 190);
    private readonly Button _sumar = Tema.CrearBoton("", 388);
    private readonly Label _prueba = new()
    {
        Dock = DockStyle.Top,
        Height = 120,
        Padding = new Padding(0, 10, 0, 0),
        Font = Tema.Normal,
        ForeColor = Tema.Acento,
    };
    private readonly ConsolaBitacora _consola = new() { Dock = DockStyle.Fill };

    private FichaSoldado? _original;
    private FichaSoldado? _clon;

    public PantallaLaboratorio(FormPrincipal principal)
        : base(principal, "Laboratorio", Patron.Prototype,
            "Cloná a un soldado, enseñale algo nuevo al clon y sumalo al escuadrón. El original no cambia.")
    {
        var centro = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        centro.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        centro.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        centro.Controls.Add(ArmarOriginal(), 0, 0);
        centro.Controls.Add(ArmarClon(), 1, 0);

        Contenido.Controls.Add(centro);
        Contenido.Controls.Add(ArmarEscuadron());
        Contenido.Controls.Add(ArmarConsola());

        _escuadron.SelectedIndexChanged += (_, _) => ElegirOriginal();
        _habilidadesClon.SelectedIndexChanged += (_, _) => ActualizarBotones();
        _catalogo.SelectedIndexChanged += (_, _) => ActualizarBotones();
        _clonar.Click += (_, _) => Clonar();
        _ensenar.Click += (_, _) => Ensenar();
        _quitar.Click += (_, _) => Quitar();
        _sumar.Click += (_, _) => SumarAlEscuadron();

        foreach (var habilidad in CatalogoHabilidades.Entrenables)
            _catalogo.Items.Add(new Renglon(habilidad.Nombre, $"{habilidad.Nombre} · {habilidad.Descripcion}"));
        CargarEscuadron();
        Refrescar();
    }

    private Control ArmarEscuadron()
    {
        var nota = Tema.CrearEtiqueta(
            "El clon sale con los mismos datos que el original. Después se le pueden enseñar habilidades sin tocar al original.",
            Tema.Chica, Tema.TextoTenue);
        nota.MaximumSize = new Size(246, 0);
        nota.Margin = new Padding(4, 10, 3, 0);

        var columna = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 270, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        columna.Controls.AddRange([Seccion("ESCUADRÓN · A QUIÉN CLONAR"), _escuadron, _clonar, nota]);
        return columna;
    }

    private Control ArmarOriginal()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0) };
        panel.Controls.Add(_prueba);
        panel.Controls.Add(Anclar(_habilidadesOriginal));
        panel.Controls.Add(Rotulo("HABILIDADES DEL ORIGINAL"));
        panel.Controls.Add(Tabla(_fichaOriginal));
        panel.Controls.Add(Rotulo("ORIGINAL"));
        return panel;
    }

    private Control ArmarClon()
    {
        var entrenar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Margin = Padding.Empty };
        entrenar.Controls.AddRange([_ensenar, _quitar]);
        var sumar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Margin = Padding.Empty };
        sumar.Controls.Add(_sumar);

        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0) };
        panel.Controls.Add(sumar);
        panel.Controls.Add(entrenar);
        panel.Controls.Add(Anclar(_catalogo));
        panel.Controls.Add(Rotulo("HABILIDADES PARA ENTRENAR"));
        panel.Controls.Add(Anclar(_habilidadesClon));
        panel.Controls.Add(Rotulo("HABILIDADES DEL CLON"));
        panel.Controls.Add(Tabla(_fichaClon));
        panel.Controls.Add(Rotulo("CLON"));
        return panel;
    }

    private Control ArmarConsola()
    {
        var zona = new Panel { Dock = DockStyle.Bottom, Height = 84, Padding = new Padding(0, 6, 0, 0) };
        zona.Controls.Add(_consola);
        return zona;
    }

    private void CargarEscuadron()
    {
        _escuadron.Items.Clear();
        foreach (var unidad in Partida.Clonables)
            _escuadron.Items.Add(new RenglonUnidad(unidad));
    }

    private void ElegirOriginal()
    {
        if (_escuadron.SelectedItem is not RenglonUnidad elegido)
            return;

        _original = Partida.FichaDe(elegido.Unidad);
        _clon = null;
        Refrescar();
    }

    private void Clonar()
    {
        if (_original is null)
            return;

        _clon = (FichaSoldado)_original.Clonar();
        _clon.Nombre = $"{_original.Nombre} II";
        Avisar("Laboratorio", $"Clon de {_original.Nombre} creado: es otra ficha, con su propia lista de habilidades.", TipoEntrada.Exito);
        Refrescar();
    }

    private void Ensenar()
    {
        if (_original is null || _clon is null || _catalogo.SelectedItem is not Renglon elegida)
            return;

        _clon.Aprender(elegida.Nombre);
        Avisar("Clon", $"{_clon.Nombre} aprende {elegida.Texto}.", TipoEntrada.Exito);
        Avisar("Original", $"{_original.Nombre} sigue igual: {string.Join(", ", _original.Habilidades)}.");
        Refrescar();
    }

    private void Quitar()
    {
        if (_original is null || _clon is null || _habilidadesClon.SelectedItem is not Renglon elegida)
            return;

        _clon.Olvidar(elegida.Nombre);
        Avisar("Clon", $"{_clon.Nombre} olvida {elegida.Nombre}.", TipoEntrada.Advertencia);
        Avisar("Original", $"{_original.Nombre} sigue igual: {string.Join(", ", _original.Habilidades)}.");
        Refrescar();
    }

    private void SumarAlEscuadron()
    {
        if (_original is null || _clon is null)
            return;

        var unidad = Partida.SumarClon(_original, _clon);
        Avisar("Laboratorio", $"{unidad.Nombre} entra al escuadrón: vida {unidad.VidaMaxima}, ataque {unidad.Ataque}.", TipoEntrada.Exito);
        foreach (var insignia in Partida.RevisarLogros())
            Avisar("Logros", $"Insignia: {insignia.Nombre}", TipoEntrada.Exito);

        _clon = null;
        Refrescar();
    }

    private void Refrescar()
    {
        MostrarFicha(_fichaOriginal, _habilidadesOriginal, _original);
        MostrarFicha(_fichaClon, _habilidadesClon, _clon);
        _prueba.Text = TextoPrueba();
        _sumar.Text = $"Sumar el clon al escuadrón ({Partida.CostoClon} créditos)";
        ActualizarBotones();
    }

    private void ActualizarBotones()
    {
        _clonar.Enabled = _original is not null;
        _ensenar.Enabled = _clon is not null && _catalogo.SelectedItem is Renglon nueva && _clon.PuedeAprender(nueva.Nombre);
        _quitar.Enabled = _clon is not null && _habilidadesClon.SelectedItem is Renglon sabida && _clon.PuedeOlvidar(sabida.Nombre);
        _sumar.Enabled = _clon is not null && Partida.PuedeClonar;
    }

    private static void MostrarFicha(Dictionary<string, Label> ficha, ListBox habilidades, FichaSoldado? soldado)
    {
        var unidad = soldado?.CrearUnidad();
        string[] valores = soldado is null || unidad is null
            ? ["-", "-", "-", "-"]
            :
            [
                soldado.Nombre,
                NombresVisibles.De(soldado.Clase),
                soldado.Especie?.Nombre ?? "No figura en el censo",
                $"{unidad.VidaMaxima} de vida · {unidad.Ataque} de ataque",
            ];
        for (var i = 0; i < Campos.Length; i++)
            ficha[Campos[i]].Text = valores[i];

        habilidades.Items.Clear();
        foreach (var nombre in soldado?.Habilidades ?? [])
        {
            var detalle = CatalogoHabilidades.Buscar(nombre)?.Descripcion ?? "de su clase";
            habilidades.Items.Add(new Renglon(nombre, $"{nombre} · {detalle}"));
        }
    }

    // La prueba visible: después de tocar al clon, el original conserva sus valores.
    private string TextoPrueba()
    {
        if (_original is null)
            return "Elegí a un soldado del escuadrón para clonarlo.";
        if (_clon is null)
            return $"{_original.Nombre} todavía no tiene un clon en la mesa.";

        var lineas = new List<string>
        {
            $"Habilidades del original: {_original.Habilidades.Count}. Habilidades del clon: {_clon.Habilidades.Count}.",
            ReferenceEquals(_original.Habilidades, _clon.Habilidades)
                ? "Comparten la misma lista de habilidades."
                : "Cada uno tiene su propia lista: es una copia profunda.",
        };
        if (_original.Especie is not null)
            lineas.Add("La especie sí es el mismo objeto: se comparte a propósito.");
        if (!Partida.PuedeClonar)
            lineas.Add($"Tenés {Partida.Progreso.Creditos} créditos: no alcanzan para sumarlo al escuadrón.");
        return string.Join(Environment.NewLine, lineas);
    }

    private void Avisar(string origen, string mensaje, TipoEntrada tipo = TipoEntrada.Info) =>
        _consola.Agregar(new EntradaBitacora(origen, mensaje, tipo));

    private static Control Tabla(Dictionary<string, Label> destino)
    {
        var tabla = new TableLayoutPanel { Dock = DockStyle.Top, Height = Campos.Length * 24 + 6, ColumnCount = 2 };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var campo in Campos)
        {
            var valor = Tema.CrearEtiqueta("-");
            valor.Margin = new Padding(3);
            destino[campo] = valor;

            var nombre = Tema.CrearEtiqueta(campo, Tema.Normal, Tema.TextoTenue);
            nombre.Margin = new Padding(3);
            tabla.Controls.Add(nombre);
            tabla.Controls.Add(valor);
        }
        return tabla;
    }

    // Las listas tienen alto fijo: se apoyan arriba, una debajo de la otra.
    private static Control Anclar(ListBox lista)
    {
        lista.Dock = DockStyle.Top;
        return lista;
    }

    private static Label Rotulo(string texto) => new()
    {
        Text = texto,
        Dock = DockStyle.Top,
        Height = 26,
        TextAlign = ContentAlignment.BottomLeft,
        Font = Tema.Chica,
        ForeColor = Tema.TextoTenue,
    };

    private static Label Seccion(string texto)
    {
        var etiqueta = Tema.CrearEtiqueta(texto, Tema.Chica, Tema.TextoTenue);
        etiqueta.Margin = new Padding(3, 8, 3, 4);
        return etiqueta;
    }

    private static ListBox Lista(int alto) => new()
    {
        Height = alto,
        Width = 246,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(5, 9, 16),
        ForeColor = Tema.Texto,
        Font = Tema.Normal,
        IntegralHeight = false,
        Margin = new Padding(4),
    };

    private sealed record Renglon(string Nombre, string Texto)
    {
        public override string ToString() => Texto;
    }

    private sealed record RenglonUnidad(UnidadEscuadron Unidad)
    {
        public override string ToString() => $"{Unidad.Nombre} · {NombresVisibles.De(Unidad.Clase)}";
    }
}
