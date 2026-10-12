using Microsoft.EntityFrameworkCore;

using Reservas.Data;
using Reservas.Domain;
using Reservas.Shared;

namespace Reservas.Features.Reservas.CriarReserva;

public static class CriarReservaEndpoint
{
    public static void Map(RouteGroupBuilder grupo) => grupo.MapPost("/", HandleAsync);

    public static async Task<IResult> HandleAsync(
        CriarReservaRequest request,
        ReservasDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        Sala? sala = await dbContext.Salas
            .FirstOrDefaultAsync(s => s.SalaId == request.SalaId, cancellationToken);

        if (sala == null)
        {
            return SalaReponseHelpers.SalaNaoEncontrada(request.SalaId);
        }

        if (sala.Desativada)
        {
            return SalaReponseHelpers.SalaDesativada(request.SalaId);
        }

        Dictionary<string, string[]> erros = Reserva.ValidarParametros(
            sala,
            request.Responsavel,
            request.Inicio,
            request.Fim,
            request.NumeroParticipantes
        );
        if (erros.Count > 0)
        {
            return TypedResults.ValidationProblem(erros);
        }

        DateTime quatroHorasAntes = request.Inicio - Reserva.TempoMaximo;
        bool existeConflito = await dbContext.Reservas
            .AsNoTracking()
            .Where(r =>
                r.SalaId == request.SalaId &&
                !r.Cancelada &&
                r.Inicio >= quatroHorasAntes &&
                r.Inicio < request.Fim &&
                r.Fim > request.Inicio
            )
            .AnyAsync(cancellationToken);

        if (existeConflito)
        {
            return TypedResults.Problem(
                statusCode: 409,
                title: "Conflito de horário",
                detail: "Já existe uma reserva do intervalo de tempo."
            );
        }

        Reserva novaReserva = new(
            sala,
            request.Responsavel,
            request.Inicio,
            request.Fim,
            request.NumeroParticipantes
        );
        dbContext.Reservas.Add(novaReserva);
        await dbContext.SaveChangesAsync(cancellationToken);

        CriarReservaResponse response = new(
            novaReserva.ReservaId,
            novaReserva.Sala.SalaId,
            novaReserva.Responsavel,
            novaReserva.Inicio,
            novaReserva.Fim,
            novaReserva.NumeroParticipantes
        );
        return TypedResults.Created($"/reservas/{novaReserva.ReservaId}", response);
    }
}
