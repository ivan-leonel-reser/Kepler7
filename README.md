# Kepler-7: Protocolo Fénix

Videojuego táctico por turnos en C# Windows Forms. Trabajo Práctico Final - *Kepler-7: El universo de los patrones* (UAI, T1-23-32 Trabajo de Diploma).

**Grupo 9:** Maite Stefania Benitez · María Belén Fiuza · Ivan Leonel Reser · Carlos Joel Sánchez Quiroz · Agustin Villaverde

> En desarrollo. Este README se completa en la Fase 4 con instalación del Release, configuración de SQL Server y capturas.

## Requisitos

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (o superior) para compilar - para solo jugar alcanza con el *.NET 8 Desktop Runtime*
- SQL Server LocalDB o Express *(a partir de la Fase 3)*

## Compilar y ejecutar

```bash
dotnet build Kepler7.sln
dotnet test
dotnet run --project src/Kepler7.Game
```

## Estructura

La solución está organizada en cinco capas:

| Proyecto | Capa | Contenido |
|---|---|---|
| `src/Kepler7.BE` | Entidades | Lo que se guarda y viaja entre capas: progreso, insignias, entradas de bitácora y los contratos de persistencia |
| `src/Kepler7.Services` | Servicios transversales | Azar inyectable y bitácora |
| `src/Kepler7.DAL` | Acceso a datos | Repositorios que implementan los contratos de BE |
| `src/Kepler7.Core` | Lógica de negocio | Reglas del juego y los 12 patrones de diseño, sin dependencias de UI |
| `src/Kepler7.Game` | Interfaz | Windows Forms: dibuja, captura la entrada y llama a Core |

Además están `tests/Kepler7.Tests` (pruebas automatizadas, incluidas las de dependencias entre capas) y `db/` (scripts de creación de la base).

Dependencias entre capas, tal como las declaran los `ProjectReference`:

`Game -> Core, BE, Services` · `Core -> DAL, BE, Services` · `DAL -> BE, Services` · `Services -> BE` · `BE` no referencia a ninguna otra.

La interfaz no referencia a la capa de datos: los repositorios se los pide a Core. La lógica y los patrones viven en `Kepler7.Core`, que no referencia a Windows Forms.
