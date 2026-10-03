using Kepler7.Core.Persistencia;

namespace Kepler7.Game;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormPrincipal(Repositorios.CrearProgreso()));
    }
}
