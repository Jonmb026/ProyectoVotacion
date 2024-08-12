using Microsoft.EntityFrameworkCore;
using ProyectoVotacion.Models;

namespace ProyectoVotacion.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Candidato> Candidatos { get; set; }
        public DbSet<Voto> Votos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Voto>()
                .HasIndex(v => new { v.UsuarioId, v.CandidatoId })
                .IsUnique();  // Asegura que un usuario solo pueda votar una vez por candidato
        }
    }
}
