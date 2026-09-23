# Kepler-7 - Reglas del proyecto

TP Final UAI: videojuego táctico por turnos en **C# Windows Forms** que implementa 12 patrones de diseño.
La consigna completa está en `docs/TP Final.pdf`. El plan, el reparto y el estado de avance están en [PLAN.md](../PLAN.md).

> Si una regla cambia, se cambia acá por PR, no "de palabra".

## Stack y restricciones duras (vienen de la consigna)

- **.NET 8**, C# 12, Windows Forms. Persistencia en **SQL Server** (LocalDB / Express).
- **Prohibido**: motores de juego, frameworks gráficos y paquetes NuGet de terceros. Solo bibliotecas de .NET.
  - Únicas excepciones admitidas (ambas de Microsoft, **a confirmar con la cátedra**): `Microsoft.Data.SqlClient` (en .NET 8 el cliente SQL no viene en el runtime) y `MSTest` para `Kepler7.Tests`.
  - Antes de agregar cualquier otro `PackageReference`: no. Se resuelve con la BCL.
- `Kepler7.Core` **no referencia** `System.Windows.Forms` ni `System.Drawing`. Nada de `Form`, `Control`, `Color`, `MessageBox`, `Timer` de WinForms en Core. Los colores se modelan como `string` hex (`"#33FF99"`).
- `Kepler7.Game` contiene **solo UI**: dibuja, captura input y llama a Core. Ninguna regla de juego vive en un `Form`.
- Los 12 patrones van en Core, con los **nombres exactos** que pide la consigna (ver tabla en PLAN.md). No renombrar `INaveBuilder`, `IngenieroJefe`, `ControlCentral`, `ProxyBoveda`, `GestorComandos`, etc.
- Cada patrón tiene que **usarse en una mecánica real de la partida** y **verse en la interfaz**. Clase que no se usa jugando = patrón no entregado.

## Arquitectura

```
Kepler7.sln
├─ src/Kepler7.Core    (classlib net8.0)          lógica del juego + 12 patrones + interfaces de persistencia
├─ src/Kepler7.Data    (classlib net8.0)          implementación SQL Server de las interfaces de Core
├─ src/Kepler7.Game    (WinForms net8.0-windows)  pantallas, render, input
├─ tests/Kepler7.Tests (net8.0)                   pruebas de Core
├─ db/                 scripts .sql numerados
└─ docs/               uml/ (13 diagramas), informe/, capturas/
```

- Dependencias: `Game → Core, Data` · `Data → Core` · `Tests → Core`. **Core no depende de nadie.**
- Core le habla a la UI por **valores de retorno, eventos y bitácoras** (`IReadOnlyList<EntradaBitacora>`), nunca al revés. Todo lo que la consigna pide "mostrar en la interfaz" (secuencia de la fachada, recorrido del daño, etapas del turno, historial de comandos) sale de Core como datos y el Form solo los pinta.
- El Form **nunca** invoca subsistemas de la estación directamente: solo `ControlCentral` (requisito de Facade).
- Azar inyectado: Core usa `IAleatorio`, nunca `new Random()` suelto (Esquiva e `IAAleatoria` tienen que ser testeables).
- Persistencia detrás de interfaces definidas en Core (`IRepositorioProgreso`, `IRegistroAccesos`). Core no conoce SQL.
- Cadena de conexión en `appsettings.json` junto al ejecutable, con seguridad integrada. **Nunca** usuario/contraseña en el repo.
- No bloquear el hilo de UI: animaciones con `Timer`, trabajo pesado (ej. generar 10.000 tripulantes) con `Task`/`await`.

## Convenciones de código

- Código, nombres y comentarios **en español** (coherente con los nombres que exige la consigna). Sin tildes ni `ñ` en identificadores (`Danio`, `Boveda`).
- `PascalCase` tipos y miembros públicos, `_camelCase` campos privados, interfaces con `I`.
- Un tipo por archivo. Carpeta por patrón: `Core/Patrones/<Familia>/<Patron>/`.
- `Nullable` habilitado y warnings como errores en Core. Comentarios solo donde el porqué no es obvio.
- Textos visibles al jugador en español rioplatense neutro, tono de ciencia ficción sobrio.

## Git - reglas del equipo

- **El autor de un commit es quien hizo ese trabajo.** Compartimos una PC de desarrollo, así que cada uno commitea con su nombre y el mail de su cuenta de GitHub cuando le toca trabajar (ver [GUIA_GIT.md](GUIA_GIT.md)).
- Nadie commitea a nombre de otro. Si dos trabajaron juntos, se agrega `Co-authored-by: Nombre <email>`.
- `main` siempre compila y pasa los tests. No se pushea directo a `main` (salvo el setup inicial): rama + PR + aprobación de **otro** integrante.
- Ramas: `feature/<patron-o-tema>`, `fix/<tema>`, `docs/<tema>`. Ej.: `feature/chain-defensas`.
- Commits chicos y significativos, en español, estilo Conventional Commits: `feat(builder): agrega RecetaCarga`, `test(proxy): cubre acceso denegado`, `docs(uml): diagrama de State`.
- Cada parte se hace en **varios commits**, uno por paso que compila (modelo, lógica con sus tests, pantalla, UML e informe). Nada de un único commit gigante al final.
- No se commitea: `bin/`, `obj/`, `.vs/`, `*.user`, bases `.mdf/.ldf`, ejecutables (el Release va como *GitHub Release*, no en el árbol).
- Cada integrante es **dueño** de sus patrones (tabla en PLAN.md): los implementa, los testea, hace su pantalla y su diagrama UML, y tiene que poder defenderlos en la exposición.

## Cada parte tiene que funcionar

Lo de cada integrante se prueba jugando, no solo compilando. Así vamos probando a medida que avanzamos.

- Todo lo que se mergea a `main` deja el juego andando: compila, arranca y lo nuevo **se usa desde la interfaz**. No se mergean clases sueltas "para conectar después".
- Cada módulo tiene su botón en la Estación (`ModulosEstacion` en `Kepler7.Game`). Queda deshabilitado hasta que su dueño lo termina y lo habilita en el mismo PR.
- Si un patrón depende de algo que todavía no existe (por ejemplo, el combate), primero se prueba en la pantalla de su módulo y en la Fase 3 se integra a la misión.
- Antes de pedir revisión: `dotnet test` en verde y probar a mano el recorrido Inicio → Estación → módulo. Quien revisa el PR también lo prueba.

## Definición de "terminado" para un patrón

1. Clases en Core con los nombres de la consigna y todos sus requisitos técnicos cumplidos.
2. Integrado a una mecánica que el jugador usa durante la partida.
3. Visible en la UI (incluye la "prueba visible" cuando la consigna la pide: Prototype, Flyweight, Proxy…).
4. Al menos una prueba automatizada en `Kepler7.Tests`.
5. Diagrama UML en `docs/uml/` y su párrafo de justificación en `docs/informe/patrones.md`.
6. Insignia del patrón conectada al panel de logros.
7. Casilla tildada en PLAN.md en el mismo PR.

## Comandos

```bash
dotnet build Kepler7.sln
dotnet test
dotnet run --project src/Kepler7.Game
dotnet publish src/Kepler7.Game -c Release -r win-x64 --self-contained false
```
