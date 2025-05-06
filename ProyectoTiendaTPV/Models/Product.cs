using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoTiendaTPV.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)] // Un poco más largo para nombres de productos
        public string Name { get; set; }

        [MaxLength(500)] // Para una descripción opcional más larga
        public string Description { get; set; } 

        [Required]
        [Column(TypeName = "decimal(10, 2)")] 
        public decimal Price { get; set; }

        [Required]
        public int? StockQuantity { get; set; } 

        [Required]
        [MaxLength(50)]
        public string Barcode { get; set; }

        [Required]
        [MaxLength(260)] 
        public string ImagePath { get; set; }

        // --- Relación con Subfamily ---
        // 1. Clave Foránea
        [Required]
        public int SubfamilyId { get; set; }

        // 2. Propiedad de navegación
        [ForeignKey("SubfamilyId")]
        public virtual Subfamily Subfamily { get; set; }

        public virtual ICollection<TicketItem> TicketItems { get; set; } = new List<TicketItem>();
    }
}