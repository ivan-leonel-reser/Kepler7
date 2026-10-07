using Kepler7.Core.Juego;
using Kepler7.Core.Persistencia;

namespace Kepler7.Game.Pantallas;

internal class PantallaInicio : Pantalla
{
    public PantallaInicio(FormPrincipal principal) : base(principal)
    {
        var titulo = Tema.CrearEtiqueta(InfoJuego.Titulo, Tema.Titulo, Tema.Acento);
        var subtitulo = Tema.CrearEtiqueta("Estación Orbital Kepler-7 · Sistema de control central", Tema.Subtitulo, Tema.TextoTenue);
        subtitulo.Margin = new Padding(6, 0, 3, 28);

        var equipo = Tema.CrearEtiqueta(InfoJuego.Equipo, Tema.Negrita, Tema.Acento);
        var integrantes = Tema.CrearEtiqueta(string.Join(Environment.NewLine, InfoJuego.Integrantes), Tema.Normal, Tema.Texto);
        integrantes.Margin = new Padding(6, 2, 3, 28);

        var nueva = Tema.CrearBoton("Nueva partida");
        nueva.Click += (_, _) => NuevaPartida();

        var continuar = Tema.CrearBoton("Continuar");
        continuar.Enabled = Principal.HayPartidaGuardada;
        continuar.Click += (_, _) => Principal.ContinuarPartida();

        var salir = Tema.CrearBoton("Salir");
        salir.Click += (_, _) => Principal.Close();

        var columna = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
        };
        columna.Controls.AddRange([titulo, subtitulo, equipo, integrantes, nueva, continuar, salir]);

        // Dónde se guarda la partida. Si SQL Server no responde, acá se entera el jugador.
        var guardado = Tema.CrearEtiqueta($"Guardado: {Repositorios.Descripcion}", Tema.Chica,
            Repositorios.UsaSqlServer ? Tema.TextoTenue : Tema.Advertencia);
        guardado.Dock = DockStyle.Bottom;
        guardado.AutoSize = false;
        guardado.Height = 40;
        guardado.Padding = new Padding(20, 0, 20, 0);
        guardado.TextAlign = ContentAlignment.MiddleLeft;

        Controls.Add(columna);
        Controls.Add(guardado);
        MantenerCentrado(columna);
    }

    private void NuevaPartida()
    {
        if (Principal.HayPartidaGuardada)
        {
            var respuesta = MessageBox.Show(
                "Hay una partida guardada. Si empezás una nueva se pierde el progreso.\n¿Querés seguir?",
                InfoJuego.Titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (respuesta != DialogResult.Yes)
                return;
        }

        Principal.NuevaPartida();
    }
}
