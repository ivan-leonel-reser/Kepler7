using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Estructurales.Facade;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>
/// El formulario solo llama a las tres operaciones de <see cref="ControlCentral"/>.
/// La secuencia que devuelve se muestra paso a paso para que se vea cómo coordina a cada subsistema.
/// </summary>
internal class PantallaControlCentral : PantallaModulo
{
    private readonly ControlCentral _control;
    private readonly Dictionary<string, TarjetaSubsistema> _tarjetas = [];
    private readonly ConsolaBitacora _consola = new() { Dock = DockStyle.Fill };
    private readonly Queue<EntradaBitacora> _pendientes = new();
    private readonly System.Windows.Forms.Timer _animacion = new() { Interval = 350 };
    private readonly Label _modo = Tema.CrearEtiqueta("", Tema.Destacado);
    private readonly Button[] _operaciones;
    private Dictionary<string, EstadoSubsistema> _estadoFinal = [];

    public PantallaControlCentral(FormPrincipal principal)
        : base(principal, "Control Central", Patron.Facade,
            "Una sola orden y la fachada coordina a los seis subsistemas de la estación.")
    {
        _control = Partida.ControlCentral;

        var iniciar = Tema.CrearBoton("Iniciar turno");
        iniciar.Click += (_, _) => Ejecutar(_control.IniciarTurno);
        var emergencia = Tema.CrearBoton("Entrar en emergencia");
        emergencia.Click += (_, _) => Ejecutar(_control.EntrarEnEmergencia);
        var apagar = Tema.CrearBoton("Apagar estación");
        apagar.Click += (_, _) => Ejecutar(_control.ApagarEstacion);
        _operaciones = [iniciar, emergencia, apagar];

        _modo.Margin = new Padding(24, 12, 0, 0);
        var botones = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(0, 12, 0, 0) };
        botones.Controls.AddRange([iniciar, emergencia, apagar, _modo]);

        var tarjetas = new TableLayoutPanel { Dock = DockStyle.Left, Width = 460, ColumnCount = 2, RowCount = 3 };
        tarjetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tarjetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var i = 0; i < 3; i++)
            tarjetas.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 3));
        foreach (var estado in _control.Subsistemas)
        {
            var tarjeta = new TarjetaSubsistema(estado);
            _tarjetas[estado.Nombre] = tarjeta;
            tarjetas.Controls.Add(tarjeta);
        }

        var zonaConsola = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 6, 0, 6) };
        zonaConsola.Controls.Add(_consola);
        zonaConsola.Controls.Add(new Label
        {
            Text = "SECUENCIA COORDINADA POR LA FACHADA",
            Dock = DockStyle.Top,
            Height = 22,
            Font = Tema.Chica,
            ForeColor = Tema.TextoTenue,
        });
        _consola.BringToFront();

        Contenido.Controls.Add(zonaConsola);
        Contenido.Controls.Add(tarjetas);
        Contenido.Controls.Add(botones);

        _animacion.Tick += (_, _) => MostrarSiguientePaso();
        ActualizarModo();
    }

    private void Ejecutar(Func<IReadOnlyList<EntradaBitacora>> operacion)
    {
        var secuencia = operacion();
        _estadoFinal = _control.Subsistemas.ToDictionary(s => s.Nombre);
        foreach (var entrada in secuencia)
            _pendientes.Enqueue(entrada);

        foreach (var boton in _operaciones)
            boton.Enabled = false;
        MostrarSiguientePaso();
        _animacion.Start();
    }

    private void MostrarSiguientePaso()
    {
        foreach (var tarjeta in _tarjetas.Values)
            tarjeta.Resaltada = false;

        if (!_pendientes.TryDequeue(out var entrada))
        {
            _animacion.Stop();
            TerminarSecuencia();
            return;
        }

        _consola.Agregar(entrada);
        // Cada tarjeta cambia recién cuando la fachada llega a ese subsistema.
        if (_tarjetas.TryGetValue(entrada.Origen, out var afectada) && _estadoFinal.TryGetValue(entrada.Origen, out var estado))
        {
            afectada.Mostrar(estado);
            afectada.Resaltada = true;
        }
    }

    private void TerminarSecuencia()
    {
        ActualizarModo();
        foreach (var insignia in Partida.RevisarLogros())
            _consola.Agregar(new EntradaBitacora("Logros",
                $"Insignia desbloqueada: {insignia.Nombre} (+{Partida.XpPorInsignia} XP, +{Partida.CreditosPorInsignia} créditos)",
                TipoEntrada.Exito));

        foreach (var boton in _operaciones)
            boton.Enabled = true;
    }

    private void ActualizarModo()
    {
        (_modo.Text, _modo.ForeColor) = _control.Modo switch
        {
            ModoEstacion.Operativa => ("ESTACIÓN OPERATIVA", Tema.Exito),
            ModoEstacion.Emergencia => ("EMERGENCIA", Tema.Peligro),
            _ => ("ESTACIÓN APAGADA", Tema.TextoTenue),
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _animacion.Dispose();
        base.Dispose(disposing);
    }
}
