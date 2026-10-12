using Microsoft.EntityFrameworkCore;

using Reservas.Data;
using Reservas.Domain;
using Reservas.Shared;

namespace Reservas.Features.Reservas.ObterReserva;

public static class ObterReservaEndpoint
{
    public static void Map(RouteGroupBuilder grupo) => grupo.MapGet("/{id}", HandleAsync);

    public static async Task<IResult> HandleAsync(
        int id,
        ReservasDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        ObterReservaResponse? response = await dbContext
            .Reservas
            .AsNoTracking()
            .Where(r => r.ReservaId == id)
            .Select(reserva => new ObterReservaResponse(
                reserva.ReservaId,
                SalaResponse.Mapear(reserva.Sala),
                reserva.Responsavel,
                reserva.Inicio,
                reserva.Fim,
                reserva.NumeroParticipantes
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (response == null)
        {
            return ReservaResponseHelper.ReservaNaoEncontrada(id);
        }

        return TypedResults.Ok(response);
    }

}
