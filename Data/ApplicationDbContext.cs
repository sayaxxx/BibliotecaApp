// ============================================================================
//  ApplicationDbContext : contexto de Entity Framework Core
// ----------------------------------------------------------------------------
//  Hereda de IdentityDbContext&lt;ApplicationUser&gt; para que EF cree, además de
//  las tablas de negocio (Autores, Libros, Prestamos), las tablas de Identity
//  (AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetRoleClaims, etc.).
//  Configura las relaciones y el borrado en cascada de la base de datos.
// ============================================================================
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BibliotecaApp.Models;

namespace BibliotecaApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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
            // Configuración interna de Identity (tablas AspNet*). Siempre primero.
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

            // Relación Prestamo -> ApplicationUser (N:1) opcional.
            // Restrict: eliminar un usuario NO borra su historial de préstamos.
            modelBuilder.Entity<Prestamo>()
                .HasOne(p => p.Lector)
                .WithMany()
                .HasForeignKey(p => p.LectorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}