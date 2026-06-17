using Microsoft.EntityFrameworkCore;
using Vendinha.Core.Models;

namespace Vendinha.Core.Data
{
    public class VendinhaDbContext : DbContext
    {
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Divida> Dividas => Set<Divida>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Default");

            if (string.IsNullOrWhiteSpace(connection))
            {
                connection = "Data Source=vendinha.db";
            }

            optionsBuilder.UseSqlite(connection);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var modelCliente = modelBuilder.Entity<Cliente>();
            modelCliente.ToTable("clientes");
            modelCliente.Property(e => e.Id).HasColumnName("id");
            modelCliente.Property(e => e.NomeCompleto).HasColumnName("nome_completo");
            modelCliente.Property(e => e.Cpf).HasColumnName("cpf");
            modelCliente.Property(e => e.DataNascimento).HasColumnName("data_nascimento");
            modelCliente.Property(e => e.Email).HasColumnName("email");
            modelCliente.HasKey(e => e.Id);
            modelCliente.HasIndex(e => e.Cpf).IsUnique();

            var modelDivida = modelBuilder.Entity<Divida>();
            modelDivida.ToTable("dividas");
            modelDivida.Property(e => e.Id).HasColumnName("id");
            modelDivida.Property(e => e.ClienteId).HasColumnName("cliente_id");
            modelDivida.Property(e => e.Valor).HasColumnName("valor");
            modelDivida.Property(e => e.Paga).HasColumnName("paga");
            modelDivida.Property(e => e.DataCriacao).HasColumnName("data_criacao");
            modelDivida.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
            modelDivida.HasKey(e => e.Id);
            modelDivida.HasOne(e => e.Cliente).WithMany(e => e.Dividas).HasForeignKey(e => e.ClienteId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
