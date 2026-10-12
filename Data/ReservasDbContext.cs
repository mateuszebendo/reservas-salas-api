using Microsoft.EntityFrameworkCore;

using Reservas.Domain;

namespace Reservas.Data;

public class ReservasDbContext : DbContext
{
    public DbSet<Sala> Salas { get; set; }
    public DbSet<Reserva> Reservas { get; set; }

    public ReservasDbContext(DbContextOptions<ReservasDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reserva>()
            .HasIndex(r => new { r.SalaId, r.Inicio, r.Fim })
            .HasDatabaseName("IX_Reserva_IntervaloPorSala");
    }
}
