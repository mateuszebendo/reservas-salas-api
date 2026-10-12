using Reservas.Domain.Exceptions;

namespace Reservas.Domain;

public class Reserva
{
    public int ReservaId { get; set; }
    public int SalaId { get; set; }
    public Sala Sala { get; set; }
    public string Responsavel { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public int NumeroParticipantes { get; set; }
    public bool Cancelada { get; set; } = false;
    public static readonly TimeSpan TempoMinimo = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan TempoMaximo = TimeSpan.FromHours(4);

#pragma warning disable CS8618
    protected Reserva() { }
#pragma warning restore CS8618

    public Reserva(Sala sala, string responsavel, DateTime inicio, DateTime fim, int numeroParticipantes)
    {
        Dictionary<string, string[]> erros = ValidarParametros(sala, responsavel, inicio, fim, numeroParticipantes);
        if (erros.Count > 0) throw new ValidacaoInternaException(erros);

        Sala = sala;
        Responsavel = responsavel;
        Inicio = inicio;
        Fim = fim;
        NumeroParticipantes = numeroParticipantes;
    }

    public static Dictionary<string, string[]> ValidarParametros(Sala sala, string responsavel, DateTime inicio, DateTime fim, int numeroParticipantes)
    {
        Dictionary<string, string[]> erros = [];

        if (sala == null)
        {
            erros.Add(nameof(sala), ["A sala não pode ser nula."]);
        }

        if (string.IsNullOrWhiteSpace(responsavel))
        {
            erros.Add(nameof(responsavel), ["O responsável não pode estar vazio."]);
        }

        if (numeroParticipantes <= 0)
        {
            erros.Add(nameof(numeroParticipantes), ["O número de participantes deve ser maior que zero."]);
        }

        if (sala != null)
        {
            ValidarNumeroDeParticipantes(erros, sala, numeroParticipantes);
        }

        ValidarDuracao(erros, inicio, fim);

        return erros;
    }

    private static void ValidarNumeroDeParticipantes(Dictionary<string, string[]> erros, Sala sala, int numeroParticipantes)
    {
        if (numeroParticipantes > sala.Capacidade)
        {
            erros.Add(nameof(numeroParticipantes), ["O número de participantes não pode ser maior que a capacidade da sala."]);
        }
    }

    private static void ValidarDuracao(Dictionary<string, string[]> erros, DateTime inicio, DateTime fim)
    {
        if (fim < inicio)
        {
            erros.Add(nameof(fim), ["A hora fim deve ser maior que a de início."]);
        }

        DateTime agora = DateTime.Now;
        if (inicio <= agora)
        {
            erros.Add(nameof(inicio), ["Uma reserva deve ser agendada para o futuro."]);
        }

        TimeSpan duracao = fim - inicio;

        if (duracao.TotalMinutes < TempoMinimo.TotalMinutes)
        {
            erros.Add($"{nameof(inicio)}, {nameof(fim)}", [$"A reserva deve ter duração mínima de {TempoMinimo.TotalMinutes} minutos."]);
        }

        if (duracao.TotalHours > TempoMaximo.TotalHours)
        {
            erros.Add($"{nameof(inicio)}, {nameof(fim)}", [$"A reserva deve ter duração máxima de {TempoMaximo.TotalHours} horas."]);
        }
    }
}
