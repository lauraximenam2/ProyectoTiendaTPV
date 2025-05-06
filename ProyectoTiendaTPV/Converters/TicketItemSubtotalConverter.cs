using ProyectoTiendaTPV.Models; // Para TicketItem
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProyectoTiendaTPV.Converters
{
    public class TicketItemSubtotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TicketItem item)
            {
                return item.Quantity * item.UnitPrice;
            }
            return 0m; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}