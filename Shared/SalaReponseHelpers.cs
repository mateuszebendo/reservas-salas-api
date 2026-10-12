namespace Reservas.Shared;

public static class SalaReponseHelpers
{
    public static IResult SalaNaoEncontrada(int id) => TypedResults.Problem(
        statusCode: 404,
        title: "Registro não encontrado",
        detail: $"A sala com o id {id} não foi encontrada."
    );

    public static IResult SalaDesativada(int id) => TypedResults.Problem(
        statusCode: 400,
        title: "Registro desativado",
        detail: $"A sala com o id {id} se encontra desativada."
    );
}
