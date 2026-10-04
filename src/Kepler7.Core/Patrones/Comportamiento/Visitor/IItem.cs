namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

/// <summary>Cualquier cosa guardada en el Almacén. Las operaciones no van acá: las trae el visitante.</summary>
public interface IItem
{
    string Nombre { get; }

    /// <summary>Lo que vale sin ningún agregado.</summary>
    int Precio { get; }

    string Descripcion { get; }

    void Aceptar(IVisitor visitante);
}
