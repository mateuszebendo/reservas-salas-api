namespace Reservas.Features.Reservas.ObterReserva;

public record ObterReservaResponse(int ReservaId, SalaResponse Sala, string Responsavel, DateTime Inicio, DateTime Fim, int NumeroParticipantes);
