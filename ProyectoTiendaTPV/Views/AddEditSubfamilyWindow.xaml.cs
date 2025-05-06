using ProyectoTiendaTPV.ViewModels;
using ProyectoTiendaTPV.Models; 
using System.Windows;
using System;

namespace ProyectoTiendaTPV.Views
{
    public partial class AddEditSubfamilyWindow : Window
    {
        public AddEditSubfamilyViewModel ViewModel => (AddEditSubfamilyViewModel)DataContext;

        // Constructor para Añadir
        public AddEditSubfamilyWindow()
        {
            InitializeComponent();
            DataContext = new AddEditSubfamilyViewModel();
            SetupCloseAction();
        }

        // Constructor para Editar
        public AddEditSubfamilyWindow(Subfamily subfamilyToEdit)
        {
            InitializeComponent();
            DataContext = new AddEditSubfamilyViewModel(subfamilyToEdit);
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
