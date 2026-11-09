using Kepler7.BE;
using Kepler7.Core.Patrones.Estructurales.Proxy;
namespace Kepler7.Game.Pantallas;

/// <summary>
/// La terminal de la bóveda. La pantalla elige credencial y archivo y le pide la operación a la partida,
/// que siempre pasa por el <see cref="ProxyBoveda"/>: el veredicto y el registro de intentos los decide Core.
/// </summary>
internal class PantallaBoveda : PantallaModulo
{
    private readonly ProxyBoveda _boveda;
    private readonly ListBox _credenciales = CrearLista(128);
    private readonly ListBox _archivos = CrearLista(172);
    private readonly Button _leer = Tema.CrearBoton("Leer archivo", 160);
    private readonly Button _guardar = Tema.CrearBoton("Guardar cambios", 160);
    private readonly Label _veredicto = Tema.CrearEtiqueta("TERMINAL DE LA BÓVEDA", Tema.Destacado, Tema.TextoTenue);
    private readonly Label _detalle = Tema.CrearEtiqueta("Elegí una credencial y un archivo.", Tema.Normal, Tema.Texto);
    private readonly TextBox _contenido = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Fill,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(5, 9, 16),
        ForeColor = Tema.Texto,
        Font = Tema.Consola,
        PlaceholderText = "Leé el archivo para ver qué tiene, o escribí algo y guardalo.",
    };
    private readonly ListBox _registro = CrearLista(140);

    public PantallaBoveda(FormPrincipal principal)
        : base(principal, "Bóveda", Patron.Proxy,
            "Información reservada. El proxy verifica el rol antes de dejar pasar cada lectura o escritura.")
    {
        _boveda = Partida.Boveda;

        Contenido.Controls.Add(ArmarTerminal());
        Contenido.Controls.Add(ArmarRegistro());
        Contenido.Controls.Add(ArmarSeleccion());

        _credenciales.SelectedIndexChanged += (_, _) => CambiarCredencial();
        _archivos.SelectedIndexChanged += (_, _) => CambiarArchivo();
        _leer.Click += (_, _) => Operar(id => _contenido.Text = Partida.LeerArchivoBoveda(id), "leer");
        _guardar.Click += (_, _) => Operar(id => Partida.EscribirArchivoBoveda(id, _contenido.Text), "escribir");

        foreach (var credencial in CatalogoBoveda.Credenciales)
            _credenciales.Items.Add(credencial);
        foreach (var archivo in CatalogoBoveda.Archivos)
            _archivos.Items.Add(new Renglon(archivo));
        if (_boveda.Credencial is { } puesta)
            _credenciales.SelectedItem = puesta;
        _archivos.SelectedIndex = Math.Min(Partida.NumeroProximaOleada, CatalogoBoveda.Archivos.Count) - 1;

        RefrescarRegistro();
        ActualizarBotones();
    }

    private Control ArmarSeleccion()
    {
        var roles = string.Join(" y ", ProxyBoveda.RolesAutorizados.Select(r => new Credencial("", r).NombreRol));
        var aviso = Tema.CrearEtiqueta($"Solo {roles} pueden leer o escribir. Los demás intentos se rechazan y quedan registrados.", Tema.Chica, Tema.TextoTenue);
        aviso.MaximumSize = new Size(330, 0);
        aviso.Margin = new Padding(4, 2, 3, 0);

        var botones = new FlowLayoutPanel { AutoSize = true, Margin = Padding.Empty };
        botones.Controls.AddRange([_leer, _guardar]);

        var columna = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 350, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        columna.Controls.AddRange(
        [
            Seccion("CREDENCIAL · QUIÉN SE PRESENTA"),
            _credenciales,
            aviso,
            Seccion("ARCHIVOS DE LA BÓVEDA"),
            _archivos,
            botones,
        ]);
        return columna;
    }

    private Control ArmarTerminal()
    {
        _veredicto.Margin = new Padding(0, 8, 0, 2);
        _detalle.MaximumSize = new Size(760, 0);
        _detalle.Margin = new Padding(0, 0, 0, 10);
        var veredicto = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        veredicto.Controls.AddRange([_veredicto, _detalle]);

        var zona = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 8) };
        zona.Controls.Add(_contenido);
        zona.Controls.Add(Titulo("CONTENIDO DEL ARCHIVO"));
        zona.Controls.Add(veredicto);
        _contenido.BringToFront();
        return zona;
    }

    private Control ArmarRegistro()
    {
        _registro.Dock = DockStyle.Fill;
        _registro.HorizontalScrollbar = true;
        var zona = new Panel { Dock = DockStyle.Bottom, Height = 150, Padding = new Padding(16, 0, 0, 0) };
        zona.Controls.Add(_registro);
        zona.Controls.Add(Titulo("REGISTRO DE INTENTOS DENEGADOS · PERSISTE ENTRE PARTIDAS"));
        _registro.BringToFront();
        return zona;
    }

    private void CambiarCredencial()
    {
        _boveda.Credencial = _credenciales.SelectedItem as Credencial;
        if (_boveda.Credencial is { } credencial)
            Veredicto("CREDENCIAL PUESTA", Tema.Texto, $"{credencial} se presenta ante la bóveda.");
        ActualizarBotones();
    }

    private void CambiarArchivo()
    {
        _contenido.Clear();
        ActualizarBotones();
    }

    // La operación la hace Core a través del proxy; acá solo se muestra cómo terminó.
    private void Operar(Action<string> operacion, string verbo)
    {
        if (_archivos.SelectedItem is not Renglon { Archivo: var archivo })
            return;

        try
        {
            operacion(archivo.Id);
            var detalle = $"{_boveda.Credencial} pudo {verbo} '{archivo.Id}'.";
            if (verbo == "leer" && CatalogoBoveda.OleadaDelInforme(archivo.Id) == Partida.NumeroProximaOleada)
                detalle += " Es el informe de la próxima oleada: la IA de cada enemigo va a verse desde el primer turno.";
            foreach (var insignia in Partida.RevisarLogros())
                detalle += $" Insignia desbloqueada: {insignia.Nombre}.";
            Veredicto("ACCESO CONCEDIDO", Tema.Exito, detalle);
        }
        catch (AccesoDenegadoException e)
        {
            if (verbo == "leer")
                _contenido.Clear();
            Veredicto("ACCESO DENEGADO", Tema.Peligro,
                $"{e.Intento.Usuario} ({e.Intento.Rol}) no puede {verbo} '{archivo.Id}'. " +
                (e.Registrado ? "El intento quedó en el registro." : "El intento no quedó en el registro: la base no responde."));
        }

        RefrescarRegistro();
    }

    private void Veredicto(string titulo, Color color, string detalle)
    {
        _veredicto.Text = titulo;
        _veredicto.ForeColor = color;
        _detalle.Text = detalle;
    }

    private void RefrescarRegistro()
    {
        _registro.BeginUpdate();
        _registro.Items.Clear();
        IReadOnlyList<IntentoAcceso> intentos;
        try
        {
            intentos = _boveda.IntentosDenegados;
        }
        catch (Exception)
        {
            // La interfaz no conoce la capa de datos: cualquier falla al leer el registro se muestra en la lista.
            _registro.Items.Add("No se pudo leer el registro: la base no responde.");
            _registro.EndUpdate();
            return;
        }
        if (intentos.Count == 0)
            _registro.Items.Add("Sin intentos denegados.");
        foreach (var intento in intentos.OrderByDescending(i => i.Fecha))
            _registro.Items.Add($"{intento.Fecha:dd/MM HH:mm:ss} · {intento.Usuario} ({intento.Rol}) · {intento.Operacion} · {intento.IdArchivo}");
        _registro.EndUpdate();
    }

    private void ActualizarBotones()
    {
        var hayArchivo = _archivos.SelectedItem is Renglon;
        _leer.Enabled = hayArchivo;
        _guardar.Enabled = hayArchivo;
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

    private sealed record Renglon(ArchivoBoveda Archivo)
    {
        public override string ToString() => $"{Archivo.Titulo}  [{Archivo.Id}]";
    }
}
