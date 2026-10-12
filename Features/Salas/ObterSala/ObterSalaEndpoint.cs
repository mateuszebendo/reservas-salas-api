using Microsoft.EntityFrameworkCore;

using Reservas.Data;
using Reservas.Domain;
using Reservas.Shared;

namespace Reservas.Features.Salas.ObterSala;

public static class ObterSalaEndpoint
{
    public static void Map(RouteGroupBuilder grupo) => grupo.MapGet("/{id}", HandleAsync);

    public static async Task<IResult> HandleAsync(
        int id,
        ReservasDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        Sala? sala = await dbContext.Salas.FirstOrDefaultAsync(s => s.SalaId == id, cancellationToken);
        if (sala == null)
        {
            return SalaReponseHelpers.SalaNaoEncontrada(id);
        }

        ObterSalaResponse response = ObterSalaResponse.Mapear(sala);
        return TypedResults.Ok(response);
    }
}
