namespace Reservas.Features.Reservas.ListarReservas;

public record ListarReservasResponse(int ReservaId, SalaResponse Sala, string Responsavel, DateTime Inicio, DateTime Fim, int NumeroParticipantes, bool Cancelada);
