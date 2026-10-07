using Kepler7.BE;
using Kepler7.Core.Combate;

namespace Kepler7.Core.Juego;

/// <summary>Las tres pantallas que explican el juego antes de la primera partida. Solo texto: la interfaz lo pagina.</summary>
public static class Tutorial
{
    public static IReadOnlyList<PaginaTutorial> Paginas { get; } =
    [
        new("La historia",
        [
            "Kepler-7 es una estación orbital en el borde del sistema. El Enjambre, una especie que devora todo lo que encuentra, " +
            $"viene a buscarla en {ProgresoJugador.OleadasTotales} oleadas, cada una más grande que la anterior.",
            "Vos sos el comandante. Antes de cada oleada preparás al escuadrón en el puente de mando y después la defendés en un combate por turnos.",
            $"Si aguantás las {ProgresoJugador.OleadasTotales} oleadas, la estación se salva. Si en una oleada cae todo el escuadrón, Kepler-7 se apaga y la partida termina.",
            "Cada oleada superada da experiencia y créditos. Subir de nivel desbloquea ventajas, y dominar cada sistema de la estación da una insignia. Son 12.",
        ]),
        new("La estación",
        [
            "El puente de mando tiene ocho módulos. El Control Central inicia el turno: sin eso no se puede defender. También declara la emergencia al empezar cada oleada y apaga la estación si cae el escuadrón.",
            "Hangar: la nave de apoyo de la próxima oleada (revela la IA enemiga, dispara a la vanguardia o trae más créditos). Censo: reclutá a un tripulante entre 10.000 y su especie le da un bonus. Laboratorio: cloná a un soldado y entrenalo sin tocar al original.",
            "Comunicaciones: pedí refuerzos a la Tierra; el cifrado cuesta, pero sin cifrar el Enjambre puede leerlo. Bóveda: el informe de la próxima oleada, solo para roles autorizados. Almacén: vendé el botín o envenená las armas. Armería: ordená las capas que atraviesa cada golpe.",
            "Todo lo que preparás se usa en la oleada siguiente. La nave y el mensaje a la Tierra se gastan; el recluta, el clon, las capas y el veneno se quedan.",
        ]),
        new("El combate en dos filas",
        [
            $"Cada bando tiene una fila de adelante y una de atrás, con hasta {CampoBatalla.LugaresPorFila} unidades por fila. Los cuerpo a cuerpo solo atacan desde adelante; el médico y el francotirador actúan desde cualquier fila. Solo se puede atacar a la fila de adelante del rival; si está vacía, a la de atrás.",
            "En tu turno cada unidad hace una acción: cambiar de fila, atacar, curar o levantar una barricada. Hasta que termines el turno podés deshacer y rehacer. También podés hackear la IA de un enemigo, que no gasta acción.",
            "Los golpes atraviesan las capas de la Armería antes de llegar a la vida. Un golpe que entra puede dejar a la unidad envenenada, aturdida o, si queda muy herida, en berserk: cada estado cambia lo que puede hacer y se ve en su tarjeta.",
            "Cuando terminás el turno juega el Enjambre: cada enemigo decide según su IA y el panel de abajo cuenta qué hizo y por qué. La oleada se gana cuando caen todos los enemigos.",
        ]),
    ];
}
