using Reservas.Features.Salas.CadastrarSala;
using Reservas.Features.Salas.DesativarSala;
using Reservas.Features.Salas.ListarSalas;
using Reservas.Features.Salas.ObterSala;

namespace Reservas.Features.Salas;

public static class SalasEndpoints
{
    public static IEndpointRouteBuilder MapSalas(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder salasGroup = endpoints.MapGroup("/salas");

        CadastrarSalaEndpoint.Map(salasGroup);
        ListarSalasEndpoint.Map(salasGroup);
        ObterSalaEndpoint.Map(salasGroup);
        DesativarSalaEndpoint.Map(salasGroup);

        return endpoints;
    }
}
