# Kepler-7 - Plan de trabajo

> Borrador v0. Las reglas de código y de Git están en [docs/REGLAS.md](docs/REGLAS.md). La consigna es `docs/TP Final.pdf`.

## 1. Decisiones tomadas

| Tema | Decisión | Estado |
|---|---|---|
| Plataforma | .NET 8 + Windows Forms | a confirmar con la cátedra (la consigna acepta .NET 8 o Framework 4.8 "según lo acordado") |
| Género | Táctico por turnos en grilla | definido |
| Persistencia | SQL Server (LocalDB/Express) + script de creación | definido; falta confirmar que `Microsoft.Data.SqlClient` (NuGet de Microsoft, no de terceros) está permitido |
| Nivel objetivo | **Comandante** (tests, Release, identidad propia) | definido |
| Tests | MSTest en `Kepler7.Tests` | a confirmar; plan B: runner de consola propio sin paquetes |
| UML | PlantUML en `docs/uml/*.puml` (texto versionable, cada uno commitea el suyo) | propuesta |
| Equipo | Grupo 9 · T1-23-32 Trabajo de Diploma · 5 integrantes | definido |
| Entrega | **Sábado 14/11/2026** | definido |

## 2. El juego

**Título de trabajo:** *Kepler-7: Protocolo Fénix*

**Premisa.** Una entidad hostil, *el Enjambre*, tomó los seis subsistemas de la estación: Energía, Soporte Vital, Navegación, Comunicaciones, Seguridad y Defensa. El jugador es el Comandante convocado por el Consejo de Ingeniería: tiene que recuperar los seis sectores uno por uno y después purgar el Núcleo.

**Ciclo de juego.**

```
ESTACIÓN (preparación)  →  MISIÓN (combate táctico)  →  RECOMPENSAS  →  guardado  →  siguiente sector
```

- **Estación (hub):** módulos donde se prepara la misión - Hangar, Laboratorio, Comunicaciones, Control Central, Censo, Bóveda, Almacén, Armería.
- **Misión:** combate por turnos en grilla, escuadrón del jugador vs. unidades del Enjambre.
- **Recompensas:** XP, créditos, botín, insignias de patrones. Subir de nivel desbloquea recetas, clases y sectores.
- **Victoria:** recuperar los 6 sectores y vencer en el Núcleo. **Derrota:** la integridad de la estación llega a 0 o el escuadrón es eliminado sin créditos para reponerlo.

## 3. Los 12 patrones → mecánicas

| # | Patrón | Dónde vive en el juego | Qué ve el jugador | Tipos obligatorios |
|---|---|---|---|---|
| 1 | **Builder** | Hangar: se ensambla la nave de apoyo de la misión | Elige receta, ve la construcción paso a paso y la ficha de la nave; la nave da bonus en la misión (radar / fuego de apoyo / más botín) | `Nave`, `INaveBuilder`, `IngenieroJefe`, `RecetaExploradora`, `RecetaCombate`, `RecetaCarga` |
| 2 | **Prototype** | Laboratorio: se clona un tripulante base para armar el escuadrón | Panel original vs. clon: se editan habilidades del clon y el original no cambia | `IPrototipo.Clonar()`, deep clone con `List<string> Habilidades` (sin `MemberwiseClone` como única estrategia) |
| 3 | **Bridge** | Comunicaciones: pedir refuerzos/suministros a la Tierra | Combos tipo × cifrado en vivo; muestra el mensaje y el texto cifrado; peor cifrado = más chance de que el Enjambre lo intercepte | `Mensaje`, `ICifrado`, `MensajeTexto/Alerta/Emergencia`, `SinCifrado`, `CifradoAES`, `CifradoCuantico` |
| 4 | **Facade** | Control Central: arranca cada misión, reacciona a ataques, cierra la partida | Tres botones y una consola que lista la secuencia coordinada de los 6 subsistemas | `ControlCentral` con `IniciarTurno()`, `EntrarEnEmergencia()`, `ApagarEstacion()` + 6 subsistemas |
| 5 | **Flyweight** | Censo: 10.000 tripulantes de 5 especies, de ahí se reclutan | Contador "instancias de especie: 5", comparación de memoria con/sin Flyweight | `IEspecie`, `Tripulante`, `FabricaEspecies` (`Dictionary<string, IEspecie>`) |
| 6 | **Proxy** | Bóveda: inteligencia sobre debilidades del Enjambre y códigos | Según el rol del personaje activo entra o es rechazado; tabla de intentos denegados (persistida en SQL) | `IBoveda`, `BovedaDatos`, `ProxyBoveda` |
| 7 | **Chain of Responsibility** | Combate: todo daño recibido atraviesa las capas de defensa | Animación del recorrido del daño capa por capa; en la Armería se agregan/quitan/reordenan eslabones | `IManejadorDanio.Manejar(Danio)`, `Esquiva`, `Bloqueo`, `Armadura`, `Escudo`, `Vida` |
| 8 | **Command** | Combate: cada acción del jugador es un comando | Botones Deshacer/Rehacer e historial del turno | `IComando`, `MoverComando`, `AtacarComando`, `CurarComando`, `ConstruirComando`, `GestorComandos` |
| 9 | **Visitor** | Almacén: análisis del botín | Selector de visitante y resultado sobre el inventario (valor total, conteo de armas, envenenar armas, estadísticas) | `IVisitor`, `IItem.Aceptar()`, `Arma`, `Pocion`, `Armadura`, `CalculadorPrecio`, `ContadorArmas`, `AplicadorVeneno` |
| 10 | **State** | Combate: estado de cada unidad | Color/ícono por estado y acciones deshabilitadas (Aturdido no actúa, Berserk pega más y recibe más…) | `IEstadoPersonaje` (`Atacar`, `Mover`, `RecibirDanio`), `Normal`, `Envenenado`, `Aturdido`, `Berserk` |
| 11 | **Template Method** | Combate: estructura del turno según la clase de la unidad | Bitácora por etapas: inicio → decisión → acción → fin | `TurnoUnidad.EjecutarTurno()` no virtual, `TurnoSoldado`, `TurnoMedico`, `TurnoFrancotirador`, `TurnoComandante` |
| 12 | **Strategy** | Combate: IA del Enjambre | Acción "Hackear" que cambia la IA de un enemigo en plena partida; panel con la decisión tomada y por qué | `IEstrategiaIA.DecidirAccion(Enemigo, Contexto)`, `IAAgresiva`, `IADefensiva`, `IAAleatoria`, `IACalculadora` |

Enganches entre patrones (para que no sean 12 demos sueltas): el veneno de `AplicadorVeneno` provoca el estado `Envenenado`; la nave del Builder modifica la misión; los clones del Laboratorio son las unidades del combate; la info de la Bóveda revela qué IA usa cada enemigo; `EntrarEnEmergencia()` se dispara cuando un mensaje es interceptado.

## 4. Gamificación y recorrido (consigna 5.4)

- [ ] Pantalla inicial con nombre del juego e integrantes
- [ ] Tutorial de 3+ pantallas (estación, combate, progreso)
- [ ] Ciclo acción → respuesta → progreso
- [ ] XP, niveles, créditos y desbloqueos
- [ ] Victoria y derrota definidas
- [ ] Panel de 12 insignias ("patrón dominado")
- [ ] Persistencia en SQL Server (progreso, logros, registro de la Bóveda)
- [ ] Feedback visual: `Timer`, flashes de color, transiciones, números de daño flotantes
- [ ] Pantalla final con resumen de la partida
- [ ] (extra) Sonido con `System.Media`

## 5. Reparto de responsabilidades

> **Propuesta inicial, a confirmar entre todos** - el reparto de nombres es arbitrario; si alguien prefiere otro patrón, se intercambia acá por PR. Criterio: 12 patrones entre 5 (3-3-2-2-2); quien tiene 2 patrones carga con un tema transversal más pesado, y casi todos tienen al menos un patrón de estación **y** uno de combate para tocar Core, UI, tests y docs.

| Integrante | GitHub | Patrones | Transversal |
|---|---|---|---|
| Ivan Leonel Reser | `BabyPop666` | Facade · Command | Setup del repo y CI, arquitectura base, motor de combate, navegación entre pantallas |
| Maite Stefania Benitez | pendiente | Prototype · Flyweight · State | Identidad visual y sonido |
| María Belén Fiuza | pendiente | Bridge · Proxy | Persistencia SQL Server (`Kepler7.Data`, scripts `db/`) - se engancha con el registro de la Bóveda |
| Carlos Joel Sánchez Quiroz | pendiente | Builder · Strategy · Template Method | README, publicación Release |
| Agustin Villaverde | pendiente | Chain of Resp. · Visitor | Gamificación (XP, niveles, insignias) + pantallas inicial, tutorial y final |

Cada dueño entrega de su patrón: código en Core, tests, pantalla, diagrama UML y párrafo del informe (ver "Definición de terminado" en docs/REGLAS.md). **Cada parte tiene que quedar funcionando dentro del juego** apenas se mergea, y se trabaja en varios commits (ver "Cada parte tiene que funcionar" en docs/REGLAS.md). Informe y PowerPoint se arman entre todos con esos párrafos; el diagrama UML general lo hace Ivan.

## 6. Fases

| Fase | Fechas (2026) | Hito de cierre |
|---|---|---|
| 0 - Setup | lun 21/09 → dom 27/09 | Todos clonaron, compilan y tienen su usuario invitado |
| 1 - Cimientos | lun 28/09 → dom 04/10 | Modelo base mergeado; cada uno puede abrir su rama |
| 2 - Patrones en paralelo | lun 05/10 → dom 25/10 | Los 12 patrones "terminados" (nivel Subteniente/Teniente) |
| 3 - Integración | lun 26/10 → jue 05/11 | Juego completable de inicio a fin con persistencia (nivel Capitán) |
| 4 - Pulido y entregables | vie 06/11 → vie 13/11 | *Code freeze* mié 11/11 · ensayo de defensa jue 12/11 |
| **Entrega** | **sáb 14/11** | |

**Fase 0 - Setup** *(una persona, directo a `main`)*
- [x] Repo privado en GitHub
- [ ] Invitar a los 4 integrantes restantes: enviadas a `kainheinlein` y `NetworksProgram`; faltan Maite y Belén
- [ ] Cada integrante autoriza su cuenta en la PC del grupo ([docs/SUMARSE_AL_REPO.md](docs/SUMARSE_AL_REPO.md))
- [x] `Kepler7.sln` con Core / Data / Game / Tests, `.gitignore`, `README.md` inicial
- [x] GitHub Actions: `dotnet build` + `dotnet test` en cada PR
- [x] Plantilla de PR y guía corta de Git para el equipo (`docs/GUIA_GIT.md`)
- [ ] Confirmar con la cátedra: .NET 8, `Microsoft.Data.SqlClient` y MSTest

**Fase 1 - Cimientos de Core** *(bloquea al resto; corta)*
- [ ] Modelo base: `Unidad`, `Enemigo`, `Grilla`, `Danio`, `Contexto`, `IAleatorio`, `EntradaBitacora`
- [ ] Interfaces de persistencia y modelo de progreso (XP, nivel, créditos, insignias)
- [ ] Esqueleto de Game: formulario principal con navegación entre pantallas vacías

**Fase 2 - Patrones en paralelo** *(cada uno en su rama)*
- [ ] Builder  [ ] Prototype  [ ] Bridge  [ ] Facade  [ ] Flyweight  [ ] Proxy
- [ ] Chain of Resp.  [ ] Command  [ ] Visitor  [ ] State  [ ] Template Method  [ ] Strategy

**Fase 3 - Integración**
- [ ] Motor de combate completo uniendo Command + State + Strategy + Template + Chain
- [ ] Campaña: 6 sectores + Núcleo, con victoria/derrota
- [ ] Persistencia SQL funcionando de punta a punta
- [ ] Gamificación e insignias conectadas

**Fase 4 - Pulido y entregables**
- [ ] Animaciones, identidad visual, sonido
- [ ] Ejecutable Release publicado como GitHub Release
- [ ] `README.md` con instalación, requisitos, configuración de SQL y capturas
- [ ] 13 diagramas UML (1 general + 12)
- [ ] Informe técnico (≤10 págs., responde las 8 preguntas de la sección 9)
- [ ] PowerPoint demostrativo
- [ ] Ensayo de la defensa: cada uno explica sus patrones

## 7. Riesgos

| Riesgo | Mitigación |
|---|---|
| La cátedra no acepta `Microsoft.Data.SqlClient` / MSTest | Preguntar **ya**. Plan B: persistencia JSON detrás de las mismas interfaces; runner de tests propio |
| La PC de evaluación no tiene SQL Server | Usar LocalDB, documentar en README, y que el juego avise claro si no conecta |
| Patrones "de adorno" | Regla de terminado: mecánica real + visible + insignia |
| Lógica filtrándose a los Forms | Core sin referencia a WinForms (no compila si alguien lo intenta) + revisión en PR |
| Historial de Git desparejo | Dueños por patrón desde el día 1, PRs chicos, cada uno commitea lo suyo desde su cuenta |
