using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProyectoTiendaTPV.Models; 
using ProyectoTiendaTPV.Enums;  
using System;
using System.Threading.Tasks;

namespace ProyectoTiendaTPV.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // --- ViewModels Secundarios ---
        public LoginViewModel LoginVM { get; }
        public SalesViewModel SalesVM { get; }
        public ProductManagementViewModel ProductManagementVM { get; }

        // --- Estado de Autenticación ---
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAdmin))] // Notifica que IsAdmin puede cambiar
        [NotifyPropertyChangedFor(nameof(IsNotLoggedIn))] // Notifica que IsNotLoggedIn puede cambiar
        [NotifyPropertyChangedFor(nameof(IsLoggedIn))]

        private User? _currentUser;

        public bool IsLoggedIn => CurrentUser != null;
        public bool IsNotLoggedIn => CurrentUser == null; // Para visibilidad del LoginView
        public bool IsAdmin => CurrentUser?.Role == UserRole.Administrator;

        // --- ViewModel de Contenido Principal ---
        [ObservableProperty]
        private ObservableObject? _currentContentViewModel; // Ventas o Admin

        // --- Constructor ---
        public MainViewModel()
        {
            
            LoginVM = new LoginViewModel();
            SalesVM = new SalesViewModel();
            ProductManagementVM = new ProductManagementViewModel();

            
            LoginVM.LoginSuccess += OnLoginSuccess;

            // Estado inicial: No logueado, sin vista de contenido principal
            CurrentContentViewModel = null; 
            CurrentUser = null; 
        }

        // --- Manejador del Evento de Login Exitoso ---
        private void OnLoginSuccess(object? sender, User authenticatedUser)
        {
            CurrentUser = authenticatedUser; // Guarda el usuario
            // Notificar cambios en propiedades calculadas
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(IsNotLoggedIn));
            OnPropertyChanged(nameof(IsAdmin));

            
            NavigateToSalesCommand.NotifyCanExecuteChanged();
            NavigateToAdminCommand.NotifyCanExecuteChanged();
            LogoutCommand.NotifyCanExecuteChanged(); 
                                                     

            CurrentContentViewModel = SalesVM;
            _ = SalesVM.LoadInitialDataAsync();
        }

        // --- Comandos de Navegación ---

        // Habilitado si está logueado
        [RelayCommand(CanExecute = nameof(CanNavigateToContent))]
        private async Task NavigateToSales()
        {
            if (CurrentContentViewModel != SalesVM) // Si no estamos ya en Ventas
            {
                CurrentContentViewModel = SalesVM; // Cambia a la vista de Ventas
                                                   // Y luegi recarga sus datos iniciales
                await SalesVM.LoadInitialDataAsync(); 
            }
            else
            {
                await SalesVM.LoadInitialDataAsync();
            }
        }

        // Habilitado si está logueado Y es Admin
        [RelayCommand(CanExecute = nameof(CanNavigateToAdmin))]
        private async Task NavigateToAdmin() 
        {
            if (CurrentContentViewModel != ProductManagementVM) // Si no estamos ya en Admin
            {
                CurrentContentViewModel = ProductManagementVM; // Cambia a la vista de Admin
                // Y luwgo recarga sus datos iniciales
                await ProductManagementVM.LoadAllDataAsync(); 
            }
            else
            {

                await ProductManagementVM.LoadAllDataAsync();
            }
        }

        // --- Lógica para Navegación ---
        private bool CanNavigateToContent() => IsLoggedIn;
        private bool CanNavigateToAdmin() => IsLoggedIn && IsAdmin;

        // --- Comando de Logout ---
        [RelayCommand(CanExecute = nameof(CanLogout))]
        private void Logout()
        {
            CurrentUser = null; // Borrar usuario
            // Notificar cambios
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(IsNotLoggedIn));
            OnPropertyChanged(nameof(IsAdmin));

            
            NavigateToSalesCommand.NotifyCanExecuteChanged();
            NavigateToAdminCommand.NotifyCanExecuteChanged();
            LogoutCommand.NotifyCanExecuteChanged();
            

            CurrentContentViewModel = null; // Ocultar vista de contenido
            LoginVM.ClearCredentials(); // Limpiar campos del login viewmodel
        }
        private bool CanLogout() => IsLoggedIn;

    }
}