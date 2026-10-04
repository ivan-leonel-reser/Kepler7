namespace Kepler7.Core.Patrones.Estructurales.Flyweight;

/// <summary>Lo que se midió al armar el mismo padrón compartiendo las especies y sin compartirlas.</summary>
public record ComparacionMemoria(
    int Tripulantes,
    int EspeciesConFlyweight,
    int EspeciesSinFlyweight,
    long BytesConFlyweight,
    long BytesSinFlyweight)
{
    public long BytesAhorrados => BytesSinFlyweight - BytesConFlyweight;
}
