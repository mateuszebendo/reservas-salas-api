using Microsoft.EntityFrameworkCore;

using Reservas.Data;
using Reservas.Domain;
using Reservas.Shared;

namespace Reservas.Features.Reservas.CancelarReserva;

public static class CancelarReservaEndpoint
{
    public static void Map(RouteGroupBuilder grupo) => grupo.MapPatch("/{id}/cancelar", HandleAsync);

    public static async Task<IResult> HandleAsync(
        int id,
        ReservasDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        Reserva? reserva = await dbContext.Reservas.FirstOrDefaultAsync(r => r.ReservaId == id, cancellationToken);
        if (reserva == null)
        {
            return ReservaResponseHelper.ReservaNaoEncontrada(id);
        }

        reserva.Cancelada = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }
}
