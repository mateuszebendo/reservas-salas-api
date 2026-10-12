namespace Reservas.Domain.Exceptions;

public class ValidacaoInternaException : Exception
{
    public IReadOnlyDictionary<string, string[]> Erros { get; }

    public ValidacaoInternaException(IReadOnlyDictionary<string, string[]> erros)
        : base("Uma ou mais regras de negócio foram violadas.")
    {
        Erros = erros;
    }
}
