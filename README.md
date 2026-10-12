# API de reservas de salas

Um coworking com 12 salas controlava as reservas numa planilha, e mais de uma vez duas equipes apareceram na mesma sala no mesmo horário. Esta API resolve isso: a recepção (e, no futuro, um app) cadastra salas e reservas, e a API não deixa marcar duas reuniões por cima uma da outra.

O que dá para fazer:

- cadastrar, listar, consultar e desativar salas;
- criar, consultar, listar e cancelar reservas;
- filtrar as reservas por sala e por período, com paginação.

## O que você precisa ter instalado

- [SDK do .NET 10](https://dotnet.microsoft.com/download)
- A ferramenta do Entity Framework, para criar o banco:

```bash
dotnet tool install --global dotnet-ef
```

O banco é SQLite, então não precisa instalar nem configurar servidor nenhum. Ele vira um arquivo `app.db` dentro da pasta do projeto.

## Como rodar

Na pasta deste README, crie o banco aplicando as migrations:

```bash
dotnet ef database update
```

Suba a API:

```bash
dotnet run
```

Ela fica disponível em `http://localhost:5192`.

Rode os comandos sempre desta pasta, onde fica o `Reservas.csproj`. O caminho do banco é relativo, então rodar de outro lugar cria um `app.db` novo e vazio em outra pasta.

## Endpoints

### Salas

| Método | Rota | O que faz | Respostas |
|---|---|---|---|
| POST | `/salas` | Cadastra uma sala | 201, 400 |
| GET | `/salas` | Lista todas as salas | 200 |
| GET | `/salas/{id}` | Busca uma sala | 200, 404 |
| PATCH | `/salas/{id}/desativar` | Desativa uma sala | 204, 404 |

### Reservas

| Método | Rota | O que faz | Respostas |
|---|---|---|---|
| POST | `/reservas` | Cria uma reserva | 201, 400, 404, 409 |
| GET | `/reservas` | Lista com filtros e paginação | 200, 400 |
| GET | `/reservas/{id}` | Busca uma reserva | 200, 404 |
| PATCH | `/reservas/{id}/cancelar` | Cancela uma reserva | 204, 404 |

Parâmetros opcionais da listagem de reservas:

| Parâmetro | Para que serve | Padrão |
|---|---|---|
| `salaId` | Só as reservas dessa sala | todas |
| `de` | Reservas que começam a partir dessa data | sem limite |
| `ate` | Reservas que começam até essa data | sem limite |
| `pagina` | Número da página, a partir de 1 | 1 |
| `tamanho` | Itens por página, de 1 a 50 | 10 |

Todos os erros voltam no formato padrão de Problem Details, com `title`, `status` e `detail`. Os erros de validação trazem também o campo `errors`, com a lista dos campos que deram problema.

## Exemplos

O arquivo [`Reservas.http`](Reservas.http) tem uma requisição pronta para cada endpoint, incluindo os casos de erro. Dá para rodar direto pelo VS Code (extensão REST Client) ou pelo Visual Studio.

Alguns exemplos com a resposta que você deve ver:

**Criar uma reserva**

```http
POST /reservas
Content-Type: application/json

{
  "salaId": 1,
  "responsavel": "Maria Silva",
  "inicio": "2027-03-01T10:00:00",
  "fim": "2027-03-01T11:00:00",
  "numeroParticipantes": 5
}
```

Resposta `201 Created`, com o cabeçalho `Location: /reservas/1`:

```json
{
  "reservaId": 1,
  "salaId": 1,
  "responsavel": "Maria Silva",
  "inicio": "2027-03-01T10:00:00",
  "fim": "2027-03-01T11:00:00",
  "numeroParticipantes": 5
}
```

**Tentar marcar por cima de outra reserva**

Mesma sala, das 10h30 às 11h30, com a reserva acima já marcada. Resposta `409 Conflict`:

```json
{
  "title": "Conflito de horário",
  "status": 409,
  "detail": "Já existe uma reserva do intervalo de tempo."
}
```

**Mandar mais de um campo errado**

A API devolve todos os problemas de uma vez, e não só o primeiro. Resposta `400 Bad Request`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "responsavel": ["O responsável não pode estar vazio."],
    "numeroParticipantes": ["O número de participantes não pode ser maior que a capacidade da sala."],
    "inicio": ["Uma reserva deve ser agendada para o futuro."]
  }
}
```

**Listar com paginação**

`GET /reservas?pagina=2&tamanho=10` devolve os itens da página e um resumo para quem estiver navegando:

```json
{
  "itens": [ ... ],
  "metadados": {
    "paginaAtual": 2,
    "tamanhoPagina": 10,
    "totalItens": 17,
    "totalPaginas": 2,
    "temPaginaAnterior": true,
    "temProximaPagina": false
  }
}
```

## Regras e decisões

As regras que a API aplica ao criar uma reserva:

- a reserva dura no mínimo 15 minutos e no máximo 4 horas;
- ela precisa começar no futuro;
- o número de participantes não pode passar da capacidade da sala;
- a sala precisa estar ativa;
- não pode haver outra reserva ativa da mesma sala no mesmo horário.

Algumas escolhas que o enunciado deixou em aberto e como ficaram:

- **Reservas encostadas são permitidas.** Uma reunião que termina às 11h não bloqueia outra que começa às 11h na mesma sala. O horário de fim não faz parte da reserva.
- **Reserva cancelada libera o horário.** Ela não conta mais para o conflito, então dá para remarcar no mesmo intervalo logo em seguida.
- **Canceladas continuam aparecendo na listagem**, com o campo `cancelada` indicando a situação. Assim a recepção vê o histórico e sabe qual reserva vale.
- **Sala desativada recusa reservas com 400**, e a mensagem diz que a sala está desativada. Desativar não apaga a sala nem as reservas que ela já tinha.
- **Os filtros de data olham o início da reserva.** Uma reserva entra no resultado se começar dentro do período pedido.
- **`ate` só com a data inclui o dia inteiro.** `ate=2027-03-01` traz tudo o que começa até o fim do dia 1º. Se você mandar a hora junto (`ate=2027-03-01T10:00:00`), o limite é exatamente aquele horário.
- **Paginação inválida é recusada.** `tamanho` fora de 1 a 50 ou `pagina` menor que 1 voltam 400, em vez de serem ajustados em silêncio.
- **A listagem sai ordenada pelo início** e, quando duas reservas começam juntas, pelo fim.

## Como o projeto está organizado

O código está dividido por caso de uso, e não por tipo técnico. Tudo o que "criar uma reserva" precisa (requisição, resposta, validação e lógica) fica numa pasta só.

```
Reservas/
  Program.cs        registra os serviços e as rotas
  Data/             DbContext e migrations
  Domain/           Sala e Reserva, com as regras de negócio
  Shared/           paginação e respostas de erro reaproveitadas
  Features/
    Salas/          CadastrarSala, ListarSalas, ObterSala, DesativarSala
    Reservas/       CriarReserva, ListarReservas, ObterReserva, CancelarReserva
```

Uma feature nunca usa código de outra. O que é compartilhado mora em `Domain`, `Data` ou `Shared`.

## Análise de complexidade

### A consulta

Antes de gravar uma reserva, a API pergunta ao banco se existe alguma reserva ativa da mesma sala que bate com o horário pedido. Em `CriarReservaEndpoint`, isso vira (já traduzido para SQL):

```sql
SELECT EXISTS (
  SELECT 1 FROM Reservas r
  WHERE r.SalaId = @sala
    AND NOT r.Cancelada
    AND r.Inicio >= @inicio - 4h   -- limite de duração máxima
    AND r.Inicio <  @fim
    AND r.Fim    >  @inicio
)
```

Duas reservas se sobrepõem quando cada uma começa antes de a outra terminar: `existente.Inicio < novo.Fim` e `existente.Fim > novo.Inicio`. Os dois `<`/`>` estritos são o que deixa as reservas encostadas passarem (uma que termina às 11h não conflita com outra que começa às 11h). O `EXISTS` para na primeira linha encontrada, então a consulta nunca traz dados, só responde sim ou não.

### O índice

```
IX_Reserva_IntervaloPorSala (SalaId, Inicio, Fim)
```

A ordem segue o que o banco consegue aproveitar de um índice composto: ele usa as colunas da esquerda para a direita, e só consegue "pular direto" para um trecho enquanto as condições forem de igualdade ou de uma única faixa.

- `SalaId` vem primeiro porque é uma igualdade. Ela leva direto ao pedaço do índice que só tem reservas daquela sala, e as outras 11 salas nem são tocadas.
- `Inicio` vem em segundo porque é a coluna da faixa (`>=` e `<`). Dentro da sala, as reservas já estão ordenadas por início, então a faixa é um trecho contínuo.
- `Fim` vem por último. Depois de uma faixa em `Inicio`, o banco não consegue mais usar `Fim` para navegar, mas como a coluna está no índice ele confere `Fim > @inicio` em cada entrada da faixa sem precisar ler a linha da tabela.

`Cancelada` não está no índice, então só as entradas que passam no teste de `Fim` vão buscar a linha na tabela para checar se ela foi cancelada.

### O papel do limite de 4 horas

O índice só consegue limitar a busca por `Inicio`, e `Fim > @inicio` é só um filtro. Sem outro limite inferior em `Inicio`, a faixa seria `Inicio < @fim`: tudo o que a sala já teve desde o primeiro dia até o horário pedido. O banco percorreria todo esse histórico, entrada por entrada, para descobrir se alguma delas termina depois de `@inicio`.

A regra de negócio resolve isso. Como nenhuma reserva dura mais de 4 horas, uma reserva que começou antes de `@inicio - 4h` já terminou antes de `@inicio` e não tem como conflitar. Por isso a consulta acrescenta `Inicio >= @inicio - 4h`, e a faixa passa a cobrir no máximo uma janela de 8 horas (4 antes do início pedido, mais a duração pedida, que também é de até 4 horas), qualquer que seja o tamanho do histórico.

Esse limite não muda o resultado da consulta, só diz ao banco onde não precisa olhar. O valor vem de `Reserva.TempoMaximo`, a mesma constante que valida a duração, então os dois não se desencontram.

### Custo

- **n** = quantidade de reservas guardadas (todas as salas, ativas e canceladas).
- **k** = quantidade de reservas da sala dentro da janela `[@inicio - 4h, @fim)`.

O custo da consulta é **O(log n + k)**: `log n` para descer na árvore do índice até o começo da janela, e `k` para percorrer as entradas dela.

Na prática `k` é minúsculo e não cresce com a idade do sistema. Reservas ativas da mesma sala não se sobrepõem e duram pelo menos 15 minutos, então cabem poucas dezenas numa janela de 8 horas. As canceladas também aparecem na faixa (o índice não as exclui), mas só dentro dessa mesma janela. Com `k` limitado, o custo real é praticamente **O(log n)**.

Sem o limite de 4 horas o custo seria **O(log n + m)**, onde `m` é o número de reservas da sala até aquele horário, e `m` só aumenta com o tempo. Cada reserva nova deixaria a próxima verificação mais lenta.

### Evidência e como reproduzir

Plano de execução do SQLite (`EXPLAIN QUERY PLAN`) para as duas versões da consulta, numa sala com 100 mil reservas:

```
Com o limite de 4 horas:
SEARCH r USING INDEX IX_Reserva_IntervaloPorSala (SalaId=? AND Inicio>? AND Inicio<?)

Sem o limite:
SEARCH r USING INDEX IX_Reserva_IntervaloPorSala (SalaId=? AND Inicio<?)
```

As duas usam o índice, mas a primeira tem faixa fechada nos dois lados e a segunda só tem o teto. É essa diferença que aparece no tempo. Medição com 100 mil reservas na mesma sala (uma a cada 2 horas, 10% canceladas), consultando um horário depois de todas as reservas existentes, que é o pior caso para a versão sem limite:

| Consulta | Mediana |
|---|---|
| Com o limite de 4 horas | 0,004 ms |
| Sem o limite | 8 ms |

Cerca de 1.800 vezes mais rápido. Os valores absolutos mudam de máquina para máquina; o que importa é a proporção, e que a primeira não depende do tamanho do histórico.

Para repetir, salve o script abaixo como `benchmark.py` fora da pasta do projeto e rode com `python3 benchmark.py`. Ele só precisa do Python, cria o próprio banco `bench.db` com a mesma tabela e o mesmo índice das migrations, e não mexe no `app.db`.

```python
import sqlite3, time, datetime as dt, statistics, os

if os.path.exists("bench.db"):
    os.remove("bench.db")
c = sqlite3.connect("bench.db")
c.executescript("""
CREATE TABLE Reservas (
  ReservaId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, SalaId INTEGER NOT NULL,
  Responsavel TEXT NOT NULL, Inicio TEXT NOT NULL, Fim TEXT NOT NULL,
  NumeroParticipantes INTEGER NOT NULL, Cancelada INTEGER NOT NULL DEFAULT 0);
CREATE INDEX IX_Reserva_IntervaloPorSala ON Reservas (SalaId, Inicio, Fim);""")

f = lambda d: d.strftime("%Y-%m-%d %H:%M:%S")
h = lambda n: dt.timedelta(hours=n)
base, N = dt.datetime(2027, 1, 1, 8), 100_000
c.executemany(
    "INSERT INTO Reservas(SalaId,Responsavel,Inicio,Fim,NumeroParticipantes,Cancelada) VALUES (?,?,?,?,?,?)",
    [(1, "x", f(base + h(2*i)), f(base + h(2*i + 1)), 3, int(i % 10 == 0)) for i in range(N)])
c.commit()
c.execute("ANALYZE")

COM = "SELECT EXISTS (SELECT 1 FROM Reservas WHERE SalaId=? AND NOT Cancelada AND Inicio>=? AND Inicio<? AND Fim>?)"
SEM = "SELECT EXISTS (SELECT 1 FROM Reservas WHERE SalaId=? AND NOT Cancelada AND Inicio<? AND Fim>?)"
ini = base + h(2*N + 1)   # depois de todas as reservas
fim = ini + h(1)
a_com = (1, f(ini - h(4)), f(fim), f(ini))
a_sem = (1, f(fim), f(ini))

for nome, q, a in (("com limite", COM, a_com), ("sem limite", SEM, a_sem)):
    print(nome, [r[3] for r in c.execute("EXPLAIN QUERY PLAN " + q, a)][-1])
    tempos = []
    for _ in range(200):
        t0 = time.perf_counter()
        c.execute(q, a).fetchone()
        tempos.append((time.perf_counter() - t0) * 1000)
    print(f"  mediana: {statistics.median(tempos):.4f} ms")
```

Para conferir o plano no banco real da API (`app.db`), com o SQLite instalado: `sqlite3 app.db "EXPLAIN QUERY PLAN SELECT 1 FROM Reservas WHERE SalaId=1 AND Inicio>='2027-03-01 06:00:00' AND Inicio<'2027-03-01 11:00:00' AND Fim>'2027-03-01 10:00:00'"`.

### Cuidado se a regra mudar

A consulta só está correta enquanto a janela de busca for pelo menos tão grande quanto a duração máxima de uma reserva. O `4h` da consulta vem de `Reserva.TempoMaximo`, então:

- **Se a duração máxima diminuir** (por exemplo, para 2 horas), a janela encolhe junto e a consulta fica ainda mais rápida, mas as reservas que já existem no banco com mais de 2 horas continuam valendo. Uma reserva de 4 horas que começou 3 horas antes do novo pedido ficaria fora da janela de 2 horas, e o conflito passaria sem aviso. Antes de reduzir, é preciso decidir o que fazer com as reservas longas já feitas, ou manter o valor antigo na consulta enquanto elas existirem.
- **Se a duração máxima aumentar**, a janela cresce junto e nada quebra, só ficam um pouco mais entradas para percorrer.
- **Se alguém trocar o valor da consulta sem mexer na regra** (ou o contrário), ela passa a deixar conflitos reais passarem. Por isso as duas coisas usam a mesma constante, e qualquer mudança deve ser feita nela.
