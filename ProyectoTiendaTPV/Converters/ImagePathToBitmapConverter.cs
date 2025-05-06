using System;
using System.Globalization;
using System.IO; 
using System.Windows.Data; 
using System.Windows.Media; 
using System.Windows.Media.Imaging; 

namespace ProyectoTiendaTPV.Converters
{
    public class ImagePathToBitmapConverter : IValueConverter
    {
        // Directorio base donde se esperan las imágenes 
        private static string _imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // El valor de entrada es la propiedad ImagePath 
            string? imageName = value as string;

            // Si el nombre es nulo o vacío, no hay imagen que mostrar
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return null; 
            }

            // Construye la ruta completa al archivo de imagen
            string fullPath = Path.Combine(_imagesDirectory, imageName);

            // Comprueba si el archivo existe antes de intentar cargarlo
            if (!File.Exists(fullPath))
            {
                System.Diagnostics.Debug.WriteLine($"WARN: Imagen no encontrada en: {fullPath}");
                return null; 
            }

            try
            {
                // Crea y carga el BitmapImage
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze(); 

                return bitmap;
            }
            catch (Exception ex)
            {
                // Log del error si la carga falla 
                System.Diagnostics.Debug.WriteLine($"ERROR: No se pudo cargar la imagen '{fullPath}'. Ex: {ex.Message}");
                return null; 
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {       
            throw new NotImplementedException();
        }
    }
}