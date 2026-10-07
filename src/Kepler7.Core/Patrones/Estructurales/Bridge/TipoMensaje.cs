namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>Un tipo de mensaje tal como se ofrece en Comunicaciones. Crear arma el mensaje con el cifrado elegido.</summary>
public record TipoMensaje(string Nombre, string Descripcion, string ContenidoSugerido, Func<string, ICifrado, Mensaje> Crear);
