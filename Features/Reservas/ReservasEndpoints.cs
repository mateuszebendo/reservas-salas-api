using Reservas.Features.Reservas.CancelarReserva;
using Reservas.Features.Reservas.CriarReserva;
using Reservas.Features.Reservas.ListarReservas;
using Reservas.Features.Reservas.ObterReserva;

namespace Reservas.Features.Reservas;

public static class ReservasEndpoints
{
    public static IEndpointRouteBuilder MapReservas(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder reservasGroup = endpoints.MapGroup("/reservas");

        CriarReservaEndpoint.Map(reservasGroup);
        ObterReservaEndpoint.Map(reservasGroup);
        ListarReservasEndpoint.Map(reservasGroup);
        CancelarReservaEndpoint.Map(reservasGroup);

        return endpoints;
    }
}
