namespace Kepler7.Core.Patrones.Estructurales.Proxy;

/// <summary>Con qué identidad se presenta alguien ante la bóveda.</summary>
public record Credencial(string Usuario, Rol Rol)
{
    public string NombreRol => Rol switch
    {
        Rol.Capitan => "Capitán",
        Rol.OficialDeSeguridad => "Oficial de Seguridad",
        Rol.Medico => "Médico",
        Rol.Soldado => "Soldado",
        _ => "Cadete",
    };

    public override string ToString() => $"{Usuario} · {NombreRol}";
}
