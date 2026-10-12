using Microsoft.EntityFrameworkCore;

using Reservas.Data;
using Reservas.Features.Reservas;
using Reservas.Features.Salas;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("Default")
?? throw new InvalidOperationException("Não foi possível achar a string de conexão default para o banco de dados.");

builder.Services.AddDbContext<ReservasDbContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

app.MapSalas();
app.MapReservas();

app.Run();
