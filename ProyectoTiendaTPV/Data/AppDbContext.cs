using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using ProyectoTiendaTPV.Models; // Acceso a las clases modelo
using System.IO; 
using System; 


namespace ProyectoTiendaTPV.Data
{
    public class AppDbContext : DbContext
    {
        // --- Definición de las tablas (DbSet) ---
        // Cada DbSet<T> representa una tabla en la base de datos
        public DbSet<Family> Families { get; set; }
        public DbSet<Subfamily> Subfamilies { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketItem> TicketItems { get; set; }

        // --- Configuración de la Conexión --- 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            if (!optionsBuilder.IsConfigured)
            {
                // Defininimos la cadena de conexión - base de datos = 'ProyectoTiendaTPV_DB'
                string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ProyectoTiendaTPV_DB;Trusted_Connection=True;MultipleActiveResultSets=true";

                optionsBuilder.UseSqlServer(connectionString);


            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            // Asegura que el número de ticket sea único
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => t.TicketNumber)
                .IsUnique();

            // Define explícitamente el tipo decimal para UnitPrice en TicketItem
            modelBuilder.Entity<TicketItem>()
                .Property(ti => ti.UnitPrice)
                .HasColumnType("decimal(10, 2)");

            // Configuración de otras relaciones 
            modelBuilder.Entity<TicketItem>()
                .HasOne(ti => ti.Ticket)
                .WithMany(t => t.TicketItems)
                .HasForeignKey(ti => ti.TicketId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<TicketItem>()
                .HasOne(ti => ti.Product)
                .WithMany(p => p.TicketItems) // Necesita la colección en Product.cs
                .HasForeignKey(ti => ti.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // No borrar producto si está en un ticket

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Barcode)
                .IsUnique(); // Asegura que no haya dos productos con el mismo código de barras (si Barcode no es nulo)

        }
    }
}