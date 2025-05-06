using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; 

namespace ProyectoTiendaTPV.Models
{
    public class Family
    {
        [Key] // Indica que esta es la clave primaria 
        public int Id { get; set; }

        [Required] // Campo obligatorio
        [MaxLength(100)] // Define una longitud máxima para la columna en la BD
        public string Name { get; set; }

        // Una familia puede tener muchas subfamilias
        public virtual ICollection<Subfamily> Subfamilies { get; set; } = new List<Subfamily>();
    }
}