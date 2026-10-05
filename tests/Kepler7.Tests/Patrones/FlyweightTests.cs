using Kepler7.Core.Patrones.Estructurales.Flyweight;
using Kepler7.Services;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class FlyweightTests
{
    [TestMethod]
    public void LaFabricaDevuelveSiempreElMismoObjetoParaUnaEspecie()
    {
        var fabrica = new FabricaEspecies();

        var primera = fabrica.Obtener("Kepleriano");
        var segunda = fabrica.Obtener("Kepleriano");

        Assert.AreSame(primera, segunda);
        Assert.AreEqual(1, fabrica.EspeciesCreadas);
        Assert.AreEqual(2, fabrica.Pedidos);
    }

    [TestMethod]
    public void LaFabricaCreaUnObjetoPorCadaEspecieDistinta()
    {
        var fabrica = new FabricaEspecies();

        foreach (var nombre in FabricaEspecies.Nombres)
            fabrica.Obtener(nombre);
        foreach (var nombre in FabricaEspecies.Nombres)
            fabrica.Obtener(nombre);

        Assert.AreEqual(5, fabrica.EspeciesCreadas);
        Assert.AreEqual(10, fabrica.Pedidos);
    }

    [TestMethod]
    public void UnaEspecieQueNoExisteNoSeCrea()
    {
        var fabrica = new FabricaEspecies();

        Assert.ThrowsExactly<ArgumentException>(() => fabrica.Obtener("Marciano"));
        Assert.AreEqual(0, fabrica.EspeciesCreadas);
    }

    [TestMethod]
    public void ElCensoTieneDiezMilTripulantesYSoloCincoObjetosDeEspecie()
    {
        var censo = new Censo();

        censo.Generar(new Aleatorio(7));

        Assert.HasCount(10_000, censo.Tripulantes);
        Assert.AreEqual(10_000, censo.Fabrica.Pedidos);
        Assert.AreEqual(5, censo.Fabrica.EspeciesCreadas);
        Assert.AreEqual(5, censo.Tripulantes.Select(t => t.Especie).Distinct().Count());
    }

    [TestMethod]
    public void LosTripulantesDeUnaEspecieCompartenElMismoObjeto()
    {
        var censo = new Censo();
        censo.Generar(new AleatorioFijo(), cantidad: 100);

        var humanos = censo.Tripulantes.Where(t => t.Especie.Nombre == "Humano").ToList();

        Assert.IsGreaterThan(1, humanos.Count);
        Assert.IsTrue(humanos.All(t => ReferenceEquals(t.Especie, humanos[0].Especie)));
        Assert.AreEqual(humanos.Count, censo.CantidadDe(humanos[0].Especie));
    }

    [TestMethod]
    public void CadaTripulanteGuardaLoPropio()
    {
        var censo = new Censo();
        censo.Generar(new Aleatorio(7), cantidad: 200);

        var ids = censo.Tripulantes.Select(t => t.Id).ToList();

        CollectionAssert.AreEqual(Enumerable.Range(1, 200).ToList(), ids);
        Assert.IsGreaterThan(1, censo.Tripulantes.Select(t => t.Nombre).Distinct().Count());
        Assert.IsGreaterThan(1, censo.Tripulantes.Select(t => t.Rango).Distinct().Count());
    }

    [TestMethod]
    public void SinFlyweightHabriaUnObjetoDeEspeciePorTripulante()
    {
        var censo = new Censo();
        censo.Generar(new Aleatorio(7));

        var comparacion = censo.Comparacion!;

        Assert.AreEqual(5, comparacion.EspeciesConFlyweight);
        Assert.AreEqual(10_000, comparacion.EspeciesSinFlyweight);
        Assert.IsGreaterThan(comparacion.BytesConFlyweight, comparacion.BytesSinFlyweight);
        Assert.IsGreaterThan(0, comparacion.BytesAhorrados);
    }

    [TestMethod]
    public void GenerarDosVecesNoDuplicaElPadron()
    {
        var censo = new Censo();
        censo.Generar(new Aleatorio(7), cantidad: 50);
        var primero = censo.Tripulantes[0];

        censo.Generar(new Aleatorio(9), cantidad: 50);

        Assert.HasCount(50, censo.Tripulantes);
        Assert.AreSame(primero, censo.Tripulantes[0]);
    }
}
