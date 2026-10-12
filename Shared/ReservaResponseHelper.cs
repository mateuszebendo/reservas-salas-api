namespace Reservas.Shared;

public static class ReservaResponseHelper
{
    public static IResult ReservaNaoEncontrada(int reservaId) => TypedResults.Problem(
            statusCode: 404,
            title: "Registro não encontrado",
            detail: $"A reserva com o id {reservaId} não foi encontrada."
    );
}
