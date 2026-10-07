namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>Un mensaje ya enviado: lo que se escribió, lo que salió por la antena y qué pasó con él.</summary>
public record Transmision(
    string Tipo,
    string Cifrado,
    string TextoPlano,
    string TextoCifrado,
    int Costo,
    int RiesgoIntercepcion,
    Refuerzo Refuerzo)
{
    /// <summary>La carga la partida después de tirar el dado de la intercepción.</summary>
    public bool Interceptada { get; init; }
}
