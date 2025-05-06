using ProyectoTiendaTPV.ViewModels; 
using System.Windows;

namespace ProyectoTiendaTPV.Views
{
    /// <summary>
    /// Lógica de interacción para AddEditProductWindow.xaml
    /// </summary>
    public partial class AddEditProductWindow : Window
    {
        public AddEditProductWindow(AddEditProductViewModel viewModel) // Recibe el ViewModel en el constructor
        {
            InitializeComponent();
            DataContext = viewModel; // Establece el DataContext
      
            viewModel.RequestClose += () =>
            {
                // Establecer DialogResult basado en el ViewModel antes de cerrar
                try
                {
                    this.DialogResult = viewModel.DialogResult;
                }
                catch (InvalidOperationException)
                {
                    this.Close();
                }

            };
        }
    }
}