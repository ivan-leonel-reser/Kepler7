using Kepler7.BE;

namespace Kepler7.Core.Patrones.Estructurales.Proxy;

/// <summary>
/// Se pone delante de la bóveda real: verifica el rol de la credencial antes de delegar,
/// registra cada intento denegado y recién abre la bóveda con el primer acceso autorizado.
/// </summary>
public class ProxyBoveda : IBoveda
{
    public const string OperacionLeer = "Leer";
    public const string OperacionEscribir = "Escribir";

    private readonly Func<IBoveda> _abrirBoveda;
    private readonly IRegistroAccesos _registro;
    private IBoveda? _boveda;

    public ProxyBoveda(Func<IBoveda> abrirBoveda, IRegistroAccesos registro)
    {
        _abrirBoveda = abrirBoveda;
        _registro = registro;
    }

    public static IReadOnlyList<Rol> RolesAutorizados { get; } = [Rol.Capitan, Rol.OficialDeSeguridad];

    /// <summary>Quién está operando la bóveda. Sin credencial, todo se rechaza.</summary>
    public Credencial? Credencial { get; set; }

    public bool BovedaAbierta => _boveda is not null;

    public int AccesosConcedidos { get; private set; }

    public IReadOnlyList<IntentoAcceso> IntentosDenegados => _registro.ObtenerTodos();

    public static bool EstaAutorizada(Credencial? credencial) =>
        credencial is not null && RolesAutorizados.Contains(credencial.Rol);

    public string LeerArchivo(string id) => Autorizar(OperacionLeer, id).LeerArchivo(id);

    public void EscribirArchivo(string id, string contenido) => Autorizar(OperacionEscribir, id).EscribirArchivo(id, contenido);

    // Toda operación pasa por acá: o devuelve la bóveda real o corta con el intento registrado.
    private IBoveda Autorizar(string operacion, string id)
    {
        if (!EstaAutorizada(Credencial))
        {
            var intento = new IntentoAcceso(DateTime.Now, Credencial?.Usuario ?? "desconocido", Credencial?.NombreRol ?? "sin credencial", operacion, id);
            _registro.Registrar(intento);
            throw new AccesoDenegadoException(intento);
        }

        AccesosConcedidos++;
        return _boveda ??= _abrirBoveda();
    }
}
