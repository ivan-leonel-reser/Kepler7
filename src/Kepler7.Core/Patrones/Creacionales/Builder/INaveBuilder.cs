namespace Kepler7.Core.Patrones.Creacionales.Builder;

/// <summary>
/// Los pasos para armar una nave. Cada receta decide qué pieza pone en cada paso;
/// si un componente opcional no va, el paso no hace nada.
/// </summary>
public interface INaveBuilder
{
    string Nombre { get; }

    string Descripcion { get; }

    /// <summary>Empieza una nave nueva y descarta la que estuviera a medio armar.</summary>
    void Reiniciar();

    void ConstruirCasco();

    void InstalarMotor();

    void InstalarEscudo();

    void InstalarArmas();

    void InstalarRadar();

    void InstalarIA();

    void CargarTanques();

    void MontarBodega();

    Nave ObtenerNave();
}
