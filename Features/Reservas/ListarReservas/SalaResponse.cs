using Reservas.Domain;

namespace Reservas.Features.Reservas.ListarReservas;

public record SalaResponse(int SalaId, string Nome, string Andar, int Capacidade, bool Desativada)
{
    public static SalaResponse Mapear(Sala sala) => new(
            sala.SalaId,
            sala.Nome,
            sala.Andar,
            sala.Capacidade,
            sala.Desativada
        );
}
