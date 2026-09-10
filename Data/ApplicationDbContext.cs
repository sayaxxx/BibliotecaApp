// ============================================================================
//  ApplicationDbContext : contexto de Entity Framework Core
// ----------------------------------------------------------------------------
//  Puente entre la aplicación y la base de datos SQL Server. Declara los
//  conjuntos de entidades (DbSet) y configura las relaciones y el borrado
//  en cascada entre las tablas. La cadena de conexión se lee de
//  appsettings.json ("DefaultConnection") en Program.cs.
// ============================================================================
using Microsoft.EntityFrameworkCore;
using BibliotecaApp.Models;

namespace BibliotecaApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>Tabla de autores.</summary>
        public DbSet<Autor> Autores { get; set; }

        /// <summary>Tabla de libros.</summary>
        public DbSet<Libro> Libros { get; set; }

        /// <summary>Tabla de préstamos.</summary>
        public DbSet<Prestamo> Prestamos { get; set; }

        /// <summary>
        /// Configuración de las entidades: relaciones y comportamiento de borrado.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación Libro -> Autor (N:1). Al borrar un autor se borran
            // sus libros en cascada (DeleteBehavior.Cascade).
            modelBuilder.Entity<Libro>()
                .HasOne(l => l.Autor)
                .WithMany(a => a.Libros)
                .HasForeignKey(l => l.AutorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Prestamo -> Libro (N:1). Al borrar un libro se borran
            // sus préstamos en cascada.
            modelBuilder.Entity<Prestamo>()
                .HasOne(p => p.Libro)
                .WithMany(l => l.Prestamos)
                .HasForeignKey(p => p.LibroId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}