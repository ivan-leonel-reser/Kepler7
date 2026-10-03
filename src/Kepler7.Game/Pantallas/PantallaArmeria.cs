using Kepler7.BE;
using Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>
/// Acá se elige qué capas lleva el escuadrón y en qué orden. La pantalla solo le pide los cambios
/// a la <see cref="Armeria"/> y muestra el recorrido del golpe de prueba que devuelve Core.
/// </summary>
internal class PantallaArmeria : PantallaModulo
{
    private static readonly int[] GolpesDePrueba = [2, 4, 8];

    private readonly Armeria _armeria;
    private readonly ListBox _cadena = CrearLista(132);
    private readonly ListBox _guardadas = CrearLista(92);
    private readonly Button _subir = Tema.CrearBoton("Subir", 146);
    private readonly Button _bajar = Tema.CrearBoton("Bajar", 146);
    private readonly Button _quitar = Tema.CrearBoton("Quitar", 146);
    private readonly Button _agregar = Tema.CrearBoton("Agregar a la cadena", 220);
    private readonly List<Button> _golpes = [];
    private readonly ConsolaBitacora _consola = new() { Dock = DockStyle.Fill };
    private readonly Queue<EntradaBitacora> _pendientes = new();
    private readonly System.Windows.Forms.Timer _animacion = new() { Interval = 350 };

    public PantallaArmeria(FormPrincipal principal)
        : base(principal, "Armería", Patron.ChainOfResponsibility,
            "Elegí qué capas de defensa lleva el escuadrón y en qué orden reciben cada golpe.")
    {
        _armeria = Partida.Armeria;

        Contenido.Controls.Add(ArmarPrueba());
        Contenido.Controls.Add(ArmarCadena());

        _cadena.SelectedIndexChanged += (_, _) => ActualizarBotones();
        _guardadas.SelectedIndexChanged += (_, _) => ActualizarBotones();
        _subir.Click += (_, _) => Cambiar(_armeria.Subir);
        _bajar.Click += (_, _) => Cambiar(_armeria.Bajar);
        _quitar.Click += (_, _) => Cambiar(_armeria.Quitar);
        _agregar.Click += (_, _) => Agregar();
        _animacion.Tick += (_, _) => MostrarSiguientePaso();

        Refrescar(null);
    }

    private Control ArmarCadena()
    {
        var mover = new FlowLayoutPanel { AutoSize = true, Margin = Padding.Empty };
        mover.Controls.AddRange([_subir, _bajar, _quitar]);

        var aviso = Tema.CrearEtiqueta(
            "La cadena se le arma de nuevo a cada unidad al empezar una oleada: el escudo y los bloqueos arrancan cargados.",
            Tema.Chica, Tema.TextoTenue);
        aviso.MaximumSize = new Size(450, 0);
        aviso.Margin = new Padding(4, 12, 3, 0);

        var columna = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 470, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        columna.Controls.AddRange(
        [
            Seccion("CADENA DEL ESCUADRÓN · EL GOLPE ENTRA POR ARRIBA"),
            _cadena,
            mover,
            Seccion("CAPAS GUARDADAS"),
            _guardadas,
            _agregar,
            aviso,
        ]);
        return columna;
    }

    private Control ArmarPrueba()
    {
        var botones = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52 };
        foreach (var cantidad in GolpesDePrueba)
        {
            var boton = Tema.CrearBoton($"Golpe de {cantidad}", 130);
            boton.Click += (_, _) => ProbarGolpe(cantidad);
            _golpes.Add(boton);
            botones.Controls.Add(boton);
        }

        var zona = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0) };
        zona.Controls.Add(_consola);
        zona.Controls.Add(botones);
        zona.Controls.Add(new Label
        {
            Text = "GOLPE DE PRUEBA CONTRA UN MANIQUÍ · RECORRIDO CAPA POR CAPA",
            Dock = DockStyle.Top,
            Height = 22,
            Font = Tema.Chica,
            ForeColor = Tema.TextoTenue,
        });
        return zona;
    }

    private void Cambiar(Action<TipoCapa> cambio)
    {
        if (_cadena.SelectedItem is not Renglon { Capa: { } capa })
            return;

        cambio(capa);
        Refrescar(capa);
    }

    private void Agregar()
    {
        if (_guardadas.SelectedItem is not Renglon { Capa: { } capa })
            return;

        _armeria.Agregar(capa);
        Refrescar(capa);
    }

    private void ProbarGolpe(int cantidad)
    {
        var golpe = _armeria.ProbarGolpe(cantidad, Partida.Aleatorio);

        _consola.Items.Clear();
        _pendientes.Enqueue(new EntradaBitacora("Golpe", $"Entra un golpe de {cantidad}.", TipoEntrada.Advertencia));
        foreach (var paso in golpe.Recorrido)
            _pendientes.Enqueue(EntradaDe(paso));
        _pendientes.Enqueue(golpe.VidaPerdida > 0
            ? new EntradaBitacora("Resultado", $"El maniquí pierde {golpe.VidaPerdida} de vida.", TipoEntrada.Peligro)
            : new EntradaBitacora("Resultado", "El golpe queda frenado antes de llegar a la vida.", TipoEntrada.Exito));

        foreach (var boton in _golpes)
            boton.Enabled = false;
        MostrarSiguientePaso();
        _animacion.Start();
    }

    private static EntradaBitacora EntradaDe(PasoDanio paso)
    {
        if (paso.Capa == "Vida")
            return new EntradaBitacora(paso.Capa, $"le llegan {paso.Entra}: {paso.Detalle}", TipoEntrada.Peligro);

        var frenoAlgo = paso.Sale < paso.Entra;
        return new EntradaBitacora(paso.Capa, $"le llegan {paso.Entra}: {paso.Detalle} · siguen {paso.Sale}",
            frenoAlgo ? TipoEntrada.Exito : TipoEntrada.Info);
    }

    private void MostrarSiguientePaso()
    {
        if (_pendientes.TryDequeue(out var entrada))
        {
            _consola.Agregar(entrada);
            return;
        }

        _animacion.Stop();
        foreach (var boton in _golpes)
            boton.Enabled = true;
    }

    private void Refrescar(TipoCapa? seleccionada)
    {
        var numero = 1;
        _cadena.BeginUpdate();
        _cadena.Items.Clear();
        foreach (var capa in _armeria.Capas)
            _cadena.Items.Add(new Renglon(capa, $"{numero++}. {capa}: {Armeria.Descripcion(capa)}"));
        _cadena.Items.Add(new Renglon(null, $"{numero}. Vida: lo que llega hasta acá se pierde (siempre va última)"));
        _cadena.EndUpdate();

        _guardadas.BeginUpdate();
        _guardadas.Items.Clear();
        foreach (var capa in _armeria.Guardadas)
            _guardadas.Items.Add(new Renglon(capa, $"{capa}: {Armeria.Descripcion(capa)}"));
        _guardadas.EndUpdate();

        // La capa que se acaba de mover queda elegida para poder seguir moviéndola.
        if (seleccionada is { } elegida && _armeria.Capas.Contains(elegida))
            _cadena.SelectedIndex = _armeria.Capas.ToList().IndexOf(elegida);
        ActualizarBotones();
    }

    private void ActualizarBotones()
    {
        var capa = (_cadena.SelectedItem as Renglon)?.Capa;
        _subir.Enabled = capa is { } paraSubir && _armeria.PuedeSubir(paraSubir);
        _bajar.Enabled = capa is { } paraBajar && _armeria.PuedeBajar(paraBajar);
        _quitar.Enabled = capa is not null;
        _agregar.Enabled = _guardadas.SelectedItem is Renglon;
    }

    private static Label Seccion(string texto)
    {
        var etiqueta = Tema.CrearEtiqueta(texto, Tema.Chica, Tema.TextoTenue);
        etiqueta.Margin = new Padding(3, 8, 3, 4);
        return etiqueta;
    }

    private static ListBox CrearLista(int alto) => new()
    {
        Height = alto,
        Width = 450,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(5, 9, 16),
        ForeColor = Tema.Texto,
        Font = Tema.Normal,
        IntegralHeight = false,
        Margin = new Padding(4),
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _animacion.Dispose();
        base.Dispose(disposing);
    }

    // Capa es null en el renglón de la vida, que no se puede mover ni quitar.
    private sealed record Renglon(TipoCapa? Capa, string Texto)
    {
        public override string ToString() => Texto;
    }
}
