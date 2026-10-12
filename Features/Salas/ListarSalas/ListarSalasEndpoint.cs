using Microsoft.EntityFrameworkCore;

using Reservas.Data;
using Reservas.Domain;

namespace Reservas.Features.Salas.ListarSalas;

public static class ListarSalasEndpoint
{
    public static void Map(RouteGroupBuilder grupo) => grupo.MapGet("/", HandleAsync);

    public static async Task<IResult> HandleAsync(
        ReservasDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        // Abordagem menos perfomática comentada apenas para documentação:
        // List<Sala> salas = await dbContext.Salas.ToListAsync(cancellationToken);
        // foreach (Sala s in salas.OrderBy(s => s.SalaId))
        // {
        //     response.Add(ListarSalasResponse.Mapear(s));
        // }

        // AsNoTracking = EF não mantém cópias para detectar alterações
        // Select = Uma projeção (Select) que faz o próprio SQL pedir só as colunas que o DTO usa.
        // Sem precisar iterar pela lista de novo com foreach
        List<ListarSalasResponse> response = await dbContext.Salas
        .AsNoTracking()
        .OrderBy(s => s.SalaId)
        .Select(s => new ListarSalasResponse(s.SalaId, s.Nome, s.Andar, s.Capacidade, s.Desativada))
        .ToListAsync();

        return TypedResults.Ok(response);
    }
}
