using Kepler7.Core.Patrones.Creacionales.Builder;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class BuilderTests
{
    private readonly IngenieroJefe _ingeniero = new();

    [TestMethod]
    public void Exploradora_LlevaRadarYVaDesarmada()
    {
        var nave = _ingeniero.Ensamblar(new RecetaExploradora());

        Assert.AreEqual("Exploradora", nave.Modelo);
        Assert.IsTrue(nave.Radar);
        Assert.AreEqual(0, nave.Armas);
        Assert.AreEqual(0, nave.Bodega);
        Assert.IsTrue(nave.RevelaIA);
        Assert.AreEqual(0, nave.DanioDeApoyo);
    }

    [TestMethod]
    public void Combate_VaArmadaYSinRadar()
    {
        var nave = _ingeniero.Ensamblar(new RecetaCombate());

        Assert.IsGreaterThan(0, nave.Armas);
        Assert.IsFalse(nave.Radar);
        Assert.AreEqual(nave.Armas, nave.DanioDeApoyo);
        Assert.AreEqual(0, nave.BonusCreditos);
    }

    [TestMethod]
    public void Carga_LlevaBodegaYNoTieneIA()
    {
        var nave = _ingeniero.Ensamblar(new RecetaCarga());

        Assert.IsGreaterThan(0, nave.Bodega);
        Assert.IsNull(nave.IA);
        Assert.AreEqual(50, nave.BonusCreditos);
        Assert.IsFalse(nave.RevelaIA);
    }

    [TestMethod]
    public void TodasLasNavesTienenCascoYMotorYSeArmanEnEseOrden()
    {
        foreach (var receta in CatalogoRecetas.Crear())
        {
            var nave = _ingeniero.Ensamblar(receta);

            Assert.AreNotEqual("", nave.Casco, receta.Nombre);
            Assert.AreNotEqual("", nave.Motor, receta.Nombre);
            Assert.AreEqual("Casco", nave.Piezas[0].Componente, receta.Nombre);
            Assert.AreEqual("Motor", nave.Piezas[1].Componente, receta.Nombre);
        }
    }

    [TestMethod]
    public void LosOpcionalesQueNoVanNoAparecenEntreLasPiezas()
    {
        var carga = _ingeniero.Ensamblar(new RecetaCarga());

        Assert.IsFalse(carga.Piezas.Any(p => p.Componente is "Armas" or "Radar" or "IA de a bordo"));
        Assert.IsTrue(carga.Piezas.Any(p => p.Componente == "Bodega"));
    }

    [TestMethod]
    public void ElIngenieroLlamaLosPasosSiempreEnElMismoOrden()
    {
        var receta = new RecetaQueAnota();

        _ingeniero.Ensamblar(receta);

        string[] esperado = ["Reiniciar", "Casco", "Motor", "Escudo", "Armas", "Radar", "IA", "Tanques", "Bodega", "Obtener"];
        CollectionAssert.AreEqual(esperado, receta.Pasos);
    }

    [TestMethod]
    public void LaMismaRecetaArmaUnaNaveNuevaCadaVez()
    {
        var receta = new RecetaCombate();

        var primera = _ingeniero.Ensamblar(receta);
        var segunda = _ingeniero.Ensamblar(receta);

        Assert.AreNotSame(primera, segunda);
        Assert.HasCount(primera.Piezas.Count, segunda.Piezas);
    }

    private class RecetaQueAnota : INaveBuilder
    {
        public List<string> Pasos { get; } = [];
        public string Nombre => "Prueba";
        public string Descripcion => "";

        public void Reiniciar() => Pasos.Add("Reiniciar");
        public void ConstruirCasco() => Pasos.Add("Casco");
        public void InstalarMotor() => Pasos.Add("Motor");
        public void InstalarEscudo() => Pasos.Add("Escudo");
        public void InstalarArmas() => Pasos.Add("Armas");
        public void InstalarRadar() => Pasos.Add("Radar");
        public void InstalarIA() => Pasos.Add("IA");
        public void CargarTanques() => Pasos.Add("Tanques");
        public void MontarBodega() => Pasos.Add("Bodega");

        public Nave ObtenerNave()
        {
            Pasos.Add("Obtener");
            return new Nave(Nombre);
        }
    }
}
