using Reservas.Data;
using Reservas.Domain;

namespace Reservas.Features.Salas.CadastrarSala;

public static class CadastrarSalaEndpoint
{
    public static void Map(RouteGroupBuilder grupo) => grupo.MapPost("/", HandleAsync);

    public static async Task<IResult> HandleAsync(
        CadastrarSalaRequest request,
        ReservasDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, string[]> erros = Sala.ValidarParametros(request.Nome, request.Capacidade, request.Andar);
        if (erros.Count > 0)
        {
            return TypedResults.ValidationProblem(erros);
        }

        Sala sala = new(request.Nome, request.Capacidade, request.Andar);
        dbContext.Salas.Add(sala);
        await dbContext.SaveChangesAsync(cancellationToken);

        CadastrarSalaResponse response = new(
            sala.SalaId,
            sala.Nome,
            sala.Andar,
            sala.Capacidade
        );
        return TypedResults.Created($"/salas/{response.SalaId}", response);
    }
}
