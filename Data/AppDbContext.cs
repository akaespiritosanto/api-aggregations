namespace api_aggregations.Data;

using api_aggregations.Models;
using api_aggregations.Utils;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Reserva> Reserva => Set<Reserva>();
    public DbSet<ProdutoReservado> ProdutoReservado => Set<ProdutoReservado>();
    public DbSet<RelatorioValoresEDuracaoReservas> RelatorioValoresEDuracaoReservas => Set<RelatorioValoresEDuracaoReservas>();
    public DbSet<DispBase> DispBase => Set<DispBase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var nullableDateConverter = new ValueConverter<string?, DateTime?>(
            value => DateStringHelper.ParseDateOrNull(value),
            value => value.HasValue ? DateStringHelper.ToDateString(value.Value) : null);

        var requiredDateConverter = new ValueConverter<string, DateTime>(
            value => DateStringHelper.ParseDate(value),
            value => DateStringHelper.ToDateString(value));

        modelBuilder.Entity<Reserva>().HasKey(reserva => reserva.id);
        modelBuilder.Entity<ProdutoReservado>().HasKey(produtoReservado => produtoReservado.id);
        modelBuilder.Entity<DispBase>().HasKey(dispBase => dispBase.id);

        modelBuilder.Entity<RelatorioValoresEDuracaoReservas>()
            .HasKey(relatorio => new
            {
                relatorio.DataInicio,
                relatorio.DataFim,
                relatorio.IdServico,
                relatorio.IdProduto,
                relatorio.IdDispBase,
                relatorio.Lugar
            });

        modelBuilder.Entity<RelatorioValoresEDuracaoReservas>()
            .ToTable("RelatorioValoresEDuracaoReservas");

        modelBuilder.Entity<DispBase>()
            .ToTable("DispBase");

        modelBuilder.Entity<Reserva>()
            .Property(reserva => reserva.data_pedido)
            .HasConversion(nullableDateConverter);

        modelBuilder.Entity<Reserva>()
            .Property(reserva => reserva.data_anulacao)
            .HasConversion(nullableDateConverter);

        modelBuilder.Entity<Reserva>()
            .Property(reserva => reserva.data_actualizacao)
            .HasConversion(requiredDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.DataInicio)
            .HasConversion(nullableDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.DataFim)
            .HasConversion(nullableDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.data_cancelamento)
            .HasConversion(requiredDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.data_actualizacao)
            .HasConversion(requiredDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.data_criacao)
            .HasConversion(requiredDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.checkin_hora)
            .HasConversion(nullableDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.DataCheckIn)
            .HasConversion(nullableDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.DataConfirmacao)
            .HasConversion(nullableDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.data_embarque)
            .HasConversion(requiredDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
            .Property(produtoReservado => produtoReservado.data_desembarque)
            .HasConversion(requiredDateConverter);

        modelBuilder.Entity<RelatorioValoresEDuracaoReservas>()
            .Property(relatorio => relatorio.DataInicio)
            .HasConversion(requiredDateConverter);

        modelBuilder.Entity<RelatorioValoresEDuracaoReservas>()
            .Property(relatorio => relatorio.DataFim)
            .HasConversion(requiredDateConverter);

        modelBuilder.Entity<ProdutoReservado>()
        .HasOne<Reserva>()
        .WithMany()
        .HasForeignKey(produtoReservado => produtoReservado.id_reserva)
        .OnDelete(DeleteBehavior.Cascade);
    }
}

