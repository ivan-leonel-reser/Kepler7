# Kepler-7 - Plan de trabajo

> Borrador v0. Las reglas de código y de Git están en [docs/REGLAS.md](docs/REGLAS.md). La consigna es `docs/TP Final.pdf`.

## 1. Decisiones tomadas

| Tema | Decisión | Estado |
|---|---|---|
| Plataforma | .NET 8 + Windows Forms | a confirmar con la cátedra (la consigna acepta .NET 8 o Framework 4.8 "según lo acordado") |
| Género | Defensa por oleadas con combate táctico por turnos en dos filas | definido |
| Persistencia | SQL Server (LocalDB/Express) + script de creación | definido; falta confirmar que `Microsoft.Data.SqlClient` (NuGet de Microsoft, no de terceros) está permitido |
| Nivel objetivo | **Comandante** (tests, Release, identidad propia) | definido |
| Tests | MSTest en `Kepler7.Tests` | a confirmar; plan B: runner de consola propio sin paquetes |
| UML | PlantUML en `docs/uml/*.puml` (texto versionable, cada uno commitea el suyo) | propuesta |
| Equipo | Grupo 9 · T1-23-32 Trabajo de Diploma · 5 integrantes | definido |
| Entrega | **Sábado 14/11/2026** | definido |

## 2. El juego

**Título:** *Kepler-7: Protocolo Fénix*

**En una frase.** El Enjambre ataca Kepler-7 en 6 oleadas. Antes de cada una preparás a tu escuadrón en la estación y después la defendés en un combate por turnos. Si aguantás las 6, salvás la estación.

**Ciclo de juego** (se repite 6 veces):

```
Inicio y tutorial
   ↓
PREPARAR el escuadrón (estación)  →  DEFENDER la oleada (combate)  →  COBRAR la recompensa
   ↑___________________________________________________________________________|
   ↓ (después de la oleada 6, o si cae el escuadrón)
Pantalla final con el resumen
```

- **Preparar (estación):** el puente de mando tiene un módulo por patrón de preparación (Hangar, Censo, Laboratorio, Comunicaciones, Bóveda, Almacén, Armería) y el Control Central.
- **Defender (oleada):** combate por turnos en dos filas contra el Enjambre. Cada oleada trae enemigos más fuertes.
- **Recompensa:** XP, créditos, botín para el Almacén e insignias. Subir de nivel desbloquea recetas y mejoras.
- **Victoria:** superar la oleada 6. **Derrota:** si cae todo el escuadrón en una oleada, la estación se apaga y termina la partida.

**Combate en dos filas.**

- Cada bando tiene una fila de adelante (vanguardia) y una de atrás (retaguardia), con hasta 3 unidades por fila.
- Las unidades cuerpo a cuerpo solo atacan desde adelante; las de distancia (francotirador, médico) actúan desde cualquier fila.
- Solo se puede atacar a la fila de adelante del rival. Si está vacía, a la de atrás.
- En su turno cada unidad hace una acción: cambiar de fila, atacar, curar o construir una barricada. Hasta confirmar el turno se puede deshacer y rehacer. Después juega el Enjambre.
- La oleada se gana cuando caen todos los enemigos y se pierde cuando cae todo el escuadrón.

## 3. Los 12 patrones → mecánicas

| # | Patrón | Dónde está en el juego | Qué ve y hace el jugador | Tipos obligatorios |
|---|---|---|---|---|
| 1 | **Builder** | Hangar: la nave de apoyo de la próxima oleada | Elige receta, ve el ensamblado paso a paso y la ficha de la nave. La nave da un bonus en la oleada (Exploradora: muestra la IA de los enemigos · Combate: disparo de apoyo · Carga: más créditos) | `Nave`, `INaveBuilder`, `IngenieroJefe`, `RecetaExploradora`, `RecetaCombate`, `RecetaCarga` |
| 2 | **Prototype** | Laboratorio: clonar a un soldado para sumarlo al escuadrón | Original y clon lado a lado: se editan las habilidades del clon y el original no cambia | `IPrototipo.Clonar()`, deep clone con `List<string> Habilidades` (sin `MemberwiseClone` como única estrategia) |
| 3 | **Bridge** | Comunicaciones: pedir refuerzos a la Tierra | Elige tipo de mensaje y cifrado y ve el texto cifrado. Sin cifrado es gratis pero el Enjambre puede interceptarlo y la próxima oleada viene más fuerte | `Mensaje`, `ICifrado`, `MensajeTexto/Alerta/Emergencia`, `SinCifrado`, `CifradoAES`, `CifradoCuantico` |
| 4 | **Facade** | Control Central: arranca y cierra cada oleada | "Defender la oleada" dispara `EntrarEnEmergencia()`, al ganar se llama a `IniciarTurno()` y al perder a `ApagarEstacion()`. En el módulo se ve la secuencia de los 6 subsistemas | `ControlCentral` con `IniciarTurno()`, `EntrarEnEmergencia()`, `ApagarEstacion()` + 6 subsistemas |
| 5 | **Flyweight** | Censo: los 10.000 tripulantes de 5 especies, de donde se recluta | Contador "objetos de especie creados: 5" y comparación de memoria con y sin Flyweight. La especie del recluta le da un bonus | `IEspecie`, `Tripulante`, `FabricaEspecies` (`Dictionary<string, IEspecie>`) |
| 6 | **Proxy** | Bóveda: informe secreto sobre la próxima oleada | Con un rol autorizado se lee el informe (qué IA usa cada enemigo); si no, se rechaza y queda en el registro de intentos | `IBoveda`, `BovedaDatos`, `ProxyBoveda` |
| 7 | **Chain of Responsibility** | Armería y cada golpe en la oleada | En la Armería se agregan, quitan y reordenan las capas. En combate se ve el recorrido del golpe capa por capa | `IManejadorDanio.Manejar(Danio)`, `Esquiva`, `Bloqueo`, `Armadura`, `Escudo`, `Vida` |
| 8 | **Command** | Oleada: cada acción del jugador | Deshacer y rehacer dentro del turno, con historial. Mover es cambiar de fila y Construir levanta una barricada | `IComando`, `MoverComando`, `AtacarComando`, `CurarComando`, `ConstruirComando`, `GestorComandos` |
| 9 | **Visitor** | Almacén: el botín de las oleadas | Elige un analizador: valor total para vender, contar armas o envenenar armas (los ataques pasan a envenenar) | `IVisitor`, `IItem.Aceptar()`, `Arma`, `Pocion`, `Armadura`, `CalculadorPrecio`, `ContadorArmas`, `AplicadorVeneno` |
| 10 | **State** | Oleada: estado de cada unidad | Color e ícono por estado y acciones habilitadas: Aturdido pierde el turno, Envenenado pierde vida cada turno, Berserk pega y recibe más | `IEstadoPersonaje` (`Atacar`, `Mover`, `RecibirDanio`), `Normal`, `Envenenado`, `Aturdido`, `Berserk` |
| 11 | **Template Method** | Oleada: el turno de cada unidad | Bitácora por etapas (inicio → decisión → acción → fin), distinta según la clase | `TurnoUnidad.EjecutarTurno()` no virtual, `TurnoSoldado`, `TurnoMedico`, `TurnoFrancotirador`, `TurnoComandante` |
| 12 | **Strategy** | Oleada: la IA de cada enemigo | "Hackear" cambia la IA de un enemigo; un panel muestra qué decidió y por qué | `IEstrategiaIA.DecidirAccion(Enemigo, Contexto)`, `IAAgresiva`, `IADefensiva`, `IAAleatoria`, `IACalculadora` |

Enganches entre patrones (para que no sean 12 demos sueltas): el Control Central arranca y cierra cada oleada; la nave del Hangar da un bonus en la oleada; los reclutas del Censo se clonan en el Laboratorio y forman el escuadrón; el informe de la Bóveda revela qué IA usa cada enemigo; un pedido de refuerzos interceptado hace más fuerte la oleada; las capas que se ordenan en la Armería son las que atraviesa cada golpe; el veneno del Almacén provoca el estado Envenenado.

## 4. Gamificación y recorrido (consigna 5.4)

- [x] Pantalla inicial con nombre del juego e integrantes
- [ ] Tutorial de 3 pantallas: la historia y las 6 oleadas, la estación, el combate en dos filas
- [x] Ciclo acción → respuesta → progreso
- [ ] XP, niveles, créditos y desbloqueos
- [x] Victoria y derrota definidas
- [ ] Panel de 12 insignias ("patrón dominado")
- [ ] Persistencia en SQL Server (progreso, logros, registro de la Bóveda)
- [ ] Feedback visual: `Timer`, flashes de color, transiciones, números de daño flotantes
- [ ] Pantalla final con resumen de la partida
- [ ] (extra) Sonido con `System.Media`

## 5. Reparto de responsabilidades

> **Propuesta inicial, a confirmar entre todos** - el reparto de nombres es arbitrario; si alguien prefiere otro patrón, se intercambia acá por PR. Criterio: 12 patrones entre 5 (3-3-2-2-2); quien tiene 2 patrones carga con un tema transversal más pesado, y casi todos tienen al menos un patrón de estación **y** uno de combate para tocar Core, UI, tests y docs.

| Integrante | GitHub | Patrones | Transversal |
|---|---|---|---|
| Ivan Leonel Reser | `ivan-leonel-reser` | Facade · Command | Setup del repo y CI, arquitectura base, motor de combate, navegación entre pantallas |
| Maite Stefania Benitez | `MaiteMB` | Prototype · Flyweight · State | Identidad visual y sonido |
| María Belén Fiuza | `belenfiuza1` | Bridge · Proxy | Persistencia SQL Server (`Kepler7.Data`, scripts `db/`) - se engancha con el registro de la Bóveda |
| Carlos Joel Sánchez Quiroz | `kainheinlein` | Builder · Strategy · Template Method | README, publicación Release |
| Agustin Villaverde | `VillaverdeAgustin` | Chain of Resp. · Visitor | Gamificación (panel de logros, recompensas, subida de nivel) + tutorial y pantalla final |

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
- [x] Invitar a los 4 integrantes restantes (Carlos y Agustín ya aceptaron; Maite y Belén tienen la invitación pendiente)
- [ ] Cada integrante autoriza su cuenta en la PC del grupo ([docs/SUMARSE_AL_REPO.md](docs/SUMARSE_AL_REPO.md))
- [x] `Kepler7.sln` con Core / Data / Game / Tests, `.gitignore`, `README.md` inicial
- [x] GitHub Actions: `dotnet build` + `dotnet test` en cada PR
- [x] Plantilla de PR y guía corta de Git para el equipo (`docs/GUIA_GIT.md`)
- [ ] Confirmar con la cátedra: .NET 8, `Microsoft.Data.SqlClient` y MSTest

**Fase 1 - Cimientos de Core** *(bloquea al resto; corta)*
- [x] Modelo base: `Unidad`, `Enemigo`, `CampoBatalla` (dos filas), `Danio`, `Contexto`, `IAleatorio`, `EntradaBitacora`
- [x] Interfaces de persistencia y modelo de progreso (XP, nivel, créditos, insignias)
- [x] Persistencia en memoria en `Kepler7.Data` mientras no esté SQL Server
- [x] Esqueleto de Game: inicio, puente de mando con los 8 módulos (deshabilitados hasta que cada dueño conecte el suyo) y bitácora

**Fase 2 - Patrones en paralelo** *(cada uno en su rama)*
- [ ] Builder  [ ] Prototype  [ ] Bridge  [x] Facade  [ ] Flyweight  [ ] Proxy
- [ ] Chain of Resp.  [x] Command  [ ] Visitor  [ ] State  [x] Template Method  [x] Strategy

**Fase 3 - Integración**
- [x] Motor de combate en dos filas con Command: oleada jugable, turno del Enjambre, victoria y derrota
- [ ] Enganchar State, Strategy, Template Method y Chain al combate (puntos de enganche: `Unidad.RecibirGolpe`, `Oleada.JugarEnemigo`)
- [x] Las 6 oleadas, con victoria y derrota, arrancadas y cerradas por el Control Central
- [ ] Balancear la dificultad de las oleadas con los bonus de los módulos
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
