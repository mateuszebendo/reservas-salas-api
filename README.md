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

<!--
Responda com as suas palavras, nesta ordem:

1. Qual é a consulta que verifica conflito de horário?
2. Qual índice ela usa, e por que as colunas estão nessa ordem?
3. Onde entra o limite de 4 horas? Sem ele, o que a consulta acabaria percorrendo?
4. Qual é o custo da consulta? O que são n e k neste sistema?
5. Qual é a evidência? Mostre o resultado do EXPLAIN QUERY PLAN e o tempo medido
   com 100 mil reservas numa sala, e explique como alguém pode repetir o teste.
6. O que acontece se a duração máxima diminuir no futuro?
-->

### A consulta

### O índice

### O papel do limite de 4 horas

### Custo

### Evidência e como reproduzir

### Cuidado se a regra mudar
