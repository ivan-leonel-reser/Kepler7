using Kepler7.Core.Juego;
using Kepler7.Core.Progreso;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>El hub: desde acá se entra a cada módulo de la estación y a las misiones.</summary>
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

        Controls.Add(CrearCuerpo());
        Controls.Add(panelBitacora);
        Controls.Add(CrearBarraInferior());
        Controls.Add(CrearEncabezado(partida));
    }

    private static Control CrearCuerpo() => new Panel { Dock = DockStyle.Fill };

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
        datos.Controls.AddRange(
        [
            CrearDato("CRÉDITOS", progreso.Creditos.ToString()),
            CrearDato("SECTORES", $"{progreso.SectoresRecuperados}/{ProgresoJugador.SectoresTotales}"),
            CrearDato("INSIGNIAS", $"{progreso.Insignias.Count}/{CatalogoInsignias.Todas.Count}"),
        ]);

        encabezado.Controls.AddRange([titulo, nivel, barraXp, xp, datos]);
        return encabezado;
    }

    private static Control CrearDato(string nombre, string valor)
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
        panel.Controls.Add(Tema.CrearEtiqueta(valor, Tema.Encabezado, Tema.Texto));
        return panel;
    }

    private Control CrearBarraInferior()
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

        var mision = Tema.CrearBoton("Iniciar misión (en desarrollo)", 260);
        mision.Enabled = false;

        barra.Controls.AddRange([menu, mision]);
        return barra;
    }
}
