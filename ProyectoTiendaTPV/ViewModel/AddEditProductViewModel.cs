using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProyectoTiendaTPV.Data;
using ProyectoTiendaTPV.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations; 
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; 
using Microsoft.Win32; 
using System.IO;        
using System.Windows.Media; 
using System.Windows.Media.Imaging; 

namespace ProyectoTiendaTPV.ViewModels
{

    public partial class AddEditProductViewModel : ObservableValidator
    {
        // --- Propiedades para los campos del formulario ---
        [ObservableProperty]
        [NotifyDataErrorInfo] // Para validación
        [Required(ErrorMessage = "El nombre es obligatorio")] // Validación básica
        [MaxLength(150)]
        private string? _productName;

        [ObservableProperty]
        [MaxLength(500)]
        private string? _description;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "El precio es obligatorio")]
        private string? _price;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "El código de barras es obligatorio")] 
        [MaxLength(50)]
        private string? _barcode;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "El stock es obligatorio")] 
        private string? _stockQuantity; 

        // --- Propiedades para el ComboBox de Subfamilias ---
        [ObservableProperty]
        private ObservableCollection<Subfamily> _subfamilies = new ObservableCollection<Subfamily>();

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Debe seleccionar una subfamilia")]
        private Subfamily? _selectedSubfamily;

        [ObservableProperty]
        private ImageSource? _selectedImagePreview; // Para la previsualización


        // --- Campo para guardar el ID del producto que se está editando ---
        private readonly int? _productIdToEdit = null; 

        [ObservableProperty]
        private string _windowTitle = "Añadir Producto";


        // Esta guardará la ruta del archivo seleccionado por el usuario
        private string _selectedImageFullPath;

        // Esta guardará el NOMBRE del archivo que queremos guardar en la BD
        private string _imageFileNameToSave;

        // --- Propiedad para indicar si la operación fue exitosa ---
        public bool DialogResult { get; private set; } = false;

        // --- Cierre de la ventana ---
        public event Action? RequestClose;

        private int? _subfamilyIdToSelectInitially = null;


        // Constructor para Añadir 
        public AddEditProductViewModel()
        {
            _ = LoadSubfamiliesAsync();
            ValidateAllProperties();
        }

        public AddEditProductViewModel(Product productToEdit) : this() // Llama al constructor base para cargar subfamilias
        {
            WindowTitle = "Editar Producto";
            _productIdToEdit = productToEdit.Id; // Guarda el ID para la actualización

            // Carga los datos del producto en las propiedades del ViewModel
            ProductName = productToEdit.Name;
            Description = productToEdit.Description;
            Price = productToEdit.Price.ToString("N2"); 
            Barcode = productToEdit.Barcode;
            StockQuantity = productToEdit.StockQuantity?.ToString(); 

            _subfamilyIdToSelectInitially = productToEdit.SubfamilyId;


            // Cargams previsualización de imagen si existe
            _imageFileNameToSave = productToEdit.ImagePath; // Guarda el nombre actual
            if (!string.IsNullOrEmpty(_imageFileNameToSave))
            {
                string imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string fullPath = Path.Combine(imageDirectory, _imageFileNameToSave);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        SelectedImagePreview = bitmap;
                    }
                    catch (Exception ex) { Debug.WriteLine($"Error cargando imagen para editar: {ex}"); SelectedImagePreview = null; }
                }
                else { SelectedImagePreview = null; }
            }
            else { SelectedImagePreview = null; }

            // Re-validar después de cargar datos
            ValidateAllProperties();
        }

        // --- Carga de Datos ---
        private async Task LoadSubfamiliesAsync()
        {
            Subfamilies.Clear();
            try
            {
                using (var context = new AppDbContext())
                {
                    var subfamiliesFromDb = await context.Subfamilies
                                                        .Include(s => s.Family) 
                                                        .OrderBy(s => s.Family.Name).ThenBy(s => s.Name)
                                                        .ToListAsync();
                    foreach (var sf in subfamiliesFromDb)
                    {
                        Subfamilies.Add(sf);
                    }
                }
                // Si estamos editando y tenemos un ID de subfamilia para seleccionar
                if (_subfamilyIdToSelectInitially.HasValue)
                {
                    // Busca la subfamilia en la lista recién cargada
                    SelectedSubfamily = Subfamilies.FirstOrDefault(s => s.Id == _subfamilyIdToSelectInitially.Value);
                    _subfamilyIdToSelectInitially = null; 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar subfamilias: {ex.Message}");
                MessageBox.Show($"Error al cargar las subfamilias:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        [RelayCommand]
        private async Task SaveAsync()
        {
      
            ValidateAllProperties();
         
            bool isImageSelected = !string.IsNullOrEmpty(_imageFileNameToSave);
            if (!isImageSelected)
            {
                
                MessageBox.Show("Debe seleccionar una imagen para el producto.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; 
            }

            if (HasErrors || !isImageSelected) 
            {
                MessageBox.Show("Por favor, corrija los errores indicados y seleccione una imagen.", "Errores de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

          
            if (!decimal.TryParse(Price, out decimal priceValue) || priceValue < 0) { /* Mensaje y return */ return; }

           
            if (!int.TryParse(StockQuantity, out int stockValue) || stockValue < 0)
            {
                MessageBox.Show("La cantidad de stock introducida no es válida o está vacía.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Lógica de Copia de Imagen ---
            string finalImageNameToSaveInDb = _imageFileNameToSave; 
        
            if (!string.IsNullOrEmpty(_selectedImageFullPath))
            {
                try
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imageFolderName = "Images";
                    string imageDirectoryPath = Path.Combine(baseDirectory, imageFolderName);
                    Directory.CreateDirectory(imageDirectoryPath); // Crea si no existe
                    string destinationPath = Path.Combine(imageDirectoryPath, _imageFileNameToSave); // Usa el nombre ya guardado

                    Debug.WriteLine($"Intentando copiar desde: '{_selectedImageFullPath}'");
                    Debug.WriteLine($"Intentando copiar hacia: '{destinationPath}'");

                    File.Copy(_selectedImageFullPath, destinationPath, true); 

                    Debug.WriteLine($"Imagen copiada exitosamente a: {destinationPath}");
                    finalImageNameToSaveInDb = _imageFileNameToSave; // Confirmar nombre a guardar
                    _selectedImageFullPath = string.Empty; // Limpiar ruta temporal después de copiar
                }
                catch (IOException ioEx)
                {
                    Debug.WriteLine($"Error de I/O al copiar imagen: {ioEx.Message}");
                    MessageBox.Show($"No se pudo guardar la imagen seleccionada (Error de archivo):\n{ioEx.Message}", "Error al Guardar Imagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; 
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Debug.WriteLine($"Error de Permisos al copiar imagen: {uaEx.Message}");
                    MessageBox.Show($"No se pudo guardar la imagen seleccionada (Error de permisos).\nAsegúrate de tener permisos de escritura en la carpeta:\n{Path.GetDirectoryName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", _imageFileNameToSave))}", "Error al Guardar Imagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error general al copiar imagen: {ex.Message}");
                    MessageBox.Show($"No se pudo guardar la imagen seleccionada:\n{ex.Message}", "Error al Guardar Imagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; 
                }
            }

            // Guardar en la Base de Datos (Añadir o Actualizar)
            try
            {
                using (var context = new AppDbContext())
                {
                    if (_productIdToEdit.HasValue) 
                    {
                        var productToUpdate = await context.Products.FindAsync(_productIdToEdit.Value);
                        if (productToUpdate != null)
                        {
                            productToUpdate.Name = ProductName!;
                            productToUpdate.Description = Description!;
                            productToUpdate.Price = priceValue;
                            productToUpdate.SubfamilyId = SelectedSubfamily!.Id;
                            productToUpdate.Barcode = Barcode!; 
                            productToUpdate.StockQuantity = stockValue; 
                            productToUpdate.ImagePath = finalImageNameToSaveInDb!;

                            context.Products.Update(productToUpdate);
                            await context.SaveChangesAsync();
                            MessageBox.Show($"Producto '{productToUpdate.Name}' actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else { /* Mensaje Error no encontrado */ return; }
                    }
                    else // MODO AÑADIR
                    {
                        var newProduct = new Product
                        {
                            Name = ProductName!,
                            Description = Description!,
                            Price = priceValue,
                            SubfamilyId = SelectedSubfamily!.Id,
                            Barcode = Barcode!, 
                            StockQuantity = stockValue, 
                            ImagePath = finalImageNameToSaveInDb! 
                        };
                        context.Products.Add(newProduct);
                        await context.SaveChangesAsync();
                        MessageBox.Show($"Producto '{newProduct.Name}' añadido correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                DialogResult = true;
                RequestClose?.Invoke();
            }
            catch (DbUpdateException dbEx) { 

                MessageBox.Show($"Error de Base de Datos al guardar: {dbEx.InnerException?.Message ?? dbEx.Message}", "Error BD"); 
            }
            catch (Exception ex) { 
           
                MessageBox.Show($"Error inesperado al guardar: {ex.Message}", "Error"); 
            }
        }



        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false; 
            RequestClose?.Invoke(); 
        }

        [RelayCommand]
        private void SelectImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar Imagen del Producto",
                Filter = "Archivos de Imagen|*.jpg;*.webp;*.jpeg;*.png;*.gif;*.bmp|Todos los Archivos|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _selectedImageFullPath = openFileDialog.FileName;

                    // Carga la imagen para previsualización
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedImageFullPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; 
                    bitmap.EndInit();
                    SelectedImagePreview = bitmap; // Actualiza la propiedad 

                    // Guarda solo el nombre del archivo para la BD
                    _imageFileNameToSave = Path.GetFileName(_selectedImageFullPath);

                    Debug.WriteLine($"Imagen seleccionada: {_selectedImageFullPath}");
                    Debug.WriteLine($"Nombre archivo a guardar: {_imageFileNameToSave}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al cargar imagen seleccionada: {ex.Message}");
                    MessageBox.Show($"Error al cargar la imagen:\n{ex.Message}", "Error de Imagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    _selectedImageFullPath = null;
                    SelectedImagePreview = null;
                    _imageFileNameToSave = null;
                }
            }
        }

    }
}





