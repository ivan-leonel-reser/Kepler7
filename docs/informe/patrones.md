# Justificación de los patrones

Cada integrante escribe acá el apartado de sus patrones. Este archivo después se pasa al informe técnico.

Formato de cada apartado: qué problema del juego resuelve, cómo está aplicado y dónde se ve durante la partida. Diagrama en `docs/uml/`.

## Patrones creacionales

### Builder - Naves del Hangar

**Problema.** Una nave de apoyo se arma con muchos componentes y varios son opcionales (escudo, armas, radar, IA de a bordo, tanques, bodega). Con un constructor que reciba todo, cada tipo de nave obligaba a pasar una lista larga de parámetros, la mitad vacíos, y el orden de armado quedaba repartido por cualquier lado.

**Solución.** `INaveBuilder` define los pasos (`ConstruirCasco()`, `InstalarMotor()`, `InstalarEscudo()`, `InstalarArmas()`, `InstalarRadar()`, `InstalarIA()`, `CargarTanques()`, `MontarBodega()`) y `ObtenerNave()`. Cada receta es un builder concreto que decide qué pieza pone en cada paso: `RecetaExploradora` lleva radar y va desarmada, `RecetaCombate` lleva cañones y blindaje, `RecetaCarga` lleva bodega y no tiene ni armas ni IA. Si un componente no va, ese paso no hace nada. `IngenieroJefe` es el director: llama siempre los mismos pasos en el mismo orden y no sabe qué piezas elige cada receta. El producto es `Nave`, que guarda sus componentes y la lista de piezas en el orden en que se montaron.

**Dónde se ve.** En el módulo Hangar el jugador elige una receta, ve aparecer las piezas una por una y después la ficha de la nave. La nave queda lista para la próxima oleada y su apoyo sale de lo que lleva montado: con radar revela la IA de cada enemigo, con armas abre la oleada con un disparo sobre la vanguardia enemiga y con bodega se gana 50% más de créditos. En el hangar entra una sola nave y se usa en una oleada. Armar las tres recetas da la insignia "Ingeniero de hangar".

**Pruebas.** `BuilderTests` verifica qué lleva cada receta, que el ingeniero llame los pasos siempre en el mismo orden y que la misma receta arme una nave nueva cada vez; `PartidaOleadasTests` prueba el apoyo de cada nave en la oleada y la insignia.

Diagrama: [`docs/uml/builder.puml`](../uml/builder.puml)

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

**Solución.** Cada acción es un objeto que implementa `IComando` (`Ejecutar()` y `Deshacer()`): `MoverComando`, `AtacarComando`, `CurarComando` y `ConstruirComando`. Cada comando guarda lo necesario para volver atrás (la fila de origen, la vida que quitó, la barricada que levantó). `GestorComandos` es el invocador: los ejecuta y los apila, con una pila para deshacer y otra para rehacer. Al terminar el turno los comandos pasan al historial y ya no se pueden deshacer. `Oleada` valida la acción y el turno de la unidad (`TurnoUnidad`, ver Template Method) crea el comando; la pantalla solo le dice qué eligió el jugador.

**Dónde se ve.** En la pantalla de la oleada. La lista "Este turno" muestra los comandos que todavía se pueden deshacer; los botones Deshacer y Rehacer los recorren; el "Historial de comandos" muestra lo confirmado en turnos anteriores. Al deshacer, la unidad vuelve a quedar libre para actuar. Deshacer y rehacer una jugada da la insignia "Dueño del tiempo".

**Pruebas.** `CommandTests` verifica que cada comando se deshaga bien (incluso revivir a un enemigo que había caído) y el orden de las pilas; `OleadaTests` y `PartidaOleadasTests` prueban el turno completo y la insignia.

Diagrama: [`docs/uml/command.puml`](../uml/command.puml)

### Template Method - Turno de cada unidad

**Problema.** El turno de cualquier unidad del escuadrón tiene la misma forma: se prepara, interpreta la orden, actúa y cierra. Lo que cambia es qué hace cada clase en cada etapa. Si cada clase escribiera su turno completo, el orden quedaría repetido cuatro veces y bastaría un descuido para que una clase actuara antes de decidir o se salteara el registro de una etapa.

**Solución.** `TurnoUnidad` es abstracta y tiene el método plantilla `EjecutarTurno()`, que no es virtual: llama siempre a `Iniciar()`, `Decidir(orden)`, crea y ejecuta el comando, y termina con `Finalizar()`, registrando cada etapa en la bitácora. `Iniciar()` y `Decidir()` son abstractos y `Finalizar()` es virtual con un comportamiento por defecto. Las subclases solo completan esos pasos: `TurnoSoldado` carga al atacar (+2 de daño), `TurnoFrancotirador` apunta si dispara desde atrás (+3), `TurnoMedico` revisa quién es el más herido y cura +5 a un herido grave, y `TurnoComandante` evalúa el campo y suma +1 de daño por cada aliado en su fila. El bonus que se decide va adentro del comando, así que deshacer la jugada también lo deshace. `Oleada` valida la orden del jugador y le pide el turno a `TurnoUnidad.Para(unidad)`.

**Dónde se ve.** En la pantalla de la oleada, cada acción del escuadrón deja en la bitácora sus cuatro etapas con el nombre de la unidad (inicio, decisión, acción y fin), y el texto cambia según la clase. Al elegir una unidad, el panel de acciones muestra la habilidad de su clase. Jugar un turno con las cuatro clases en una misma oleada da la insignia "Cadena de mando".

**Pruebas.** `TemplateMethodTests` verifica que `EjecutarTurno()` no sea virtual, que las etapas queden siempre en el mismo orden y el bonus de cada clase; `OleadaTests` comprueba que una acción real deja las cuatro etapas y que deshacer devuelve todo el daño.

Diagrama: [`docs/uml/template-method.puml`](../uml/template-method.puml)

### Strategy - IA de los enemigos

**Problema.** Los enemigos del Enjambre no tenían que jugar todos igual, y además queríamos que el jugador pudiera cambiarles la forma de pensar en medio de la oleada. Con un `switch` dentro de la oleada, cada IA nueva obligaba a tocar el motor de combate, y cambiar la de un enemigo ya en juego era manejar banderas a mano.

**Solución.** Cada forma de decidir es una clase que implementa `IEstrategiaIA.DecidirAccion(Enemigo, Contexto)` y devuelve una `DecisionIA` (qué hace, a quién y por qué). `IAAgresiva` va al frente y le pega al que más daño hace; `IADefensiva` se repliega cuando le queda menos de la mitad de la vida; `IAAleatoria` sortea entre todo lo que puede hacer usando el `IAleatorio` del contexto; `IACalculadora` remata al que cae de un golpe y, si no, va por el que cae antes. `Enemigo` guarda su estrategia y solo delega en ella. La estrategia no toca el campo: `Oleada` valida la decisión contra las reglas y la ejecuta, así una IA no puede hacer trampa. Cada tipo de enemigo llega con su IA (Zángano agresiva, Escupidor aleatoria, Coloso defensiva, Reina calculadora).

**Dónde se ve.** En la pantalla de la oleada hay un panel "IA del Enjambre" que muestra, después de cada turno enemigo, qué decidió cada uno y el motivo. Al lado está el hackeo: se elige una IA, se toca "Hackear IA" y después al enemigo. Se puede hackear dos veces por oleada y no se deshace. El enemigo hackeado muestra su IA nueva en la tarjeta y desde el turno siguiente juega distinto (por ejemplo, un Zángano herido con IA defensiva se va atrás en vez de atacar). La IA de un enemigo no se ve hasta que se la revela. El primer hackeo da la insignia "Hacker del Enjambre".

**Pruebas.** `StrategyTests` prueba cada IA por separado sobre el mismo campo y que la misma situación se resuelva distinto según la estrategia; `OleadaTests` verifica que hackear cambia lo que hace el enemigo, el límite de hackeos y que una decisión inválida no se ejecuta.

Diagrama: [`docs/uml/strategy.puml`](../uml/strategy.puml)
