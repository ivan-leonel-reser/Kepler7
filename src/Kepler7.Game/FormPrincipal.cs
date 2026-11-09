using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Game.Controles;
using Kepler7.Game.Pantallas;
using Kepler7.Services;

namespace Kepler7.Game;

/// <summary>Única ventana del juego: va cambiando la pantalla que muestra.</summary>
public partial class FormPrincipal : Form
{
    private readonly IRepositorioProgreso _repositorio;
    private Pantalla? _pantallaActual;

    public FormPrincipal(IRepositorioProgreso repositorio)
    {
        InitializeComponent();
        _repositorio = repositorio;

        Text = InfoJuego.Titulo;
        ClientSize = new Size(1180, 740);
        MinimumSize = new Size(1100, 750);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Tema.Fondo;

        Mostrar(new PantallaInicio(this));
    }

    internal Partida? PartidaActual { get; private set; }

    // Si la base no responde, "Continuar" queda deshabilitado en vez de tirar la ventana.
    internal bool HayPartidaGuardada
    {
        get
        {
            try
            {
                return _repositorio.Cargar() is not null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    // Una partida nueva arranca por el tutorial; se puede saltar.
    internal void NuevaPartida()
    {
        PartidaActual = Partida.Nueva(_repositorio, new Aleatorio());
        MostrarTutorial(IrAEstacion);
    }

    internal void MostrarTutorial(Action alTerminar) => Mostrar(new PantallaTutorial(this, alTerminar));

    // Si la base se cayó entre que se habilitó "Continuar" y el clic, se avisa y se queda en el inicio.
    internal void ContinuarPartida()
    {
        try
        {
            PartidaActual = Partida.Continuar(_repositorio, new Aleatorio());
        }
        catch (Exception e)
        {
            MessageBox.Show($"No se pudo cargar la partida guardada.\n{e.Message}", InfoJuego.Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        IrAEstacion();
    }

    internal void IrAInicio()
    {
        PartidaActual?.Guardar();
        Mostrar(new PantallaInicio(this));
    }

    internal void IrAEstacion() => Mostrar(new PantallaEstacion(this));

    /// <summary>Después de una derrota: la partida ya se borró y se vuelve al inicio.</summary>
    internal void TerminarPartida()
    {
        PartidaActual = null;
        Mostrar(new PantallaInicio(this));
    }

    internal void Mostrar(Pantalla pantalla)
    {
        foreach (var vieja in Controls.OfType<Transicion>().ToList())
            vieja.Terminar();
        // Sin foto no hay fundido: pasa con la primera pantalla o con la ventana minimizada.
        var anterior = Transicion.Fotografiar(_pantallaActual);

        SuspendLayout();
        if (_pantallaActual is not null)
        {
            Controls.Remove(_pantallaActual);
            _pantallaActual.Dispose();
        }

        _pantallaActual = pantalla;
        Controls.Add(pantalla);
        ResumeLayout();

        if (anterior is not null)
        {
            var transicion = new Transicion(anterior, pantalla);
            Controls.Add(transicion);
            transicion.BringToFront();
            transicion.Empezar();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        PartidaActual?.Guardar();
        base.OnFormClosing(e);
    }
}
