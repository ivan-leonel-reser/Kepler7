namespace Kepler7.Core.Patrones.Creacionales.Builder;

/// <summary>Director: sabe en qué orden se arma una nave, pero no qué piezas lleva cada receta.</summary>
public class IngenieroJefe
{
    public Nave Ensamblar(INaveBuilder receta)
    {
        receta.Reiniciar();

        // Primero la estructura y la propulsión; todo lo demás va montado encima.
        receta.ConstruirCasco();
        receta.InstalarMotor();
        receta.InstalarEscudo();
        receta.InstalarArmas();
        receta.InstalarRadar();
        receta.InstalarIA();
        receta.CargarTanques();
        receta.MontarBodega();

        return receta.ObtenerNave();
    }
}
