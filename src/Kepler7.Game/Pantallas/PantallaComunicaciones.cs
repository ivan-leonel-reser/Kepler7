using Kepler7.BE;
using Kepler7.Core.Patrones.Estructurales.Bridge;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>
/// La antena hacia la Tierra. La pantalla combina el tipo de mensaje con el cifrado que se elija,
/// muestra lo que sale en claro y lo que viaja cifrado, y le pide a la partida que lo transmita.
/// </summary>
internal class PantallaComunicaciones : PantallaModulo
{
    private readonly ListBox _tipos = CrearLista(80);
    private readonly ListBox _cifrados = CrearLista(80);
    private readonly TextBox _contenido = new()
    {
        Width = 330,
        Height = 54,
        Multiline = true,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(5, 9, 16),
        ForeColor = Tema.Texto,
        Font = Tema.Normal,
        Margin = new Padding(4),
    };
    private readonly Label _info = Tema.CrearEtiqueta("", Tema.Chica, Tema.TextoTenue);
    private readonly Label _antena = Tema.CrearEtiqueta("", Tema.Chica, Tema.Advertencia);
    private readonly Button _transmitir = Tema.CrearBoton("Transmitir a la Tierra", 330);
    private readonly TextBox _enClaro = CrearVisor();
    private readonly TextBox _cifrado = CrearVisor();
    private readonly ListBox _historial = CrearLista(120);
    private readonly ConsolaBitacora _consola = new() { Dock = DockStyle.Fill };
    private int _entradasMostradas;

    public PantallaComunicaciones(FormPrincipal principal)
        : base(principal, "Comunicaciones", Patron.Bridge,
            "Pedí refuerzos a la Tierra. El tipo de mensaje y el cifrado se combinan como quieras.")
    {
        _entradasMostradas = Partida.Bitacora.Entradas.Count;

        Contenido.Controls.Add(ArmarAntena());
        Contenido.Controls.Add(ArmarHistorial());
        Contenido.Controls.Add(ArmarRedaccion());

        foreach (var tipo in CatalogoComunicaciones.Tipos)
            _tipos.Items.Add(new RenglonTipo(tipo));
        foreach (var cifrado in CatalogoComunicaciones.Cifrados)
            _cifrados.Items.Add(new RenglonCifrado(cifrado));

        _tipos.SelectedIndexChanged += (_, _) => CambiarTipo();
        _cifrados.SelectedIndexChanged += (_, _) => Previsualizar();
        _contenido.TextChanged += (_, _) => Previsualizar();
        _transmitir.Click += (_, _) => Transmitir();

        _tipos.SelectedIndex = 0;
        _cifrados.SelectedIndex = 0;
        RefrescarHistorial();
    }

    private Control ArmarRedaccion()
    {
        _info.MaximumSize = new Size(330, 0);
        _info.Margin = new Padding(4, 4, 3, 4);
        _antena.MaximumSize = new Size(330, 0);
        _antena.Margin = new Padding(4, 6, 3, 0);

        var columna = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 350, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        columna.Controls.AddRange(
        [
            Seccion("TIPO DE MENSAJE"),
            _tipos,
            Seccion("CIFRADO"),
            _cifrados,
            Seccion("CONTENIDO"),
            _contenido,
            _info,
            _transmitir,
            _antena,
        ]);
        return columna;
    }

    private Control ArmarAntena()
    {
        _enClaro.Height = 72;
        _enClaro.Dock = DockStyle.Top;
        var cifrado = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
        cifrado.Controls.Add(_cifrado);
        cifrado.Controls.Add(Titulo("RESULTADO DEL CIFRADO · LO QUE VIAJA POR LA ANTENA"));
        _cifrado.BringToFront();

        var zona = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 8) };
        zona.Controls.Add(cifrado);
        zona.Controls.Add(_enClaro);
        zona.Controls.Add(Titulo("MENSAJE EN CLARO"));
        cifrado.BringToFront();
        return zona;
    }

    private Control ArmarHistorial()
    {
        _historial.Dock = DockStyle.Fill;
        _historial.HorizontalScrollbar = true;
        var historial = new Panel { Dock = DockStyle.Top, Height = 88 };
        historial.Controls.Add(_historial);
        historial.Controls.Add(Titulo("TRANSMISIONES DE ESTA PARTIDA"));
        _historial.BringToFront();

        var consola = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
        consola.Controls.Add(_consola);
        consola.Controls.Add(Titulo("RESPUESTA DE LA TIERRA"));
        _consola.BringToFront();

        var zona = new Panel { Dock = DockStyle.Bottom, Height = 196, Padding = new Padding(16, 0, 0, 0) };
        zona.Controls.Add(consola);
        zona.Controls.Add(historial);
        return zona;
    }

    private void CambiarTipo()
    {
        if (_tipos.SelectedItem is RenglonTipo { Tipo: var tipo })
            _contenido.Text = tipo.ContenidoSugerido;
        Previsualizar();
    }

    private Mensaje? MensajeActual() =>
        _tipos.SelectedItem is RenglonTipo { Tipo: var tipo } && _cifrados.SelectedItem is RenglonCifrado { Cifrado: var cifrado }
            ? tipo.Crear(_contenido.Text, cifrado)
            : null;

    // El mensaje se arma de nuevo con cada cambio: el mismo tipo con otro cifrado da otro texto en la antena.
    private void Previsualizar()
    {
        if (MensajeActual() is not { } mensaje)
            return;

        _enClaro.Text = mensaje.TextoPlano;
        _cifrado.Text = mensaje.TextoCifrado;
        _info.Text = string.Join(Environment.NewLine,
            ((RenglonTipo)_tipos.SelectedItem!).Tipo.Descripcion,
            $"{mensaje.Cifrado.Nombre}: {mensaje.Cifrado.Descripcion}",
            $"Costo: {mensaje.Cifrado.Costo} créditos · riesgo de intercepción: {mensaje.Cifrado.RiesgoIntercepcion}%",
            $"Respuesta esperada: {mensaje.Refuerzo.Descripcion}.");

        _transmitir.Enabled = Partida.PuedeTransmitir(mensaje);
        _antena.Text = !Partida.AntenaDisponible ? "La antena ya transmitió: se libera al empezar la próxima oleada."
            : Partida.Progreso.Creditos < mensaje.Cifrado.Costo ? $"No alcanzan los créditos (tenés {Partida.Progreso.Creditos})."
            : $"Créditos disponibles: {Partida.Progreso.Creditos}.";
    }

    private void Transmitir()
    {
        if (MensajeActual() is not { } mensaje || !Partida.PuedeTransmitir(mensaje))
            return;

        Partida.Transmitir(mensaje);
        Partida.RevisarLogros();
        RefrescarHistorial();
        MostrarEntradasNuevas();
        Previsualizar();
    }

    private void RefrescarHistorial()
    {
        _historial.BeginUpdate();
        _historial.Items.Clear();
        if (Partida.Transmisiones.Count == 0)
            _historial.Items.Add("Todavía no se transmitió nada.");
        foreach (var t in Partida.Transmisiones.Reverse())
            _historial.Items.Add($"{t.Tipo} · {t.Cifrado} · {(t.Interceptada ? "INTERCEPTADO por el Enjambre" : "recibido por la Tierra")} · {t.Refuerzo.Descripcion}");
        _historial.EndUpdate();
    }

    private void MostrarEntradasNuevas()
    {
        var entradas = Partida.Bitacora.Entradas;
        for (; _entradasMostradas < entradas.Count; _entradasMostradas++)
            _consola.Agregar(entradas[_entradasMostradas]);
    }

    private static Label Seccion(string texto)
    {
        var etiqueta = Tema.CrearEtiqueta(texto, Tema.Chica, Tema.TextoTenue);
        etiqueta.Margin = new Padding(3, 8, 3, 4);
        return etiqueta;
    }

    private static Label Titulo(string texto) => new()
    {
        Text = texto,
        Dock = DockStyle.Top,
        Height = 22,
        Font = Tema.Chica,
        ForeColor = Tema.TextoTenue,
    };

    private static ListBox CrearLista(int alto) => new()
    {
        Height = alto,
        Width = 330,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(5, 9, 16),
        ForeColor = Tema.Texto,
        Font = Tema.Normal,
        IntegralHeight = false,
        Margin = new Padding(4),
    };

    private static TextBox CrearVisor() => new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Fill,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(5, 9, 16),
        ForeColor = Tema.Acento,
        Font = Tema.Consola,
    };

    private sealed record RenglonTipo(TipoMensaje Tipo)
    {
        public override string ToString() => $"{Tipo.Nombre} · prioridad {Tipo.Crear("", new SinCifrado()).Prioridad}";
    }

    private sealed record RenglonCifrado(ICifrado Cifrado)
    {
        public override string ToString() => $"{Cifrado.Nombre} · {Cifrado.Costo} créditos · riesgo {Cifrado.RiesgoIntercepcion}%";
    }
}
