namespace Kepler7.Core.Patrones.Creacionales.Prototype;

/// <summary>Algo que se le puede enseñar a un clon en el Laboratorio y lo que le suma.</summary>
public record Habilidad(string Nombre, int BonusVida, int BonusAtaque)
{
    public string Descripcion =>
        (BonusVida, BonusAtaque) switch
        {
            ( > 0, > 0) => $"+{BonusVida} de vida y +{BonusAtaque} de ataque",
            ( > 0, _) => $"+{BonusVida} de vida",
            _ => $"+{BonusAtaque} de ataque",
        };
}
