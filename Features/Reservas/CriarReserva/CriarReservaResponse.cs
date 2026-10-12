namespace Reservas.Features.Reservas.CriarReserva;

public record CriarReservaResponse(int ReservaId, int SalaId, string Responsavel, DateTime Inicio, DateTime Fim, int NumeroParticipantes);
