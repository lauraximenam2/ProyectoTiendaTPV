using ProyectoTiendaTPV.ViewModels; 
using System.Windows;

namespace ProyectoTiendaTPV.Views
{
    /// <summary>
    /// Lógica de interacción para LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        // Expone el ViewModel para que App.xaml.cs pueda acceder al resultado
        public LoginViewModel ViewModel { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            ViewModel = new LoginViewModel(); // Crea instancia del ViewModel
            DataContext = ViewModel; // Establece DataContext


            {

                try
                {
                    //this.DialogResult = ViewModel.LoginSuccessful;
                }
                catch (InvalidOperationException)
                {
                    // Si no se mostró como diálogo, simplemente cierra
                    this.Close();
                }
            };

            Loaded += (sender, e) => UsernameTextBox.Focus(); 
        }
    }
}