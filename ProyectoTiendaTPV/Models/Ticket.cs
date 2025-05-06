using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoTiendaTPV.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)] // Longitud para número de ticket
        public string TicketNumber { get; set; } // Un identificador único para el ticket

        [Required]
        public DateTime DateTimeCreated { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Precisión mayor para el total general
        public decimal TotalAmount { get; set; }

        // Propiedad de navegación para los items del ticket
        public virtual ICollection<TicketItem> TicketItems { get; set; } = new List<TicketItem>();

    }
}