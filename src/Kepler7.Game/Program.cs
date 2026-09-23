using Kepler7.Data.Memoria;

namespace Kepler7.Game;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormPrincipal(new RepositorioProgresoEnMemoria()));
    }
}
