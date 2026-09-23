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

| Carpeta | Contenido |
|---|---|
| `src/Kepler7.Core` | Lógica del juego y los 12 patrones de diseño (sin dependencias de UI) |
| `src/Kepler7.Data` | Persistencia en SQL Server |
| `src/Kepler7.Game` | Interfaz Windows Forms |
| `tests/Kepler7.Tests` | Pruebas automatizadas de Core |
| `db/` | Scripts de creación de la base |
| `docs/` | Consigna, UML, informe, guía de Git |

## Para el equipo

- [PLAN.md](PLAN.md) - plan, reparto de patrones y fechas
- [docs/REGLAS.md](docs/REGLAS.md) - reglas del proyecto
- [docs/GUIA_GIT.md](docs/GUIA_GIT.md) - cómo clonar, crear tu rama y abrir un PR
- [docs/SUMARSE_AL_REPO.md](docs/SUMARSE_AL_REPO.md) - cómo dar de alta tu cuenta en la PC del grupo
