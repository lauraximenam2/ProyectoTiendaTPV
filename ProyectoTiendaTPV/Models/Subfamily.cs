using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace ProyectoTiendaTPV.Models
{
    public class Subfamily
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // --- Relación con Family ---

        // 1. Clave Foránea 
        public int FamilyId { get; set; }

        // 2. Propiedad de navegación hacia la Familia relacionada
        [ForeignKey("FamilyId")] // Vincula esta propiedad con la FK FamilyId
        public virtual Family Family { get; set; }

        // --- Relación con Product ---
        // Una subfamilia puede tener muchos productos
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}