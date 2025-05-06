using CommunityToolkit.Mvvm.ComponentModel; 
using ProyectoTiendaTPV.Models; 

namespace ProyectoTiendaTPV.Models 
{
    // Usamios ObservableObject para que si cambia Quantity, podamos recalcular ItemTotal y notificar
    public partial class TicketLineItem : ObservableObject
    {
        public Product SaleProduct { get; } // El producto añadido
        public string ProductName => SaleProduct.Name; // Acceso fácil al nombre
        public decimal UnitPrice { get; } // Precio al momento de añadir

        [ObservableProperty]
        private int _quantity;

        // Propiedad calculada para el total de la línea
        public decimal ItemTotal => Quantity * UnitPrice;

        // Constructor
        public TicketLineItem(Product product, int quantity = 1)
        {
            SaleProduct = product;
            UnitPrice = product.Price; // Guarda el precio actual del producto
            _quantity = quantity; 
        }

        // Método para incrementar cantidad
        public void IncrementQuantity()
        {
            Quantity++; // Usa la propiedad generada para disparar OnPropertyChanged
            OnPropertyChanged(nameof(ItemTotal)); // Notifica que el total también cambió
        }

        public bool DecrementQuantity()
        {
            if (Quantity > 1) // Solo decrementa si es mayor que 1
            {
                Quantity--;
                OnPropertyChanged(nameof(ItemTotal));
                return true; // Se pudo decrementar
            }
            return false; // No se decrementó (ya era 1)
            
        }

        // Sobrescribir el setter de Quantity para notificar cambio en ItemTotal
        partial void OnQuantityChanged(int value)
        {
            // Cuando la cantidad cambia, notificamos que ItemTotal también debe refrescarse 
            OnPropertyChanged(nameof(ItemTotal));
        }
    }
}