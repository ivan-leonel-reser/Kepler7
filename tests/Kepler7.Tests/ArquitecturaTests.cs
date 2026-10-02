using System.Reflection;
using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Core.Persistencia;
using Kepler7.DAL.Memoria;
using Kepler7.Services;

namespace Kepler7.Tests;

[TestClass]
public class ArquitecturaTests
{
    private static readonly Assembly Be = typeof(ProgresoJugador).Assembly;
    private static readonly Assembly Servicios = typeof(Bitacora).Assembly;
    private static readonly Assembly Dal = typeof(RepositorioProgresoEnMemoria).Assembly;
    private static readonly Assembly Core = typeof(Partida).Assembly;

    private static string[] CapasQueUsa(Assembly capa) =>
        capa.GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .Where(n => n.StartsWith("Kepler7."))
            .Order()
            .ToArray();

    [TestMethod]
    public void BE_NoDependeDeNingunaOtraCapa()
    {
        Assert.IsEmpty(CapasQueUsa(Be));
    }

    [TestMethod]
    public void Services_SoloDependeDeBE()
    {
        CollectionAssert.IsSubsetOf(CapasQueUsa(Servicios), new[] { "Kepler7.BE" });
    }

    [TestMethod]
    public void DAL_NoConoceACoreNiALaInterfaz()
    {
        CollectionAssert.IsSubsetOf(CapasQueUsa(Dal), new[] { "Kepler7.BE", "Kepler7.Services" });
    }

    [TestMethod]
    public void Core_NoDependeDeLaInterfazNiDeWinForms()
    {
        var referencias = Core.GetReferencedAssemblies().Select(a => a.Name ?? "").ToList();

        CollectionAssert.IsSubsetOf(CapasQueUsa(Core), new[] { "Kepler7.BE", "Kepler7.DAL", "Kepler7.Services" });
        Assert.IsFalse(referencias.Any(n => n is "System.Windows.Forms" or "System.Drawing" or "System.Drawing.Common"));
    }

    [TestMethod]
    public void CoreEntregaElRepositorioQueLaInterfazNoPuedeCrear()
    {
        var repositorio = Repositorios.CrearProgreso();
        var progreso = new ProgresoJugador();
        progreso.GanarCreditos(40);

        repositorio.Guardar(progreso);

        Assert.AreEqual(40, repositorio.Cargar()?.Creditos);
        Assert.AreSame(Dal, repositorio.GetType().Assembly);
    }
}
