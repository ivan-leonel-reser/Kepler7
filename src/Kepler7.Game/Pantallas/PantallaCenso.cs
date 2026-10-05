using System.Globalization;
using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Estructurales.Flyweight;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>
/// El padrón de la estación. Muestra los contadores de la fábrica de especies y, del tripulante
/// elegido, qué es propio y qué comparte con el resto de su especie. Desde acá se recluta.
/// </summary>
internal class PantallaCenso : PantallaModulo
{
    private static readonly CultureInfo Formato = CultureInfo.GetCultureInfo("es-AR");
    private static readonly string[] Propio = ["Identificador", "Nombre", "Rango"];
    private static readonly string[] Compartido = ["Especie", "Planeta de origen", "Dieta", "Bonus al reclutar", "Comparten el objeto"];

    private readonly ListBox _padron = new()
    {
        Dock = DockStyle.Fill,
        DrawMode = DrawMode.OwnerDrawFixed,
        ItemHeight = 20,
        IntegralHeight = false,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(5, 9, 16),
        ForeColor = Tema.Texto,
        Font = Tema.Normal,
    };
    private readonly Label _tituloPadron = Rotulo("PADRÓN DE KEPLER-7");
    private readonly List<Button> _filtros = [];
    private readonly Label _tripulantes = Tema.CrearEtiqueta("-", Tema.Encabezado);
    private readonly Label _pedidos = Tema.CrearEtiqueta("-", Tema.Encabezado);
    private readonly Label _objetos = Tema.CrearEtiqueta("-", Tema.Encabezado, Tema.Acento);
    private readonly Label _comparacion = new()
    {
        Dock = DockStyle.Top,
        Height = 64,
        Font = Tema.Normal,
        ForeColor = Tema.Texto,
        Text = "Generando el padrón...",
    };
    private readonly Dictionary<string, Label> _ficha = [];
    private readonly Button _reclutar = Tema.CrearBoton("Reclutar", 240);
    private readonly Label _estado = Tema.CrearEtiqueta("", Tema.Normal, Tema.TextoTenue);
    private readonly ConsolaBitacora _consola = new() { Dock = DockStyle.Fill };

    public PantallaCenso(FormPrincipal principal)
        : base(principal, "Censo", Patron.Flyweight,
            "10.000 tripulantes y solo 5 especies. Elegí a quién reclutar: su especie le da un bonus.")
    {
        Contenido.Controls.Add(ArmarDetalle());
        Contenido.Controls.Add(ArmarPadron());

        _padron.DrawItem += DibujarTripulante;
        _padron.SelectedIndexChanged += (_, _) => MostrarElegido();
        _reclutar.Click += (_, _) => Reclutar();

        MostrarElegido();
        Cargar();
    }

    private Control ArmarPadron()
    {
        var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Margin = Padding.Empty };
        foreach (var especie in new string?[] { null }.Concat(FabricaEspecies.Nombres))
        {
            var boton = Tema.CrearBoton(especie ?? "Todas", 78);
            boton.Height = 30;
            boton.Font = Tema.Chica;
            boton.Enabled = false;
            boton.Click += (_, _) => Filtrar(especie, boton);
            _filtros.Add(boton);
            filtros.Controls.Add(boton);
        }

        var columna = new Panel { Dock = DockStyle.Left, Width = 520 };
        columna.Controls.Add(_padron);
        columna.Controls.Add(filtros);
        columna.Controls.Add(_tituloPadron);
        return columna;
    }

    private Control ArmarDetalle()
    {
        var contadores = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 62, WrapContents = false };
        contadores.Controls.AddRange(
        [
            Dato("TRIPULANTES", _tripulantes),
            Dato("PEDIDOS A LA FÁBRICA", _pedidos),
            Dato("OBJETOS DE ESPECIE CREADOS", _objetos),
        ]);

        _estado.Margin = new Padding(12, 6, 0, 0);
        _estado.MaximumSize = new Size(330, 0);
        var zonaReclutar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(0, 6, 0, 0), WrapContents = false };
        zonaReclutar.Controls.AddRange([_reclutar, _estado]);

        var zona = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0) };
        zona.Controls.Add(_consola);
        zona.Controls.Add(zonaReclutar);
        zona.Controls.Add(Tabla(Compartido));
        zona.Controls.Add(Rotulo("LO DE SU ESPECIE · ESTADO INTRÍNSECO, COMPARTIDO"));
        zona.Controls.Add(Tabla(Propio));
        zona.Controls.Add(Rotulo("LO PROPIO DEL TRIPULANTE · ESTADO EXTRÍNSECO"));
        zona.Controls.Add(_comparacion);
        zona.Controls.Add(contadores);
        zona.Controls.Add(Rotulo("FLYWEIGHT · LA FÁBRICA DE ESPECIES"));
        return zona;
    }

    private Control Tabla(string[] campos)
    {
        var tabla = new TableLayoutPanel { Dock = DockStyle.Top, Height = campos.Length * 24 + 6, ColumnCount = 2 };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var campo in campos)
        {
            var valor = Tema.CrearEtiqueta("-");
            valor.Margin = new Padding(3, 3, 3, 3);
            _ficha[campo] = valor;

            var nombre = Tema.CrearEtiqueta(campo, Tema.Normal, Tema.TextoTenue);
            nombre.Margin = new Padding(3, 3, 3, 3);
            tabla.Controls.Add(nombre);
            tabla.Controls.Add(valor);
        }
        return tabla;
    }

    // Son 10.000 tripulantes: se generan fuera del hilo de la ventana para que no se trabe.
    private async void Cargar()
    {
        if (!Partida.Censo.Generado)
            await Task.Run(Partida.GenerarCenso);
        if (IsDisposed)
            return;

        var censo = Partida.Censo;
        _tripulantes.Text = Numero(censo.Tripulantes.Count);
        _pedidos.Text = Numero(censo.Fabrica.Pedidos);
        _objetos.Text = Numero(censo.Fabrica.EspeciesCreadas);
        if (censo.Comparacion is { } c)
        {
            _comparacion.Text =
                $"Con Flyweight: {Numero(c.EspeciesConFlyweight)} objetos de especie · {Kb(c.BytesConFlyweight)} para armar el padrón.\n" +
                $"Sin Flyweight: {Numero(c.EspeciesSinFlyweight)} objetos de especie · {Kb(c.BytesSinFlyweight)}.\n" +
                $"Se ahorran {Kb(c.BytesAhorrados)}: cada tripulante guarda una referencia a su especie, no una copia.";
        }

        foreach (var boton in _filtros)
            boton.Enabled = true;
        Filtrar(null, _filtros[0]);
    }

    private void Filtrar(string? especie, Button elegido)
    {
        var tripulantes = Partida.Censo.Tripulantes.Where(t => especie is null || t.Especie.Nombre == especie).ToArray<object>();

        _padron.BeginUpdate();
        _padron.Items.Clear();
        _padron.Items.AddRange(tripulantes);
        _padron.EndUpdate();

        foreach (var boton in _filtros)
            boton.BackColor = boton == elegido ? Tema.PanelResaltado : Tema.Panel;
        _tituloPadron.Text = especie is null
            ? $"PADRÓN DE KEPLER-7 · {Numero(tripulantes.Length)} TRIPULANTES"
            : $"PADRÓN DE KEPLER-7 · {Numero(tripulantes.Length)} DE LA ESPECIE {especie.ToUpperInvariant()}";
        MostrarElegido();
    }

    private void MostrarElegido()
    {
        var elegido = _padron.SelectedItem as Tripulante;
        string[] valores = elegido is null
            ? ["-", "-", "-", "-", "-", "-", "-", "-"]
            :
            [
                $"#{elegido.Id:00000}",
                elegido.Nombre,
                elegido.Rango,
                elegido.Especie.Nombre,
                elegido.Especie.PlanetaOrigen,
                elegido.Especie.Dieta,
                $"+{elegido.Especie.BonusVida} de vida · +{elegido.Especie.BonusAtaque} de ataque",
                $"{Numero(Partida.Censo.CantidadDe(elegido.Especie))} tripulantes",
            ];

        var campos = Propio.Concat(Compartido).ToArray();
        for (var i = 0; i < campos.Length; i++)
            _ficha[campos[i]].Text = valores[i];
        _ficha["Especie"].ForeColor = elegido is null ? Tema.Texto : ColorTranslator.FromHtml(elegido.Especie.Color);

        _reclutar.Text = $"Reclutar ({Partida.CostoRecluta} créditos)";
        _reclutar.Enabled = elegido is not null && elegido != Partida.Recluta && Partida.PuedeReclutar;
        _estado.Text = TextoEstado(elegido);
    }

    private string TextoEstado(Tripulante? elegido)
    {
        var creditos = $"Tenés {Partida.Progreso.Creditos} créditos.";
        var detalle = elegido switch
        {
            not null when elegido == Partida.Recluta => $"{elegido.Nombre} ya está en el escuadrón.",
            _ when !Partida.PuedeReclutar => "No alcanzan para reclutar.",
            null => "Elegí a un tripulante del padrón.",
            _ when Partida.Recluta is { } actual => $"Reemplaza a {actual.Nombre} en el escuadrón.",
            _ => "Entra al escuadrón como soldado.",
        };
        return creditos + Environment.NewLine + detalle;
    }

    private void Reclutar()
    {
        if (_padron.SelectedItem is not Tripulante elegido)
            return;

        var unidad = Partida.Reclutar(elegido);
        _consola.Agregar(new EntradaBitacora("Censo",
            $"{elegido.Nombre} entra al escuadrón: vida {unidad.VidaMaxima}, ataque {unidad.Ataque}.",
            TipoEntrada.Exito));
        foreach (var insignia in Partida.RevisarLogros())
            _consola.Agregar(new EntradaBitacora("Logros", $"Insignia: {insignia.Nombre}", TipoEntrada.Exito));
        MostrarElegido();
    }

    private void DibujarTripulante(object? sender, DrawItemEventArgs e)
    {
        var elegido = (e.State & DrawItemState.Selected) != 0;
        using (var fondo = new SolidBrush(elegido ? Tema.PanelResaltado : _padron.BackColor))
            e.Graphics.FillRectangle(fondo, e.Bounds);

        if (e.Index < 0 || _padron.Items[e.Index] is not Tripulante tripulante)
            return;

        // El color sale del objeto de especie: es el mismo para todos los que la comparten.
        var color = ColorTranslator.FromHtml(tripulante.Especie.Color);
        using (var muestra = new SolidBrush(color))
            e.Graphics.FillRectangle(muestra, e.Bounds.X + 6, e.Bounds.Y + 5, 10, 10);
        TextRenderer.DrawText(e.Graphics, $"#{tripulante.Id:00000}   {tripulante.Nombre} · {tripulante.Rango}", _padron.Font,
            new Point(e.Bounds.X + 24, e.Bounds.Y + 1), Tema.Texto);
        TextRenderer.DrawText(e.Graphics, tripulante.Especie.Nombre, _padron.Font,
            new Rectangle(e.Bounds.X, e.Bounds.Y + 1, e.Bounds.Width - 8, e.Bounds.Height), color, TextFormatFlags.Right);
    }

    private static Control Dato(string nombre, Label valor)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 28, 0),
        };
        panel.Controls.Add(Tema.CrearEtiqueta(nombre, Tema.Chica, Tema.TextoTenue));
        panel.Controls.Add(valor);
        return panel;
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

    private static string Numero(int valor) => valor.ToString("N0", Formato);

    private static string Kb(long bytes) => $"{(bytes / 1024.0).ToString("N0", Formato)} KB";
}
