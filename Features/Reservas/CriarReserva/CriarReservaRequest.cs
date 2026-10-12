namespace Reservas.Features.Reservas.CriarReserva;

public record CriarReservaRequest(int SalaId, string Responsavel, DateTime Inicio, DateTime Fim, int NumeroParticipantes);
