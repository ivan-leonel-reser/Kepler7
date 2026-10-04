using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Creacionales.Prototype;
using Kepler7.Core.Patrones.Estructurales.Flyweight;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class PrototypeTests
{
    private FichaSoldado _original = null!;

    [TestInitialize]
    public void Preparar()
    {
        _original = new FichaSoldado("Rojas", ClaseUnidad.Soldado);
        _original.Aprender("Tiro certero");
    }

    private FichaSoldado Clon() => (FichaSoldado)_original.Clonar();

    [TestMethod]
    public void ElClonArrancaConLosMismosDatosQueElOriginal()
    {
        var clon = Clon();

        Assert.AreEqual(_original.Nombre, clon.Nombre);
        Assert.AreEqual(_original.Clase, clon.Clase);
        CollectionAssert.AreEqual(_original.Habilidades, clon.Habilidades);
    }

    [TestMethod]
    public void ElClonEsOtroObjetoConOtraLista()
    {
        var clon = Clon();

        Assert.AreNotSame(_original, clon);
        Assert.AreNotSame(_original.Habilidades, clon.Habilidades);
    }

    [TestMethod]
    public void CambiarLasHabilidadesDelClonNoCambiaAlOriginal()
    {
        var clon = Clon();

        clon.Aprender("Resistencia");
        clon.Olvidar("Tiro certero");

        CollectionAssert.AreEqual(new[] { "Carga", "Tiro certero" }, _original.Habilidades);
        CollectionAssert.AreEqual(new[] { "Carga", "Resistencia" }, clon.Habilidades);
    }

    [TestMethod]
    public void CambiarElNombreDelClonNoCambiaAlOriginal()
    {
        var clon = Clon();

        clon.Nombre = "Rojas II";

        Assert.AreEqual("Rojas", _original.Nombre);
    }

    [TestMethod]
    public void CambiarAlOriginalDespuesDeClonarNoCambiaAlClon()
    {
        var clon = Clon();

        _original.Aprender("Piel reforzada");

        Assert.HasCount(2, clon.Habilidades);
    }

    [TestMethod]
    public void LaEspecieDelClonEsElMismoObjetoCompartido()
    {
        var especie = new FabricaEspecies().Obtener("Abisal");
        var recluta = new FichaSoldado("Tania", ClaseUnidad.Soldado, especie);

        var clon = (FichaSoldado)recluta.Clonar();

        Assert.AreSame(especie, clon.Especie);
    }

    [TestMethod]
    public void LasHabilidadesEntrenadasLeSumanALaUnidadQueSaleAPelear()
    {
        var soldadoBase = UnidadEscuadron.Crear("Base", ClaseUnidad.Soldado);
        var clon = Clon();
        clon.Aprender("Resistencia");

        var unidad = clon.CrearUnidad();

        Assert.AreEqual(soldadoBase.VidaMaxima + 8, unidad.VidaMaxima);
        Assert.AreEqual(soldadoBase.Ataque + 2, unidad.Ataque);
        Assert.AreEqual(ClaseUnidad.Soldado, unidad.Clase);
    }

    [TestMethod]
    public void LaHabilidadDeLaClaseNoSeOlvidaYHayUnMaximo()
    {
        var clon = Clon();
        clon.Aprender("Resistencia");

        Assert.IsFalse(clon.PuedeOlvidar("Carga"));
        Assert.IsFalse(clon.PuedeAprender("Golpe pesado"));
        Assert.ThrowsExactly<InvalidOperationException>(() => clon.Aprender("Golpe pesado"));
        Assert.HasCount(FichaSoldado.MaximoHabilidades, clon.Habilidades);
    }

    [TestMethod]
    public void NoSeAprendeDosVecesLoMismoNiAlgoQueNoEstaEnElCatalogo()
    {
        var clon = Clon();

        Assert.IsFalse(clon.PuedeAprender("Tiro certero"));
        Assert.IsFalse(clon.PuedeAprender("Telepatía"));
    }
}
