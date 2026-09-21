using Kepler7.Core.Juego;

namespace Kepler7.Game;

public partial class FormPrincipal : Form
{
    public FormPrincipal()
    {
        InitializeComponent();
        Text = InfoJuego.Titulo;
    }
}
