using ProyectoTiendaTPV.ViewModels;
using ProyectoTiendaTPV.Models; 
using System.Windows;
using System; 


namespace ProyectoTiendaTPV.Views
{
    public partial class AddEditFamilyWindow : Window
    {
        public AddEditFamilyViewModel ViewModel => (AddEditFamilyViewModel)DataContext;

        // Constructor para Añadir
        public AddEditFamilyWindow()
        {
            InitializeComponent();
            DataContext = new AddEditFamilyViewModel();
            SetupCloseAction();
        }

        // Constructor para Editar
        public AddEditFamilyWindow(Family familyToEdit) 
        {
            InitializeComponent();
            DataContext = new AddEditFamilyViewModel(familyToEdit); // Pasa el objeto a editar
            SetupCloseAction();
        }

        private void SetupCloseAction()
        {
            ViewModel.RequestClose += () =>
            {
                try { this.DialogResult = ViewModel.DialogResult; }
                catch (InvalidOperationException) { this.Close(); }
            };
        }
    }
}