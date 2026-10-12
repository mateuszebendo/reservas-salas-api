using Microsoft.EntityFrameworkCore;

using Reservas.Data;
using Reservas.Domain;
using Reservas.Shared;

namespace Reservas.Features.Salas.DesativarSala;

public static class DesativarSalaEndpoint
{
    public static void Map(RouteGroupBuilder grupo) => grupo.MapPatch("/{id}/desativar", HandleAsync);

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

        sala.Desativada = true;
        await dbContext.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
