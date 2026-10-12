using Reservas.Domain.Exceptions;

namespace Reservas.Domain;

public class Sala
{
    public int SalaId { get; set; }
    public string Nome { get; set; }
    public int Capacidade { get; set; }
    public string Andar { get; set; }
    public bool Desativada { get; set; } = false;

#pragma warning disable CS8618
    protected Sala() { }
#pragma warning restore CS8618
    public Sala(string nome, int capacidade, string andar)
    {
        Dictionary<string, string[]> erros = ValidarParametros(nome, capacidade, andar);
        if (erros.Count > 0)
        {
            throw new ValidacaoInternaException(erros);
        }

        Nome = nome;
        Capacidade = capacidade;
        Andar = andar;
    }

    public static Dictionary<string, string[]> ValidarParametros(string nome, int capacidade, string andar)
    {
        Dictionary<string, string[]> erros = [];
        if (string.IsNullOrWhiteSpace(nome))
        {
            erros.Add(nameof(nome), ["O nome da sala não pode estar vazio."]);
        }

        if (string.IsNullOrWhiteSpace(andar))
        {
            erros.Add(nameof(andar), ["O andar da sala não pode estar vazio."]);
        }

        if (capacidade <= 0)
        {
            erros.Add(nameof(capacidade), ["A capacidade não pode ser igual ou menor a 0."]);
        }

        return erros;
    }
}
