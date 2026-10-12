using Reservas.Domain;

namespace Reservas.Features.Salas.ListarSalas;

public record ListarSalasResponse(int SalaId, string Nome, string Andar, int Capacidade, bool Desativada);
