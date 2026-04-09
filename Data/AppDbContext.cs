namespace api_aggregations.Data;

using api_aggregations.Models;
using api_aggregations.Services;
using api_aggregations.Controllers;

using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Reserva> Reserva => Set<Reserva>();
    public DbSet<ProdutoReservado> ProdutoReservado => Set<ProdutoReservado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Reserva>().HasKey(reserva => reserva.id);
        modelBuilder.Entity<ProdutoReservado>().HasKey(produtoReservado => produtoReservado.id);

        modelBuilder.Entity<ProdutoReservado>()
        .HasOne<Reserva>()
        .WithMany()
        .HasForeignKey(produtoReservado => produtoReservado.id_reserva)
        .OnDelete(DeleteBehavior.Cascade);
    }
}

