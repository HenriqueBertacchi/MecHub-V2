using Microsoft.EntityFrameworkCore;
using MecHub.Models;

namespace MecHub.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Cliente> cliente { get; set; } = null!;
        public DbSet<Usuario> usuario { get; set; } = null!;
        public DbSet<Mecanico> mecanico { get; set; } = null!;
        public DbSet<Veiculo> veiculo { get; set; } = null!;
        public DbSet<Servico> servico { get; set; } = null!;
        public DbSet<OrdemServico> ordem_servico { get; set; } = null!;
        public DbSet<ItemOrdemServico> item_ordem_servico { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Unique
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Cpf)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Veiculo>()
               .HasIndex(v => v.Placa)
               .IsUnique();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.TipoLogin)
                .HasConversion<int>();

            // 🔗 RELACIONAMENTOS

            // Usuario 1:1 Mecanico
            modelBuilder.Entity<Mecanico>()
                .HasOne(m => m.Usuario)
                .WithOne(u => u.Mecanico)
                .HasForeignKey<Mecanico>(m => m.UsuarioId);

            // Cliente 1:N Veiculo
            modelBuilder.Entity<Veiculo>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Veiculos)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrdemServico -> Cliente
            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Cliente)
                .WithMany()
                .HasForeignKey(o => o.ClienteId);

            // OrdemServico -> Mecanico
            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Mecanico)
                .WithMany(m => m.OrdensServico)
                .HasForeignKey(o => o.MecanicoId);

            // OrdemServico -> Veiculo
            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Veiculo)
                .WithMany(v => v.OrdensServico)
                .HasForeignKey(o => o.VeiculoId);

            // ItemOrdemServico -> OrdemServico
            modelBuilder.Entity<ItemOrdemServico>()
                .HasOne(i => i.OrdemServico)
                .WithMany(o => o.Itens)
                .HasForeignKey(i => i.OrdemServicoId);

            // ItemOrdemServico -> Servico
            modelBuilder.Entity<ItemOrdemServico>()
                .HasOne(i => i.Servico)
                .WithMany(s => s.Itens)
                .HasForeignKey(i => i.ServicoId);

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Cliente -> Mecanico
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Mecanico)
                .WithMany()
                .HasForeignKey(c => c.MecanicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Veiculo -> Mecanico
            modelBuilder.Entity<Veiculo>()
                .HasOne(v => v.Mecanico)
                .WithMany()
                .HasForeignKey(v => v.MecanicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Servico -> Mecanico
            modelBuilder.Entity<Servico>()
                .HasOne(s => s.Mecanico)
                .WithMany()
                .HasForeignKey(s => s.MecanicoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
