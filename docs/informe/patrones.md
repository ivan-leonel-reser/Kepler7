# Justificación de los patrones

Cada integrante escribe acá el apartado de sus patrones. Este archivo después se pasa al informe técnico.

Formato de cada apartado: qué problema del juego resuelve, cómo está aplicado y dónde se ve durante la partida. Diagrama en `docs/uml/`.

## Patrones estructurales

### Facade - Control Central

**Problema.** Para iniciar un turno, declarar una emergencia o apagar la estación hay que tocar seis subsistemas (Energía, Soporte vital, Navegación, Comunicaciones, Seguridad y Defensa), cada uno con operaciones distintas y en un orden que importa: sin reactor no arranca nada, en una emergencia primero hay que cerrar la estación y al apagar el reactor tiene que ser lo último. Si el formulario hiciera todo eso, la pantalla tendría que conocer los seis subsistemas y el orden correcto de cada operación.

**Solución.** `ControlCentral` es la fachada. Expone solo `IniciarTurno()`, `EntrarEnEmergencia()` y `ApagarEstacion()`, y adentro llama a cada subsistema en el orden que corresponde. Los seis subsistemas son `internal` en `Kepler7.Core`, así que desde `Kepler7.Game` directamente no se pueden usar: el formulario está obligado a pasar por la fachada. La fachada también controla lo que no tiene sentido (por ejemplo, declarar una emergencia con la estación apagada) y devuelve la secuencia de pasos como una lista de `EntradaBitacora`, así la interfaz solo la pinta.

**Dónde se ve.** En el módulo Control Central del puente de mando. Cada botón dispara una sola operación y la consola muestra, paso a paso, cómo la fachada va coordinando a cada subsistema; las tarjetas cambian de color recién cuando la fachada llega a ese subsistema. Al usar las tres operaciones se gana la insignia "Operador central". El estado de la estación también aparece en el encabezado del puente de mando.

**Pruebas.** `FacadeTests` verifica el orden de cada secuencia, que la emergencia no haga nada con la estación apagada y que los subsistemas no sean públicos.

Diagrama: [`docs/uml/facade.puml`](../uml/facade.puml)
