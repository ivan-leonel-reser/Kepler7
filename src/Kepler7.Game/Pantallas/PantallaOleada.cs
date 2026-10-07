using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Comportamiento.State;
using Kepler7.Core.Patrones.Comportamiento.Strategy;
using Kepler7.Core.Patrones.Comportamiento.TemplateMethod;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>
/// El combate de una oleada. La pantalla solo arma las acciones que elige el jugador;
/// las reglas, los comandos y el turno del Enjambre están en <see cref="Oleada"/>.
/// </summary>
internal class PantallaOleada : Pantalla
{
    private enum AccionPendiente { Ninguna, Atacar, Curar, Hackear }

    private readonly Partida _partida;
    private readonly Oleada _oleada;
    private readonly VistaCampo _campo;
    private readonly ConsolaBitacora _consola = new() { Dock = DockStyle.Fill };
    private readonly ListBox _accionesTurno = CrearLista(104);
    private readonly ListBox _historial = CrearLista(80);
    private readonly ListBox _decisionesIA = CrearLista(72);
    private readonly ComboBox _nuevaIA = new()
    {
        Width = 200,
        DropDownStyle = ComboBoxStyle.DropDownList,
        DrawMode = DrawMode.OwnerDrawFixed,
        FlatStyle = FlatStyle.Flat,
        BackColor = Tema.Panel,
        ForeColor = Tema.Texto,
        Font = Tema.Normal,
        Margin = new Padding(4, 0, 4, 4),
    };
    private readonly Button _hackear = Tema.CrearBoton("Hackear IA", 200);
    private readonly Label _estado = Tema.CrearEtiqueta("", Tema.Destacado);
    private readonly Label _ayuda = Tema.CrearEtiqueta("", Tema.Normal, Tema.Texto);
    private readonly Button _mover = Tema.CrearBoton("Cambiar de fila", 330);
    private readonly Button _atacar = Tema.CrearBoton("Atacar", 330);
    private readonly Button _curar = Tema.CrearBoton("Curar", 330);
    private readonly Button _construir = Tema.CrearBoton("Construir barricada", 330);
    private readonly Button _deshacer = Tema.CrearBoton("Deshacer", 161);
    private readonly Button _rehacer = Tema.CrearBoton("Rehacer", 161);
    private readonly Button _terminar = Tema.CrearBoton("Terminar turno", 330);
    private readonly Panel _resultado = new() { Dock = DockStyle.Bottom, Height = 64, Visible = false, BackColor = Tema.Panel };
    private readonly Label _mensajeResultado = Tema.CrearEtiqueta("", Tema.Destacado);
    private readonly Button _botonResultado = Tema.CrearBoton("", 220);
    private readonly System.Windows.Forms.Timer _turnoEnjambre = new() { Interval = 650 };

    private UnidadEscuadron? _seleccionada;
    private AccionPendiente _pendiente;
    private int _entradasMostradas;
    private Action _alCerrar = () => { };

    public PantallaOleada(FormPrincipal principal) : base(principal)
    {
        _partida = principal.PartidaActual
            ?? throw new InvalidOperationException("No hay una partida en curso.");
        _oleada = _partida.EmpezarOleada();
        _campo = new VistaCampo(_oleada);

        ArmarPantalla();

        _campo.UnidadClickeada += (_, unidad) => ClickEnUnidad(unidad);
        _mover.Click += (_, _) => Actuar(u => _oleada.Mover(u));
        _construir.Click += (_, _) => Actuar(u => _oleada.Construir(u));
        _atacar.Click += (_, _) => Elegir(AccionPendiente.Atacar);
        _curar.Click += (_, _) => Elegir(AccionPendiente.Curar);
        _deshacer.Click += (_, _) => Deshacer();
        _rehacer.Click += (_, _) => DespuesDeActuar(_oleada.Rehacer() is not null);
        _terminar.Click += (_, _) => TerminarTurno();
        _hackear.Click += (_, _) => Elegir(_pendiente == AccionPendiente.Hackear ? AccionPendiente.Ninguna : AccionPendiente.Hackear);
        _nuevaIA.SelectedIndexChanged += (_, _) => Refrescar();
        _botonResultado.Click += (_, _) => _alCerrar();
        _turnoEnjambre.Tick += (_, _) => JugarEnemigo();

        SeleccionarSiguiente();
        Refrescar();
    }

    private void ArmarPantalla()
    {
        var titulo = Tema.CrearEtiqueta($"Oleada {_oleada.Numero} de {ProgresoJugador.OleadasTotales}", Tema.Encabezado, Tema.Acento);
        titulo.Location = new Point(20, 14);
        var zonaEstado = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 360, Padding = new Padding(14, 20, 0, 0) };
        zonaEstado.Controls.Add(_estado);
        var encabezado = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Tema.Panel };
        encabezado.Controls.AddRange([zonaEstado, titulo]);

        _ayuda.MaximumSize = new Size(330, 0);
        _ayuda.Margin = new Padding(3, 4, 3, 10);
        var acciones = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Margin = Padding.Empty,
        };
        acciones.Controls.AddRange([_mover, _atacar, _curar, _construir]);
        var deshacerRehacer = new FlowLayoutPanel { AutoSize = true, Margin = Padding.Empty };
        deshacerRehacer.Controls.AddRange([_deshacer, _rehacer]);
        _terminar.Height = 46;
        _terminar.Margin = new Padding(4, 8, 4, 4);

        var lateral = new TableLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 360,
            ColumnCount = 1,
            Padding = new Padding(12, 10, 12, 10),
            BackColor = Tema.Panel,
        };
        lateral.Controls.Add(Seccion("ACCIONES"));
        lateral.Controls.Add(_ayuda);
        lateral.Controls.Add(acciones);
        lateral.Controls.Add(Seccion("ESTE TURNO · SE PUEDE DESHACER"));
        lateral.Controls.Add(_accionesTurno);
        lateral.Controls.Add(deshacerRehacer);
        lateral.Controls.Add(_terminar);
        lateral.Controls.Add(Seccion("HISTORIAL DE COMANDOS"));
        lateral.Controls.Add(_historial);
        _historial.Dock = DockStyle.Fill;
        for (var i = 0; i < lateral.Controls.Count - 1; i++)
            lateral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lateral.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _mensajeResultado.Location = new Point(20, 20);
        var zonaBoton = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 250, Padding = new Padding(0, 12, 12, 0) };
        zonaBoton.Controls.Add(_botonResultado);
        _resultado.Controls.AddRange([zonaBoton, _mensajeResultado]);

        var zonaBitacora = new Panel { Dock = DockStyle.Bottom, Height = 120, Padding = new Padding(16, 4, 16, 8) };
        zonaBitacora.Controls.Add(_consola);

        var centro = new Panel { Dock = DockStyle.Fill };
        centro.Controls.Add(_campo);
        centro.Controls.Add(ArmarPanelIA());
        centro.Controls.Add(zonaBitacora);
        centro.Controls.Add(_resultado);

        Controls.Add(centro);
        Controls.Add(lateral);
        Controls.Add(encabezado);
    }

    // Panel de Strategy: a la izquierda lo que decidió cada enemigo, a la derecha el hackeo.
    private Panel ArmarPanelIA()
    {
        _nuevaIA.Items.AddRange(CatalogoEstrategias.Todas.ToArray<object>());
        _nuevaIA.DisplayMember = nameof(IEstrategiaIA.Nombre);
        _nuevaIA.SelectedIndex = 0;
        _nuevaIA.DrawItem += DibujarOpcionIA;
        _hackear.Height = 34;

        var tituloHackeo = Tema.CrearEtiqueta("HACKEAR · NUEVA IA", Tema.Chica, Tema.TextoTenue);
        tituloHackeo.Margin = new Padding(4, 2, 3, 3);
        var hackeo = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 216, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        hackeo.Controls.AddRange([tituloHackeo, _nuevaIA, _hackear]);

        _decisionesIA.Dock = DockStyle.Fill;
        var decisiones = new Panel { Dock = DockStyle.Fill };
        decisiones.Controls.Add(_decisionesIA);
        decisiones.Controls.Add(new Label
        {
            Text = "IA DEL ENJAMBRE · QUÉ DECIDIÓ CADA ENEMIGO Y POR QUÉ",
            Dock = DockStyle.Top,
            Height = 20,
            Font = Tema.Chica,
            ForeColor = Tema.TextoTenue,
        });

        var panel = new Panel { Dock = DockStyle.Bottom, Height = 100, Padding = new Padding(16, 4, 12, 0) };
        panel.Controls.Add(decisiones);
        panel.Controls.Add(hackeo);
        return panel;
    }

    // El combo estándar se pinta claro y no se lee sobre el fondo oscuro.
    private void DibujarOpcionIA(object? sender, DrawItemEventArgs e)
    {
        var resaltada = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
        using (var fondo = new SolidBrush(resaltada ? Tema.PanelResaltado : Tema.Panel))
            e.Graphics.FillRectangle(fondo, e.Bounds);

        if (e.Index >= 0 && _nuevaIA.Items[e.Index] is IEstrategiaIA ia)
            TextRenderer.DrawText(e.Graphics, ia.Nombre, _nuevaIA.Font, e.Bounds, Tema.Texto,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }

    private void ClickEnUnidad(Unidad unidad)
    {
        if (_oleada.Estado != EstadoOleada.TurnoJugador)
            return;

        if (_pendiente == AccionPendiente.Hackear && unidad is Enemigo enemigo
            && _oleada.PuedeHackear(enemigo) && _nuevaIA.SelectedItem is IEstrategiaIA nuevaIA)
        {
            _oleada.Hackear(enemigo, nuevaIA);
            _pendiente = AccionPendiente.Ninguna;
            RevisarLogros();
            Refrescar();
            return;
        }

        var actor = _seleccionada;
        if (actor is not null && _pendiente == AccionPendiente.Atacar && _oleada.ObjetivosDeAtaque(actor).Contains(unidad))
        {
            _oleada.Atacar(actor, unidad);
            _campo.MarcarGolpe(unidad);
            DespuesDeActuar(true);
            return;
        }
        if (actor is not null && _pendiente == AccionPendiente.Curar && _oleada.ObjetivosDeCuracion(actor).Contains(unidad))
        {
            _oleada.Curar(actor, unidad);
            DespuesDeActuar(true);
            return;
        }

        if (unidad is UnidadEscuadron propia && _oleada.PuedeActuar(propia))
        {
            _seleccionada = propia;
            _pendiente = AccionPendiente.Ninguna;
        }
        Refrescar();
    }

    private void Actuar(Action<UnidadEscuadron> accion)
    {
        if (_seleccionada is null)
            return;
        accion(_seleccionada);
        DespuesDeActuar(true);
    }

    private void Elegir(AccionPendiente accion)
    {
        _pendiente = accion;
        Refrescar();
    }

    private void Deshacer()
    {
        // La unidad que se deshizo vuelve a quedar elegida, para que se vea qué se liberó.
        if (_oleada.Deshacer() is { Actor: UnidadEscuadron liberada })
            _seleccionada = liberada;
        _pendiente = AccionPendiente.Ninguna;
        RevisarLogros();
        Refrescar();
    }

    private void DespuesDeActuar(bool huboCambio)
    {
        if (!huboCambio)
            return;

        _pendiente = AccionPendiente.Ninguna;
        RevisarLogros();
        if (_oleada.Terminada)
        {
            Terminar();
            return;
        }
        SeleccionarSiguiente();
        Refrescar();
    }

    private void TerminarTurno()
    {
        _seleccionada = null;
        _pendiente = AccionPendiente.Ninguna;
        _oleada.TerminarTurno();
        Refrescar();
        _turnoEnjambre.Start();
    }

    private void JugarEnemigo()
    {
        var jugada = _oleada.JugarSiguienteEnemigo();
        if (jugada?.Objetivo is { } objetivo)
            _campo.MarcarGolpe(objetivo);

        if (_oleada.Terminada)
        {
            _turnoEnjambre.Stop();
            Terminar();
            return;
        }
        if (jugada is null)
        {
            _turnoEnjambre.Stop();
            SeleccionarSiguiente();
        }
        Refrescar();
    }

    private void Terminar()
    {
        var resultado = _partida.CerrarOleada();
        _seleccionada = null;
        Refrescar();
        _consola.AgregarTodas(resultado.SecuenciaEstacion);
        RevisarLogros();

        if (resultado.Victoria)
        {
            var salvada = _partida.Progreso.EstacionSalvada;
            _mensajeResultado.Text = salvada
                ? "Kepler-7 está a salvo: resististe las 6 oleadas."
                : $"Oleada superada: +{resultado.Xp} XP · +{resultado.Creditos} créditos" + (resultado.SubioNivel ? " · subiste de nivel" : "");
            _mensajeResultado.ForeColor = Tema.Exito;
            _botonResultado.Text = salvada ? "Ver el resumen" : "Volver a la estación";
            _alCerrar = salvada
                ? () => Principal.Mostrar(new PantallaFinal(Principal, _partida.Resumen(), victoria: true))
                : Principal.IrAEstacion;
        }
        else
        {
            _mensajeResultado.Text = "Cayó el escuadrón. Kepler-7 se apaga.";
            _mensajeResultado.ForeColor = Tema.Peligro;
            _botonResultado.Text = "Ver el resumen";
            _alCerrar = () => Principal.Mostrar(new PantallaFinal(Principal, _partida.Resumen(), victoria: false));
        }
        _resultado.Visible = true;
    }

    private void RevisarLogros()
    {
        // Primero lo que pasó en la oleada, así la insignia queda debajo de la jugada que la dio.
        MostrarEntradasNuevas();
        foreach (var insignia in _partida.RevisarLogros())
            _consola.Agregar(new EntradaBitacora("Logros", $"Insignia desbloqueada: {insignia.Nombre}", TipoEntrada.Exito));
    }

    private void SeleccionarSiguiente() =>
        _seleccionada = _oleada.Escuadron.FirstOrDefault(_oleada.PuedeActuar);

    private void Refrescar()
    {
        var turnoJugador = _oleada.Estado == EstadoOleada.TurnoJugador;
        var actor = _seleccionada is not null && _oleada.PuedeActuar(_seleccionada) ? _seleccionada : null;

        _mover.Enabled = actor is not null && _oleada.PuedeMover(actor);
        _mover.Text = actor?.Fila == Fila.Vanguardia ? "Pasar atrás" : actor is null ? "Cambiar de fila" : "Pasar adelante";
        _atacar.Enabled = actor is not null && _oleada.ObjetivosDeAtaque(actor).Count > 0;
        _curar.Enabled = actor is not null && _oleada.ObjetivosDeCuracion(actor).Count > 0;
        _construir.Enabled = actor is not null && _oleada.PuedeConstruir(actor);
        _construir.Text = $"Construir barricada (quedan {_oleada.BarricadasDisponibles})";
        _deshacer.Enabled = turnoJugador && _oleada.Comandos.PuedeDeshacer;
        _rehacer.Enabled = turnoJugador && _oleada.Comandos.PuedeRehacer;
        _terminar.Enabled = turnoJugador;
        _hackear.Enabled = turnoJugador && _oleada.HackeosDisponibles > 0;
        _hackear.Text = _pendiente == AccionPendiente.Hackear ? "Cancelar hackeo" : $"Hackear IA (quedan {_oleada.HackeosDisponibles})";

        (_estado.Text, _estado.ForeColor) = _oleada.Estado switch
        {
            EstadoOleada.TurnoJugador => ($"TU TURNO · TURNO {_oleada.Turno}", Tema.Acento),
            EstadoOleada.TurnoEnjambre => ("TURNO DEL ENJAMBRE", Tema.Peligro),
            EstadoOleada.Victoria => ("OLEADA SUPERADA", Tema.Exito),
            _ => ("DERROTA", Tema.Peligro),
        };
        _ayuda.Text = Ayuda(actor);

        Llenar(_accionesTurno, _oleada.Comandos.AccionesDelTurno.Select(c => c.Descripcion));
        Llenar(_historial, _oleada.Comandos.Historial.Reverse());
        Llenar(_decisionesIA, _oleada.UltimasJugadas.Count > 0
            ? _oleada.UltimasJugadas.Select(TextoDecision)
            : ["Todavía no jugó el Enjambre. Cuando juegue, acá queda qué decidió cada enemigo."]);

        IReadOnlyCollection<Unidad> objetivos = _pendiente switch
        {
            AccionPendiente.Hackear => _oleada.Campo.UnidadesVivasDe(Bando.Enjambre).ToList(),
            AccionPendiente.Atacar when actor is not null => _oleada.ObjetivosDeAtaque(actor),
            AccionPendiente.Curar when actor is not null => _oleada.ObjetivosDeCuracion(actor),
            _ => [],
        };
        _campo.Mostrar(actor, objetivos);
        MostrarEntradasNuevas();
    }

    private void MostrarEntradasNuevas()
    {
        var entradas = _oleada.Bitacora.Entradas;
        for (; _entradasMostradas < entradas.Count; _entradasMostradas++)
            _consola.Agregar(entradas[_entradasMostradas]);
    }

    private string Ayuda(UnidadEscuadron? actor)
    {
        if (_oleada.Estado == EstadoOleada.TurnoEnjambre)
            return "El Enjambre está jugando...";
        if (_oleada.Terminada)
            return "";
        if (_pendiente == AccionPendiente.Hackear && _nuevaIA.SelectedItem is IEstrategiaIA ia)
            return $"Hackeo con IA {ia.Nombre}. {ia.Descripcion} Elegí un enemigo (en amarillo); no se puede deshacer.";
        if (actor is null)
        {
            var sinTurno = _oleada.Escuadron.Where(u => !u.Estado.PuedeActuar)
                .Select(u => $" {u.Nombre} está {u.Estado.Nombre.ToLowerInvariant()}: {u.Estado.Efecto}.");
            var aviso = _oleada.Escuadron.Any(_oleada.PuedeActuar)
                ? "Elegí una unidad de tu escuadrón."
                : "No queda nadie por actuar. Terminá el turno o deshacé una acción.";
            return aviso + string.Concat(sinTurno);
        }

        // Si la unidad no está normal, se avisa en qué estado quedó y qué le cambia.
        var estado = actor.Estado is Normal ? "" : $" Está {actor.Estado.Nombre.ToLowerInvariant()}: {actor.Estado.Efecto}.";
        return _pendiente switch
        {
            AccionPendiente.Atacar => $"{actor.Nombre}: elegí a qué enemigo atacar (en amarillo).",
            AccionPendiente.Curar => $"{actor.Nombre}: elegí a quién curar (en amarillo).",
            _ when !actor.PuedeAtacar && !actor.AtacaADistancia =>
                $"{actor.Nombre} ({actor.Clase}) está atrás: cuerpo a cuerpo solo ataca desde adelante.{estado}",
            _ => $"{actor.Nombre} ({actor.Clase}): elegí una acción. {TurnoUnidad.Para(actor, _oleada.Campo).Habilidad}{estado}",
        };
    }

    private static string TextoDecision(JugadaEnemiga jugada)
    {
        var ia = jugada.Enemigo.IARevelada ? jugada.IA : "???";
        var accion = jugada.Decision.Accion switch
        {
            AccionIA.Atacar => $"ataca a {jugada.Objetivo?.Nombre}",
            AccionIA.Avanzar => "avanza",
            AccionIA.Retroceder => "se repliega",
            _ => "espera",
        };
        return $"{jugada.Enemigo.Nombre} [IA {ia}] {accion}: {jugada.Decision.Motivo}";
    }

    private static Label Seccion(string texto)
    {
        var etiqueta = Tema.CrearEtiqueta(texto, Tema.Chica, Tema.TextoTenue);
        etiqueta.Margin = new Padding(3, 10, 3, 4);
        return etiqueta;
    }

    private static ListBox CrearLista(int alto) => new()
    {
        Height = alto,
        Width = 330,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(5, 9, 16),
        ForeColor = Tema.Texto,
        Font = Tema.Normal,
        SelectionMode = SelectionMode.None,
        IntegralHeight = false,
    };

    private static void Llenar(ListBox lista, IEnumerable<string> textos)
    {
        lista.BeginUpdate();
        lista.Items.Clear();
        foreach (var texto in textos)
            lista.Items.Add(texto);
        lista.EndUpdate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _turnoEnjambre.Dispose();
        base.Dispose(disposing);
    }
}
