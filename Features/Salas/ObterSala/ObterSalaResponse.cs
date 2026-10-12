using Reservas.Domain;

namespace Reservas.Features.Salas.ObterSala;

public record ObterSalaResponse(int SalaId, string Nome, string Andar, int Capacidade, bool Desativada)
{
    public static ObterSalaResponse Mapear(Sala sala) => new(
            sala.SalaId,
            sala.Nome,
            sala.Andar,
            sala.Capacidade,
            sala.Desativada
        );
}
