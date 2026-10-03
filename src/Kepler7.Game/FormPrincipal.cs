using Kepler7.BE;
using Kepler7.Core.Juego;
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

    internal bool HayPartidaGuardada => _repositorio.Cargar() is not null;

    internal void NuevaPartida()
    {
        PartidaActual = Partida.Nueva(_repositorio, new Aleatorio());
        IrAEstacion();
    }

    internal void ContinuarPartida()
    {
        PartidaActual = Partida.Continuar(_repositorio, new Aleatorio());
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
        SuspendLayout();
        if (_pantallaActual is not null)
        {
            Controls.Remove(_pantallaActual);
            _pantallaActual.Dispose();
        }

        _pantallaActual = pantalla;
        Controls.Add(pantalla);
        ResumeLayout();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        PartidaActual?.Guardar();
        base.OnFormClosing(e);
    }
}
