using System;
using System.Globalization;
using System.Windows.Data;

namespace ProyectoTiendaTPV.Converters 
{
    public class StockToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Convierte la cantidad de stock (int) a un booleano (IsEnabled).
        /// </summary>
        /// <param name="value">El valor de StockQuantity (espera un int).</param>
        /// <param name="targetType">El tipo de destino (bool).</param>
        /// <param name="parameter">Parámetro opcional (no usado).</param>
        /// <param name="culture">Cultura.</param>
        /// <returns>True si el stock es mayor que 0, False en caso contrario.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                return stock > 0; // Habilita el botón si el stock es positivo
            }
            // Si el valor no es un entero,
            // por defecto lo consideramos como no habilitado.
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}