using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProyectoTiendaTPV.Data;
using ProyectoTiendaTPV.Models;
using ProyectoTiendaTPV.Views; 
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProyectoTiendaTPV.ViewModels
{
    public partial class ProductManagementViewModel : ObservableObject
    {
        // --- Indicador General de Carga ---
        [ObservableProperty]
        private bool _isLoading = false;

        // --- Propiedades para Productos ---
        [ObservableProperty]
        private ObservableCollection<Product> _products = new ObservableCollection<Product>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditProductCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteProductCommand))]
        private Product? _selectedProduct;

        // --- Propiedades para Familias ---
        [ObservableProperty]
        private ObservableCollection<Family> _families = new ObservableCollection<Family>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditFamilyCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteFamilyCommand))]
        private Family? _selectedFamily;

        // --- Propiedades para Subfamilias ---
        [ObservableProperty]
        private ObservableCollection<Subfamily> _subfamilies = new ObservableCollection<Subfamily>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditSubfamilyCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteSubfamilyCommand))]
        private Subfamily? _selectedSubfamily;

        // --- Propiedades para Historial de Tickets ---
        [ObservableProperty]
        private ObservableCollection<Ticket> _tickets = new ObservableCollection<Ticket>();

        [ObservableProperty]     
        private Ticket? _selectedTicket;

        [ObservableProperty]
        private ObservableCollection<TicketItem> _selectedTicketItems = new ObservableCollection<TicketItem>();

        // --- Constructor ---
        public ProductManagementViewModel()
        {
            // Carga inicial de todos los datos
            _ = LoadAllDataAsync();
        }

        // --- Métodos de Carga ---

        public async Task LoadAllDataAsync() 
        {
            IsLoading = true;

            await Task.WhenAll(
                LoadProductsAsync(false),
                LoadFamiliesAsync(false),
                LoadSubfamiliesAsync(false),
                LoadTicketsAsync(false) 
            );
            IsLoading = false;
        }
        private bool CanAlwaysExecute()
        {
            return true;
        }


        [RelayCommand] 

        private async Task LoadProductsAsync(bool setLoading = true)
        {
            if (setLoading) IsLoading = true;

            try
            {
                using (var context = new AppDbContext())
                {
                    var productsFromDb = await context.Products
                                                    .Include(p => p.Subfamily)
                                                        .ThenInclude(s => s.Family) // Incluye Familia desde Subfamilia
                                                    .OrderBy(p => p.Name)
                                                    .ToListAsync();
                    Products.Clear(); // Limpiar colección antes de añadir
                    foreach (var product in productsFromDb) Products.Add(product);
                }
                Debug.WriteLine($"Cargados {Products.Count} productos.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar productos: {ex.Message}");
                MessageBox.Show($"Error al cargar los productos:\n{ex.Message}", "Error BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { if (setLoading) IsLoading = false; }
        }





        [RelayCommand]
        private async Task LoadFamiliesAsync(bool setLoading = true)
        {
            if (setLoading) IsLoading = true;
            try
            {
                using (var context = new AppDbContext())
                {
                    var familiesFromDb = await context.Families.OrderBy(f => f.Name).ToListAsync();
                    Families.Clear(); // Limpiar colección antes de añadir
                    foreach (var family in familiesFromDb) Families.Add(family);
                }
                Debug.WriteLine($"Cargadas {Families.Count} familias.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar familias: {ex.Message}");
                MessageBox.Show($"Error al cargar las familias:\n{ex.Message}", "Error BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { if (setLoading) IsLoading = false; }
        }

        [RelayCommand] 
        private async Task LoadSubfamiliesAsync(bool setLoading = true)
        {
            if (setLoading) IsLoading = true;
            try
            {
                using (var context = new AppDbContext())
                {
                    var subfamiliesFromDb = await context.Subfamilies
                                                          .Include(s => s.Family) // Incluir Familia
                                                          .OrderBy(s => s.Family.Name).ThenBy(s => s.Name)
                                                          .ToListAsync();
                    Subfamilies.Clear(); // Limpiar colección antes de añadir
                    foreach (var subfamily in subfamiliesFromDb) Subfamilies.Add(subfamily);
                }
                Debug.WriteLine($"Cargadas {Subfamilies.Count} subfamilias.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar subfamilias: {ex.Message}");
                MessageBox.Show($"Error al cargar las subfamilias:\n{ex.Message}", "Error BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { if (setLoading) IsLoading = false; }
        }
        [RelayCommand] 
        private async Task LoadTicketsAsync(bool setLoading = true)
        {
            if (setLoading) IsLoading = true;
            Tickets.Clear();
            SelectedTicketItems.Clear(); // Limpiar detalles al recargar la lista principal
            SelectedTicket = null;     // Deseleccionar ticket previo
            try
            {
                using (var context = new AppDbContext())
                {
                    // Cargar tickets ordenados por fecha descendente (más recientes primero)
                    // Incluir TicketItems y luego el Producto de cada Item para mostrar detalles
                    var ticketsFromDb = await context.Tickets
                                                    .Include(t => t.TicketItems) // Carga los items relacionados
                                                        .ThenInclude(ti => ti.Product) // Desde los items, carga el producto
                                                    .OrderByDescending(t => t.DateTimeCreated)
                                                    .ToListAsync();
                    foreach (var ticket in ticketsFromDb) Tickets.Add(ticket);
                }
                Debug.WriteLine($"Cargados {Tickets.Count} tickets.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar tickets: {ex.Message}");
                MessageBox.Show($"Error al cargar el historial de ventas:\n{ex.Message}", "Error BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { if (setLoading) IsLoading = false; }
        }
        // --- Comandos CRUD Productos ---

        [RelayCommand]
        private async Task ReLoadProducts()
        {
            try
            {
                await LoadProductsAsync();
                MessageBox.Show("Los productos se han recargado de forma correcta.");
            }
            catch
            {
                MessageBox.Show("Error al recargar los productos desde la base de datos.");
            }

        }


        [RelayCommand]
        private async Task AddProduct()
        {
            var addViewModel = new AddEditProductViewModel();
            var addWindow = new AddEditProductWindow(addViewModel);
            addWindow.Owner = Application.Current.MainWindow;
            bool? result = addWindow.ShowDialog();
            if (result == true)
            {
                await LoadProductsAsync(); // Recargar solo productos
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteProduct))]
        private async Task EditProduct()
        {
            if (SelectedProduct == null) return;
            var editViewModel = new AddEditProductViewModel(SelectedProduct);
            var editWindow = new AddEditProductWindow(editViewModel);
            editWindow.Owner = Application.Current.MainWindow;
            bool? result = editWindow.ShowDialog();
            if (result == true)
            {
                await LoadProductsAsync(); // Recargar solo productos
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteProduct))]
        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;
            var productToDelete = SelectedProduct;

            MessageBoxResult confirm = MessageBox.Show($"¿Seguro que quieres eliminar el producto '{productToDelete.Name}'?",
                                                     "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                using (var context = new AppDbContext())
                {
                    bool isInTicket = await context.TicketItems.AnyAsync(ti => ti.ProductId == productToDelete.Id);
                    if (isInTicket)
                    {
                        MessageBox.Show($"No se puede eliminar el producto '{productToDelete.Name}' porque ha sido vendido (está en algún ticket).\nConsidere marcarlo como inactivo en lugar de eliminarlo.", "Error de Eliminación", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {

                        context.Products.Remove(productToDelete);
                        await context.SaveChangesAsync();
                        MessageBox.Show($"Producto '{productToDelete.Name}' eliminado correctamente.");
                        Products.Remove(productToDelete); // Quitar de la lista local
                        SelectedProduct = null; // Deseleccionar
                    }
                }
            }
            catch (DbUpdateException dbEx) { MessageBox.Show($"Error BD al eliminar producto: {dbEx.InnerException?.Message ?? dbEx.Message}", "Error BD"); }
            catch (Exception ex) { MessageBox.Show($"Error inesperado al eliminar producto: {ex.Message}", "Error"); }
            finally { IsLoading = false; }
        }
        private bool CanEditOrDeleteProduct() => SelectedProduct != null;


        // --- Comandos CRUD Familias ---

        [RelayCommand]
        private async Task ReLoadFamilys()
        {
            try
            {
                await LoadFamiliesAsync();
                MessageBox.Show("Las familias se han recargado de forma correcta.");
            }
            catch
            {
                MessageBox.Show("Error al recargar los productos desde la base de datos.");
            }

        }

        [RelayCommand]
        private async Task AddFamily()
        {
            var addWindow = new AddEditFamilyWindow();
            addWindow.Owner = Application.Current.MainWindow;
            bool? result = addWindow.ShowDialog();
            if (result == true)
            {
                await LoadFamiliesAsync(); // Recargar familias
                // Recargar subfamilias puede ser necesario si el orden o algo depende de las familias
                await LoadSubfamiliesAsync(false);
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteFamily))]
        private async Task EditFamily()
        {
            if (SelectedFamily == null) return;
            var editWindow = new AddEditFamilyWindow(SelectedFamily); // Pasa la familia seleccionada
            editWindow.Owner = Application.Current.MainWindow;
            bool? result = editWindow.ShowDialog();
            if (result == true)
            {
                await LoadFamiliesAsync(); // Recargar familias
                                           // Recargar otros por si los nombres cambiaron y afectan la visualización
                await LoadSubfamiliesAsync(false);
                await LoadProductsAsync(false);
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteFamily))]
        private async Task DeleteFamily()
        {
            if (SelectedFamily == null) return;
            MessageBoxResult confirm = MessageBox.Show($"¿Seguro que quieres eliminar la familia '{SelectedFamily.Name}'?\n¡Esto podría afectar a subfamilias y productos!", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                using (var context = new AppDbContext())
                {
                    bool hasSubfamilies = await context.Subfamilies.AnyAsync(s => s.FamilyId == SelectedFamily.Id);
                    if (hasSubfamilies)
                    {
                        MessageBox.Show($"No se puede eliminar la familia '{SelectedFamily.Name}' porque tiene subfamilias asociadas.\nElimine o reasigne primero las subfamilias.", "Error de Eliminación", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        var familyToDelete = await context.Families.FindAsync(SelectedFamily.Id);
                        if (familyToDelete != null)
                        {
                            context.Families.Remove(familyToDelete);
                            await context.SaveChangesAsync();
                            MessageBox.Show($"Familia '{SelectedFamily.Name}' eliminada.", "Éxito");
                            await LoadFamiliesAsync(); // Recargar vista
                        }
                    }
                }
            }
            catch (DbUpdateException dbEx) { MessageBox.Show($"Error BD al eliminar familia: {dbEx.InnerException?.Message ?? dbEx.Message}", "Error BD"); }
            catch (Exception ex) { MessageBox.Show($"Error inesperado al eliminar familia: {ex.Message}", "Error"); }
            finally { IsLoading = false; }
        }
        private bool CanEditOrDeleteFamily() => SelectedFamily != null;

        // --- Comandos CRUD Subfamilias ---

        [RelayCommand]
        private async Task ReLoadSubfamilys()
        {
            try
            {
                await LoadSubfamiliesAsync(); 
                MessageBox.Show("Las subfamilias se han recargado de forma correcta.");
            }
            catch
            {
                MessageBox.Show("Error al recargar las subfamilias desde la base de datos.");
            }

        }

        [RelayCommand]
        private async Task AddSubfamily() 
        {
            var addWindow = new AddEditSubfamilyWindow(); 
            addWindow.Owner = Application.Current.MainWindow;
            bool? result = addWindow.ShowDialog();
            if (result == true)
            {
                await LoadSubfamiliesAsync(); // Recargar subfamilias
                                              // Podría ser necesario recargar productos si muestran datos de subfamilia
                await LoadProductsAsync(false);
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteSubfamily))]
        private async Task EditSubfamily() 
        {
            if (SelectedSubfamily == null) return;

            var editWindow = new AddEditSubfamilyWindow(SelectedSubfamily); // Pasa la subfamilia
            editWindow.Owner = Application.Current.MainWindow;
            bool? result = editWindow.ShowDialog();
            if (result == true)
            {
                await LoadSubfamiliesAsync(); // Recargar subfamilias
                await LoadProductsAsync(false); // Recargar productos
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteSubfamily))]
        private async Task DeleteSubfamily() 
        {
            if (SelectedSubfamily == null) return;
            MessageBoxResult confirm = MessageBox.Show($"¿Seguro que quieres eliminar la subfamilia '{SelectedSubfamily.Name}'?\n¡Esto podría afectar a productos asociados!", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                using (var context = new AppDbContext())
                {
                    bool hasProducts = await context.Products.AnyAsync(p => p.SubfamilyId == SelectedSubfamily.Id);
                    if (hasProducts)
                    {
                        MessageBox.Show($"No se puede eliminar la subfamilia '{SelectedSubfamily.Name}' porque tiene productos asociados.\nElimine o reasigne primero los productos.", "Error de Eliminación", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        var subfamilyToDelete = await context.Subfamilies.FindAsync(SelectedSubfamily.Id);
                        if (subfamilyToDelete != null)
                        {
                            context.Subfamilies.Remove(subfamilyToDelete);
                            await context.SaveChangesAsync();
                            MessageBox.Show($"Subfamilia '{SelectedSubfamily.Name}' eliminada.", "Éxito");
                            await LoadSubfamiliesAsync(); // Recargar vista
                        }
                    }
                }
            }
            catch (DbUpdateException dbEx) { MessageBox.Show($"Error BD al eliminar subfamilia: {dbEx.InnerException?.Message ?? dbEx.Message}", "Error BD"); }
            catch (Exception ex) { MessageBox.Show($"Error inesperado al eliminar subfamilia: {ex.Message}", "Error"); }
            finally { IsLoading = false; }
        }
        private bool CanEditOrDeleteSubfamily() => SelectedSubfamily != null;


        

        [RelayCommand]
        private async Task ReLoadLoadTickets()
        {
            try
            {
                await LoadTicketsAsync();
                MessageBox.Show("Los tickets se han recargado de forma correcta.");
            }
            catch
            {
                MessageBox.Show("Error al recargar los tickets desde la base de datos.");
            }

        }



        // Este método se llama automáticamente cuando SelectedTicket cambia
        partial void OnSelectedTicketChanged(Ticket? value)
        {
            // Actualizar la lista de items para mostrar en el detalle
            SelectedTicketItems.Clear();
            if (value?.TicketItems != null) // Si el ticket seleccionado y sus items no son nulos
            {
                foreach (var item in value.TicketItems)
                {
                    SelectedTicketItems.Add(item);
                }
            }
            Debug.WriteLine($"Ticket seleccionado: {value?.TicketNumber}. Items: {SelectedTicketItems.Count}");

        }

        // --- Manejadores de Cambio de Selección (Notifican a los Comandos) ---

        partial void OnSelectedProductChanged(Product? value)
        {
            EditProductCommand.NotifyCanExecuteChanged();
            DeleteProductCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedFamilyChanged(Family? value)
        {
            EditFamilyCommand.NotifyCanExecuteChanged();
            DeleteFamilyCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedSubfamilyChanged(Subfamily? value)
        {
            EditSubfamilyCommand.NotifyCanExecuteChanged();
            DeleteSubfamilyCommand.NotifyCanExecuteChanged();
        }




    }
}