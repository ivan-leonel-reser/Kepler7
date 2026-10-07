using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Estructurales.Facade;
using Kepler7.Core.Progreso;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>El hub: desde acá se entra a cada módulo de la estación y a defender la oleada.</summary>
internal class PantallaEstacion : Pantalla
{
    public PantallaEstacion(FormPrincipal principal) : base(principal)
    {
        var partida = principal.PartidaActual
            ?? throw new InvalidOperationException("No hay una partida en curso.");

        var bitacora = new ConsolaBitacora { Dock = DockStyle.Fill };
        bitacora.AgregarTodas(partida.Bitacora.Entradas);

        var panelBitacora = new Panel { Dock = DockStyle.Bottom, Height = 170, Padding = new Padding(20, 6, 20, 0) };
        panelBitacora.Controls.Add(bitacora);
        panelBitacora.Controls.Add(new Label
        {
            Text = "BITÁCORA DE LA ESTACIÓN",
            Dock = DockStyle.Top,
            Height = 22,
            Font = Tema.Chica,
            ForeColor = Tema.TextoTenue,
        });
        bitacora.BringToFront();

        Controls.Add(CrearModulos(partida));
        Controls.Add(panelBitacora);
        Controls.Add(CrearBarraInferior(partida));
        Controls.Add(CrearEncabezado(partida));
    }

    private Control CrearModulos(Partida partida)
    {
        const int columnas = 4;
        var modulos = ModulosEstacion.Todos;
        var filas = (modulos.Count + columnas - 1) / columnas;

        var grilla = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = columnas,
            RowCount = filas,
            Padding = new Padding(12, 12, 12, 0),
        };
        for (var c = 0; c < columnas; c++)
            grilla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columnas));
        for (var f = 0; f < filas; f++)
            grilla.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / filas));

        foreach (var modulo in modulos)
        {
            var tarjeta = new TarjetaModulo(modulo, partida.Progreso.TieneInsignia(modulo.Patron));
            tarjeta.Abrir += (_, m) => Principal.Mostrar(m.Abrir!(Principal));
            grilla.Controls.Add(tarjeta);
        }

        return grilla;
    }

    private static Control CrearEncabezado(Partida partida)
    {
        var progreso = partida.Progreso;

        var encabezado = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Tema.Panel, Padding = new Padding(20, 12, 20, 8) };

        var titulo = Tema.CrearEtiqueta("Puente de mando · Kepler-7", Tema.Encabezado, Tema.Acento);
        titulo.Location = new Point(20, 10);

        var nivel = Tema.CrearEtiqueta($"Nivel {progreso.Nivel}", Tema.Negrita);
        nivel.Location = new Point(22, 52);

        var barraXp = new BarraProgreso
        {
            Maximo = ProgresoJugador.XpPorNivel,
            Valor = progreso.XpEnNivelActual,
            Location = new Point(90, 56),
        };

        var xp = Tema.CrearEtiqueta($"{progreso.XpEnNivelActual}/{ProgresoJugador.XpPorNivel} XP", Tema.Chica, Tema.TextoTenue);
        xp.Location = new Point(278, 53);

        var proximo = Desbloqueos.Proximo(progreso.Nivel);
        var desbloqueo = Tema.CrearEtiqueta(proximo is null
            ? "Todas las ventajas de nivel desbloqueadas"
            : $"Nivel {proximo.Nivel} desbloquea: {proximo.Nombre} (ver Logros)",
            Tema.Chica, Tema.Advertencia);
        desbloqueo.Location = new Point(370, 53);

        var datos = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 20, 0, 0),
            BackColor = Color.Transparent,
        };
        var (modo, colorModo) = partida.ControlCentral.Modo switch
        {
            ModoEstacion.Operativa => ("OPERATIVA", Tema.Exito),
            ModoEstacion.Emergencia => ("EMERGENCIA", Tema.Peligro),
            _ => ("APAGADA", Tema.TextoTenue),
        };

        datos.Controls.AddRange(
        [
            CrearDato("ESTACIÓN", modo, colorModo),
            CrearDato("CRÉDITOS", progreso.Creditos.ToString()),
            CrearDato("OLEADAS", $"{progreso.OleadasSuperadas}/{ProgresoJugador.OleadasTotales}"),
            CrearDato("INSIGNIAS", $"{progreso.Insignias.Count}/{CatalogoInsignias.Todas.Count}"),
        ]);

        encabezado.Controls.AddRange([titulo, nivel, barraXp, xp, desbloqueo, datos]);
        return encabezado;
    }

    private static Control CrearDato(string nombre, string valor, Color? color = null)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(24, 0, 0, 0),
            BackColor = Color.Transparent,
        };
        panel.Controls.Add(Tema.CrearEtiqueta(nombre, Tema.Chica, Tema.TextoTenue));
        panel.Controls.Add(Tema.CrearEtiqueta(valor, Tema.Encabezado, color ?? Tema.Texto));
        return panel;
    }

    private Control CrearBarraInferior(Partida partida)
    {
        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(16, 8, 16, 8),
        };

        var menu = Tema.CrearBoton("Menú principal", 180);
        menu.Click += (_, _) => Principal.IrAInicio();

        var logros = Tema.CrearBoton($"Logros ({partida.Progreso.Insignias.Count}/{CatalogoInsignias.Todas.Count})", 180);
        logros.Click += (_, _) => Principal.Mostrar(new PantallaLogros(Principal));

        var defender = Tema.CrearBoton(TextoDefender(partida), 340);
        defender.Enabled = partida.PuedeDefender;
        defender.Click += (_, _) => Principal.Mostrar(new PantallaOleada(Principal));

        barra.Controls.AddRange([menu, logros, defender]);
        return barra;
    }

    private static string TextoDefender(Partida partida)
    {
        if (partida.Progreso.EstacionSalvada)
            return "Kepler-7 está a salvo";
        return partida.ControlCentral.Modo == ModoEstacion.Operativa
            ? $"Defender la oleada {partida.NumeroProximaOleada}"
            : $"Oleada {partida.NumeroProximaOleada}: primero iniciá un turno";
    }
}
