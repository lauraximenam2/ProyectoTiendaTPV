using ProyectoTiendaTPV.ViewModels;
using System.Windows;

namespace ProyectoTiendaTPV
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(); 
        }
    }
}