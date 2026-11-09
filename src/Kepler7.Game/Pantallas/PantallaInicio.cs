using Kepler7.Core.Juego;
using Kepler7.Core.Persistencia;

namespace Kepler7.Game.Pantallas;

internal class PantallaInicio : Pantalla
{
    public PantallaInicio(FormPrincipal principal) : base(principal)
    {
        ConCielo = true;
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

        var comoSeJuega = Tema.CrearBoton("Cómo se juega");
        comoSeJuega.Click += (_, _) => Principal.MostrarTutorial(Principal.IrAInicio, "Volver al menú");

        var historial = Tema.CrearBoton("Historial de partidas");
        historial.Click += (_, _) => Principal.Mostrar(new PantallaHistorial(Principal));

        var sonido = Tema.CrearBoton(TextoSonido());
        sonido.Click += (_, _) =>
        {
            Sonidos.Activados = !Sonidos.Activados;
            sonido.Text = TextoSonido();
            Sonidos.Insignia();
        };

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
        columna.Controls.AddRange([titulo, subtitulo, equipo, integrantes, nueva, continuar, comoSeJuega, historial, sonido, salir]);

        // Dónde se guarda la partida. Si SQL Server no responde, acá se entera el jugador;
        // el motivo completo es largo y queda en el historial.
        var guardado = Tema.CrearEtiqueta(
            Repositorios.UsaSqlServer
                ? $"Guardado: {Repositorios.Descripcion}"
                : "Guardado: en memoria, se pierde al cerrar el juego. El motivo está en Historial de partidas.",
            Tema.Chica, Repositorios.UsaSqlServer ? Tema.TextoTenue : Tema.Advertencia);
        guardado.Dock = DockStyle.Bottom;
        guardado.AutoSize = false;
        guardado.Height = 40;
        guardado.Padding = new Padding(20, 0, 20, 0);
        guardado.TextAlign = ContentAlignment.MiddleLeft;

        Controls.Add(columna);
        Controls.Add(guardado);
        MantenerCentrado(columna);
    }

    private static string TextoSonido() => Sonidos.Activados ? "Sonido: activado" : "Sonido: apagado";

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
