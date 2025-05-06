using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProyectoTiendaTPV.Data;
using ProyectoTiendaTPV.Models; 
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; 

namespace ProyectoTiendaTPV.ViewModels
{


    public partial class SalesViewModel : ObservableObject
    {

        // --- Define la cultura a usar para la moneda ---
        private static readonly CultureInfo EuroCulture = new CultureInfo("es-ES");

        // --- Propiedades Observables ---

        [ObservableProperty]
        private ObservableCollection<Product> _availableProducts = new ObservableCollection<Product>();

        [ObservableProperty]
        private ObservableCollection<TicketLineItem> _currentTicketItems = new ObservableCollection<TicketLineItem>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(FinalizeSaleCommand))] 
        [NotifyCanExecuteChangedFor(nameof(CancelSaleCommand))] 
        private decimal _totalAmount = 0m;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private ObservableCollection<Family> _allFamilies = new ObservableCollection<Family>();

        [ObservableProperty]
        private ObservableCollection<Subfamily> _allSubfamilies = new ObservableCollection<Subfamily>();

        // Colección para mostrar las subfamilias del filtro (se actualiza al seleccionar familia)
        [ObservableProperty]
        private ObservableCollection<Subfamily> _filterableSubfamilies = new ObservableCollection<Subfamily>();

        // Propiedades para el estado actual del filtro
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyCategoryFilterActive))] // Notifica cambio relacionado
        private Family? _selectedFamilyFilter;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyCategoryFilterActive))] // Notifica cambio relacionado
        private Subfamily? _selectedSubfamilyFilter;

        [ObservableProperty]
        private string? _searchText;

        // Lista original de productos (privada) - La cargamos una vez
        private List<Product> _originalAvailableProducts = new List<Product>();

        [ObservableProperty]
        private ObservableCollection<Product> _filteredProducts = new ObservableCollection<Product>();

        // Propiedad calculada para saber si hay filtros activos 
        public bool IsAnyCategoryFilterActive => SelectedFamilyFilter != null || SelectedSubfamilyFilter != null;


        // --- Constructor ---
        public SalesViewModel()
        {
            _ = LoadInitialDataAsync(); //Metodo de carga inicial
        }

        public async Task LoadInitialDataAsync() // La hacemos pública para que la podemos usar desde otras clases
        {
            IsLoading = true;
            try
            {
                using (var context = new AppDbContext())
                {
                    // Cargar Familias
                    var familiesFromDb = await context.Families.OrderBy(f => f.Name).ToListAsync();
                    AllFamilies.Clear();
                    foreach (var f in familiesFromDb) AllFamilies.Add(f);

                    // Cargar TODAS las Subfamilias (incluyendo su Familia)
                    var subfamiliesFromDb = await context.Subfamilies.Include(s => s.Family).OrderBy(s => s.Name).ToListAsync();
                    AllSubfamilies.Clear();
                    foreach (var sf in subfamiliesFromDb) AllSubfamilies.Add(sf);

                    // Cargar TODOS los Productos disponibles
                    var productsFromDb = await context.Products                                                   
                                                    .Include(p => p.Subfamily) 
                                                        .ThenInclude(s => s.Family) 
                                                    .OrderBy(p => p.Name)
                                                    .ToListAsync();
                    _originalAvailableProducts = productsFromDb; 

                    Debug.WriteLine($"Cargados {AllFamilies.Count} familias, {AllSubfamilies.Count} subfamilias, {_originalAvailableProducts.Count} productos.");
                }
                // Aplicar filtros iniciales 
                ApplyFilters();
                // Poblar la lista de subfamilias filtrables inicialmente 
                UpdateFilterableSubfamilies();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error carga inicial ventas: {ex.Message}");
                MessageBox.Show($"Error al cargar datos iniciales:\n{ex.Message}", "Error BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // --- Carga de Datos ---
        [RelayCommand]
        private async Task LoadAvailableProductsAsync()
        {
            IsLoading = true;
            AvailableProducts.Clear(); // Limpiar antes de cargar
            try
            {
                using (var context = new AppDbContext())
                {
                    // Cargamos productos que podríamos querer vender 
                    // Incluimos datos necesarios para mostrar (Subfamilia/Familia opcional, ImagePath sí)
                    var productsFromDb = await context.Products
                                                    .OrderBy(p => p.Name)
                                                    .ToListAsync();

                    foreach (var product in productsFromDb)
                    {
                        AvailableProducts.Add(product);
                    }
                }
                Debug.WriteLine($"Cargados {AvailableProducts.Count} productos disponibles.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar productos disponibles: {ex.Message}");
                MessageBox.Show($"Error al cargar productos:\n{ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // --- Comandos de Venta ---

        [RelayCommand]
        private void AddProductToTicket(Product? productToAdd)
        {
            if (productToAdd == null) return;

            // --- Verificación de Stock ---
            int? currentStock = productToAdd.StockQuantity;
            int quantityInTicket = 0;

            var existingItem = CurrentTicketItems.FirstOrDefault(item => item.SaleProduct.Id == productToAdd.Id);
            if (existingItem != null)
            {
                quantityInTicket = existingItem.Quantity;
            }

            // ¿Hay suficiente stock para añadir UNA unidad más?
            if (currentStock <= quantityInTicket)
            {
                MessageBox.Show($"No hay suficiente stock disponible para '{productToAdd.Name}'.\nStock actual: {currentStock}",
                                "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // No añadir ni incrementar
            }
           

            // Si hay stock
            if (existingItem != null)
            {
                // Incrementar cantidad (ya sabemos que hay stock para al menos una más)
                existingItem.IncrementQuantity();
            }
            else
            {
                // Añadir nueva línea al ticket
                var newItem = new TicketLineItem(productToAdd);
                CurrentTicketItems.Add(newItem);
            }

            CalculateTotal();
        }


        // En SalesViewModel.cs
        [RelayCommand]
        private void IncrementQuantity(TicketLineItem? item)
        {
            if (item != null)
            {
                // Verificar stock ANTES de incrementar
                if (item.SaleProduct.StockQuantity > item.Quantity)
                {
                    item.IncrementQuantity();
                    CalculateTotal();
                }
                else
                {
                    MessageBox.Show($"No hay más stock disponible para '{item.ProductName}'.\nStock actual: {item.SaleProduct.StockQuantity}",
                                    "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        [RelayCommand]
        private void DecrementQuantity(TicketLineItem? item)
        {
            if (item != null)
            {
                // El método DecrementQuantity en TicketLineItem ya evita ir por debajo de 1
                if (item.DecrementQuantity()) // Solo recalcula si realmente cambió
                {
                    CalculateTotal();
                }
                
            }
        }

        [RelayCommand]
        private void RemoveItemFromTicket(TicketLineItem? item)
        {
            if (item != null)
            {
                CurrentTicketItems.Remove(item);
                CalculateTotal(); // Recalcular después de eliminar
            }
        }
        [RelayCommand(CanExecute = nameof(CanFinalizeOrCancelSale))]
        private async Task FinalizeSale()
        {
            if (!CurrentTicketItems.Any()) return;

            MessageBoxResult confirm = MessageBox.Show($"Confirmar venta por un total de {TotalAmount.ToString("C", EuroCulture)}?",
                                                       "Confirmar Venta",
                                                       MessageBoxButton.YesNo,
                                                       MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            IsLoading = true;

            
            using (var context = new AppDbContext())

            {
                bool saveSuccess = false;
                Ticket? savedTicket = null; // Para guardar referencia al ticket guardado

                try
                {
                    // --- 1. Crear y Guardar Ticket e Items ---
                    var ticket = new Ticket
                    {
                        TicketNumber = $"{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 4)}", // Ejemplo sin prefijo
                        DateTimeCreated = DateTime.Now,
                        TotalAmount = this.TotalAmount
                        
                    };

                    foreach (var lineItem in CurrentTicketItems)
                    {
                        var ticketItem = new TicketItem
                        {
                            ProductId = lineItem.SaleProduct.Id,
                            Quantity = lineItem.Quantity,
                            UnitPrice = lineItem.UnitPrice
                        };
                        ticket.TicketItems.Add(ticketItem);
                    }

                    context.Tickets.Add(ticket);
                    await context.SaveChangesAsync(); // Guarda Ticket e Items
                    savedTicket = ticket; // Guarda referencia 


                    // --- 2. Actualizar Stock ---

                    foreach (var item in ticket.TicketItems) // Iterar sobre los items guardados
                    {
                        var productToUpdate = await context.Products.FindAsync(item.ProductId);
                        if (productToUpdate != null)
                        {
                            // Descontar stock
                            productToUpdate.StockQuantity -= item.Quantity;

                            // Lógica para evitar stock negativo
                            if (productToUpdate.StockQuantity < 0)
                            {
                                
                                Debug.WriteLine($"ADVERTENCIA: Stock negativo para producto ID {productToUpdate.Id} ({productToUpdate.Name}). Stock actual: {productToUpdate.StockQuantity}");
                                
                            }

                            context.Products.Update(productToUpdate); // Marcar para actualizar
                        }
                        else
                        {
                            // Producto no encontrado, esto sería un error de datos inconsistentes
                            Debug.WriteLine($"ERROR: Producto con ID {item.ProductId} no encontrado al intentar actualizar stock.");
                            
                        }
                    }

                    // Guardar los cambios de stock
                    await context.SaveChangesAsync();


                    saveSuccess = true; 

                    // --- RECARGAR DATOS DESPUÉS DE ACTUALIZAR STOCK ---
                    await LoadInitialDataAsync();                  

                }


                catch (DbUpdateException dbEx)
                {

                    Debug.WriteLine($"Error de BD al guardar venta/stock: {dbEx.InnerException?.Message ?? dbEx.Message}");
                    MessageBox.Show($"Error de base de datos al guardar la venta o actualizar stock.\nDetalles: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                    "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                    saveSuccess = false; 
                }

                catch (Exception ex)
                {
                    Debug.WriteLine($"Error inesperado al guardar venta/stock: {ex.Message}");
                    MessageBox.Show($"Ocurrió un error inesperado al guardar la venta o actualizar stock:\n{ex.Message}",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    saveSuccess = false; 
                }


                // --- 3. Finalizar ---
                if (saveSuccess && savedTicket != null)
                {
                    GenerateAndShowReceipt(savedTicket); // Generar recibo
                    ClearTicket(); // Limpiar vista
                }

            } 

            IsLoading = false;
        } 

        // --- RECIBO ---
        private void GenerateAndShowReceipt(Ticket ticket)
        {
            var receiptBuilder = new System.Text.StringBuilder();
            receiptBuilder.AppendLine("****** MI TIENDA TPV ******");
            receiptBuilder.AppendLine($"Ticket #: {ticket.TicketNumber}");
            receiptBuilder.AppendLine($"Fecha: {ticket.DateTimeCreated:dd/MM/yyyy HH:mm:ss}");
            receiptBuilder.AppendLine("------------------------------");
            receiptBuilder.AppendLine("Cant. Producto         Subtotal");
            receiptBuilder.AppendLine("------------------------------");

            // Añadir cada item (necesitamos los nombres de producto)
            // Usamos los datos del objeto Ticket que ya tiene los TicketItems asociados
            foreach (var item in ticket.TicketItems)
            {
                // Necesitamos obtener el nombre del producto. El objeto 'item' tiene ProductId,
                // pero no el objeto Product completo a menos que lo carguemos.
                var originalLineItem = CurrentTicketItems.FirstOrDefault(li => li.SaleProduct.Id == item.ProductId);
                string productName = originalLineItem?.ProductName ?? $"Producto ID: {item.ProductId}";
                decimal itemTotal = item.Quantity * item.UnitPrice;

                // Formateo con cultura Euro
                receiptBuilder.AppendLine($"{item.Quantity.ToString().PadRight(4)} {productName.PadRight(15).Substring(0, 15)} {itemTotal.ToString("C", EuroCulture).PadLeft(8)}");
            }

            receiptBuilder.AppendLine("------------------------------");
            receiptBuilder.AppendLine($"TOTAL: {ticket.TotalAmount.ToString("C", EuroCulture).PadLeft(22)}");
            receiptBuilder.AppendLine();
            receiptBuilder.AppendLine("Gracias por su compra!");

            // Mostrar en un MessageBox
            MessageBox.Show(receiptBuilder.ToString(), "Recibo de Venta", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        // Comando para cancelar
        [RelayCommand(CanExecute = nameof(CanFinalizeOrCancelSale))]
        private void CancelSale()
        {
            MessageBoxResult confirm = MessageBox.Show("¿Está seguro de que desea cancelar la venta actual y vaciar el ticket?",
                                                       "Cancelar Venta",
                                                       MessageBoxButton.YesNo,
                                                       MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.Yes)
            {
                ClearTicket();
            }
        }

        // comandos de los filtros

        [RelayCommand]
        private void SelectFamilyFilter(Family? family)
        {
            SelectedFamilyFilter = family;
            SelectedSubfamilyFilter = null; // Limpiar filtro de subfamilia al cambiar familia
            UpdateFilterableSubfamilies(); // Actualizar lista de subfamilias disponibles
            ApplyFilters(); // Aplicar el filtro principal
            OnPropertyChanged(nameof(IsAnyCategoryFilterActive)); // Notificar cambio del estado del filtro
        }

        [RelayCommand]
        private void SelectSubfamilyFilter(Subfamily? subfamily)
        {
            SelectedSubfamilyFilter = subfamily;
            // Si seleccionamos una subfamilia, nos aseguramos de que su familia esté seleccionada
            if (subfamily != null && SelectedFamilyFilter?.Id != subfamily.FamilyId)
            {
                SelectedFamilyFilter = AllFamilies.FirstOrDefault(f => f.Id == subfamily.FamilyId);
                
            }
            ApplyFilters();
            OnPropertyChanged(nameof(IsAnyCategoryFilterActive));
        }

        [RelayCommand]
        private void ClearCategoryFilters()
        {
            SelectedFamilyFilter = null;
            SelectedSubfamilyFilter = null;
            UpdateFilterableSubfamilies(); // Mostrar todas de nuevo
            ApplyFilters();
            OnPropertyChanged(nameof(IsAnyCategoryFilterActive));
        }




        // --- Métodos Auxiliares ---

        private void CalculateTotal()
        {
            TotalAmount = CurrentTicketItems.Sum(item => item.ItemTotal);
            
        }

        private void ClearTicket()
        {
            CurrentTicketItems.Clear();
            CalculateTotal(); // Resetea el total a 0
        }

        // Lógica para habilitar/deshabilitar botones Finalizar/Cancelar
        private bool CanFinalizeOrCancelSale()
        {
            // Solo se pueden usar si hay al menos un item en el ticket
            return CurrentTicketItems.Any();
        }

        private void ApplyFilters()
        {
            IEnumerable<Product> filtered = _originalAvailableProducts; // Empezar con todos

            // Aplicar filtro de Familia
            if (SelectedFamilyFilter != null)
            {
                filtered = filtered.Where(p => p.Subfamily?.FamilyId == SelectedFamilyFilter.Id);
            }

            // Aplicar filtro de Subfamilia 
            if (SelectedSubfamilyFilter != null)
            {
                filtered = filtered.Where(p => p.SubfamilyId == SelectedSubfamilyFilter.Id);
            }

            // Aplicar filtro de Texto de Búsqueda
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lowerSearchText = SearchText.ToLowerInvariant().Trim();
                filtered = filtered.Where(p =>
                    (p.Name?.ToLowerInvariant().Contains(lowerSearchText) ?? false) ||
                    (p.Barcode?.ToLowerInvariant().Contains(lowerSearchText) ?? false) || // Buscar también por código de barras
                    (p.Description?.ToLowerInvariant().Contains(lowerSearchText) ?? false) // Buscar en descripción
                );
            }

            // Actualiza
            FilteredProducts.Clear();
            foreach (var product in filtered)
            {
                FilteredProducts.Add(product);
            }
            Debug.WriteLine($"Filtro aplicado. Mostrando {FilteredProducts.Count} productos.");
        }

        // Método auxiliar para actualizar las subfamilias mostradas en el filtro
        private void UpdateFilterableSubfamilies()
        {
            FilterableSubfamilies.Clear();
            if (SelectedFamilyFilter == null)
            {
                // Si no hay familia seleccionada, mostrar todas las subfamilias
                foreach (var sf in AllSubfamilies.OrderBy(s => s.Name)) FilterableSubfamilies.Add(sf);
            }
            else
            {
                // Mostrar solo subfamilias de la familia seleccionada
                foreach (var sf in AllSubfamilies.Where(s => s.FamilyId == SelectedFamilyFilter.Id).OrderBy(s => s.Name))
                {
                    FilterableSubfamilies.Add(sf);
                }
            }
        }

        // Este método se llama automáticamente cuando la propiedad SearchText cambia
        partial void OnSearchTextChanged(string? value)
        {
            ApplyFilters(); // Re-aplicar filtros cuando el texto de búsqueda cambia
        }
       
    }
}