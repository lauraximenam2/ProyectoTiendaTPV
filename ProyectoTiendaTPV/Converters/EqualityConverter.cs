using System;
using System.Globalization;
using System.Linq; 
using System.Windows;
using System.Windows.Data; 

namespace ProyectoTiendaTPV.Converters
{
    /// <summary>
    /// Compara los múltiples valores proporcionados por un MultiBinding.
    /// Espera exactamente dos valores y devuelve true si son iguales.
    /// </summary>
    public class EqualityConverter : IMultiValueConverter 
    {
        /// <summary>
        /// Convierte los valores de origen para el MultiBinding.
        /// </summary>
        /// <param name="values">Array de valores producidos por los bindings de origen. Se esperan 2 valores.</param>
        /// <param name="targetType">El tipo de la propiedad de destino del enlace (normalmente bool).</param>
        /// <param name="parameter">El parámetro del convertidor a usar.</param>
        /// <param name="culture">La referencia cultural que se va a usar en el convertidor.</param>
        /// <returns>True si los dos primeros valores en 'values' son iguales, False en caso contrario.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
           
            if (values == null || values.Length < 2)
            {
                return false; 
            }
        
            object? value1 = values[0];
            object? value2 = values[1];

        
            return object.Equals(value1, value2);
        }

        /// <summary>
        /// Convierte el valor de destino de vuelta a los valores de origen.
        /// En este caso no es necesario para la funcionalidad de IsChecked.
        /// </summary>
        /// <param name="value">El valor producido por el destino del enlace.</param>
        /// <param name="targetTypes">Array de tipos a los que convertir.</param>
        /// <param name="parameter">El parámetro del convertidor a usar.</param>
        /// <param name="culture">La referencia cultural que se va a usar en el convertidor.</param>
        /// <returns>Un array de valores. Aquí devolvemos valores que indican que no se realiza ninguna conversión.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {

            return targetTypes.Select(t => Binding.DoNothing).ToArray();

        }
    }
}