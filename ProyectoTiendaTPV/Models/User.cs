using ProyectoTiendaTPV.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProyectoTiendaTPV.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; } // Almacenará el hash BCrypt

        [Required]
        public UserRole Role { get; set; } // Almacena el rol del enum
    }
}