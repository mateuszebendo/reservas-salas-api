namespace Reservas.Shared;

public record ResultadoPaginado<T>(
    List<T> Itens,
    MetadadosPaginacao Metadados
)
{
    public ResultadoPaginado(List<T> itens, int totalItens, int paginaAtual, int tamanhoPagina)
        : this(
            itens,
            new MetadadosPaginacao(
                PaginaAtual: paginaAtual,
                TamanhoPagina: tamanhoPagina,
                TotalItens: totalItens,
                TotalPaginas: (int)Math.Ceiling(totalItens / (double)tamanhoPagina),
                TemPaginaAnterior: paginaAtual > 1,
                TemProximaPagina: paginaAtual < (int)Math.Ceiling(totalItens / (double)tamanhoPagina)
            )
        )
    {
    }
}

public record MetadadosPaginacao(
    int PaginaAtual,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas,
    bool TemPaginaAnterior,
    bool TemProximaPagina
);
