# Justificación de los patrones

Cada integrante escribe acá el apartado de sus patrones. Este archivo después se pasa al informe técnico.

Formato de cada apartado: qué problema del juego resuelve, cómo está aplicado y dónde se ve durante la partida. Diagrama en `docs/uml/`.

## Patrones estructurales

### Facade - Control Central

**Problema.** Para iniciar un turno, declarar una emergencia o apagar la estación hay que tocar seis subsistemas (Energía, Soporte vital, Navegación, Comunicaciones, Seguridad y Defensa), cada uno con operaciones distintas y en un orden que importa: sin reactor no arranca nada, en una emergencia primero hay que cerrar la estación y al apagar el reactor tiene que ser lo último. Si el formulario hiciera todo eso, la pantalla tendría que conocer los seis subsistemas y el orden correcto de cada operación.

**Solución.** `ControlCentral` es la fachada. Expone solo `IniciarTurno()`, `EntrarEnEmergencia()` y `ApagarEstacion()`, y adentro llama a cada subsistema en el orden que corresponde. Los seis subsistemas son `internal` en `Kepler7.Core`, así que desde `Kepler7.Game` directamente no se pueden usar: el formulario está obligado a pasar por la fachada. La fachada también controla lo que no tiene sentido (por ejemplo, declarar una emergencia con la estación apagada) y devuelve la secuencia de pasos como una lista de `EntradaBitacora`, así la interfaz solo la pinta.

**Dónde se ve.** Cada oleada la arranca y la cierra el Control Central: "Defender la oleada" llama a `EntrarEnEmergencia()`, al ganar se llama a `IniciarTurno()` y si cae el escuadrón, a `ApagarEstacion()`. Las secuencias quedan en la bitácora del combate. Sin un turno iniciado no se puede defender. En el módulo Control Central del puente de mando cada botón dispara una sola operación y la consola muestra, paso a paso, cómo la fachada va coordinando a cada subsistema; las tarjetas cambian de color recién cuando la fachada llega a ese subsistema. Al usar las tres operaciones se gana la insignia "Operador central". El estado de la estación también aparece en el encabezado del puente de mando.

**Pruebas.** `FacadeTests` verifica el orden de cada secuencia, que la emergencia no haga nada con la estación apagada y que los subsistemas no sean públicos.

Diagrama: [`docs/uml/facade.puml`](../uml/facade.puml)

## Patrones de comportamiento

### Command - Acciones del escuadrón

**Problema.** En cada turno el jugador mueve, ataca, cura o construye con cada unidad, y queríamos que pudiera arrepentirse antes de terminar el turno. Si cada botón cambiara el estado directamente, para deshacer habría que saber qué hizo cada uno y cómo revertirlo, y todo eso terminaría mezclado en la pantalla.

**Solución.** Cada acción es un objeto que implementa `IComando` (`Ejecutar()` y `Deshacer()`): `MoverComando`, `AtacarComando`, `CurarComando` y `ConstruirComando`. Cada comando guarda lo necesario para volver atrás (la fila de origen, la vida que quitó, la barricada que levantó). `GestorComandos` es el invocador: los ejecuta y los apila, con una pila para deshacer y otra para rehacer. Al terminar el turno los comandos pasan al historial y ya no se pueden deshacer. `Oleada` valida la acción y crea el comando; la pantalla solo le dice qué eligió el jugador.

**Dónde se ve.** En la pantalla de la oleada. La lista "Este turno" muestra los comandos que todavía se pueden deshacer; los botones Deshacer y Rehacer los recorren; el "Historial de comandos" muestra lo confirmado en turnos anteriores. Al deshacer, la unidad vuelve a quedar libre para actuar. Deshacer y rehacer una jugada da la insignia "Dueño del tiempo".

**Pruebas.** `CommandTests` verifica que cada comando se deshaga bien (incluso revivir a un enemigo que había caído) y el orden de las pilas; `OleadaTests` y `PartidaOleadasTests` prueban el turno completo y la insignia.

Diagrama: [`docs/uml/command.puml`](../uml/command.puml)
