using Microsoft.EntityFrameworkCore;

using Reservas.Data;
using Reservas.Domain;
using Reservas.Shared;

namespace Reservas.Features.Reservas.ListarReservas;

public static class ListarReservasEndpoint
{
    public static void Map(RouteGroupBuilder grupo) => grupo.MapGet("/", HandleAsync);

    public static async Task<IResult> HandleAsync(
        ReservasDbContext dbContext,
        CancellationToken cancellationToken,
        int? salaId,
        DateTime? de,
        DateTime? ate,
        int pagina = 1,
        int tamanho = 10
    )
    {
        IResult? validacao = ValidarPaginacao(pagina, tamanho);
        if (validacao != null) return validacao;

        IQueryable<Reserva> query = dbContext.Reservas.AsNoTracking();
        query = AplicarFiltros(query, salaId, de, ate);
        query = AplicarOrdenacao(query);

        int totalItens = await query.CountAsync(cancellationToken);

        List<ListarReservasResponse> itens = await query
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(reserva => new ListarReservasResponse(
                reserva.ReservaId,
                SalaResponse.Mapear(reserva.Sala),
                reserva.Responsavel,
                reserva.Inicio,
                reserva.Fim,
                reserva.NumeroParticipantes, 
                reserva.Cancelada
            ))
            .ToListAsync(cancellationToken);

        var resultadoPaginado = new ResultadoPaginado<ListarReservasResponse>(
            itens,
            totalItens,
            pagina,
            tamanho
        );

        return TypedResults.Ok(resultadoPaginado);
    }

    private static IResult? ValidarPaginacao(int pagina, int tamanho)
    {
        if (tamanho < 1 || tamanho > 50)
        {
            return TypedResults.Problem(
                statusCode: 400,
                title: "Tamanho inválido",
                detail: "O tamanho da página deve ser estar entre 1 e 50."
            );
        }

        if (pagina < 1)
        {
            return TypedResults.Problem(
                statusCode: 400,
                title: "Número de páginas inválido",
                detail: "O número de páginas deve ser igual ou maior a 1."
            );
        }

        return null;
    }

    private static IQueryable<Reserva> AplicarFiltros(IQueryable<Reserva> query, int? salaId, DateTime? de, DateTime? ate)
    {
        if (salaId != null && salaId > 0) query = query.Where(r => r.SalaId == salaId);
        if (de != null) query = query.Where(r => r.Inicio >= de);
        // Filtra pelo INÍCIO da reserva (reservas que começam no intervalo [de, ate]).
        // Se 'ate' vier apenas com a data (sem hora, ex.: 2027-03-01), o dia inteiro é incluído:
        // o limite passa a ser o início do dia seguinte (exclusivo). Com hora informada, é inclusivo.
        if (ate != null)
        {
            if (ate.Value.TimeOfDay == TimeSpan.Zero)
            {
                DateTime limite = ate.Value.Date.AddDays(1);
                query = query.Where(r => r.Inicio < limite);
            }
            else
            {
                query = query.Where(r => r.Inicio <= ate);
            }
        }

        return query;
    }

    private static IQueryable<Reserva> AplicarOrdenacao(IQueryable<Reserva> query)
        => query.OrderBy(r => r.Inicio).ThenBy(r => r.Fim);
}

