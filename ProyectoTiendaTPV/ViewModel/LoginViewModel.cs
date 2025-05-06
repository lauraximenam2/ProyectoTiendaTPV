using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore; 
using ProyectoTiendaTPV.Data;
using ProyectoTiendaTPV.Models;
using ProyectoTiendaTPV.Enums; 
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; 

namespace ProyectoTiendaTPV.ViewModels 
{
    public partial class LoginViewModel : ObservableObject
    {
        // Propiedad para el nombre de usuario, enlazada desde LoginView.xaml
        [ObservableProperty]
        private string? _username;

        [ObservableProperty]
        private string? _errorMessage;

        // Evento que se disparará cuando el login sea exitos
        // El MainViewModel se suscribirá a este evento
        // Pasa el objeto User autenticado como argumento del evento
        public event EventHandler<User>? LoginSuccess;

        // Constructor
        public LoginViewModel()
        {
            
        }

        // Comando ejecutado por el botón "Entrar" en LoginView.xaml
        // Recibe el PasswordBox como parámetro para obtener la contraseña de forma segura
        [RelayCommand]
        private async Task Login(object? parameter)
        {
            ErrorMessage = null; 

            // Verifica y obtiene el PasswordBox 
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            if (passwordBox == null)
            {
                ErrorMessage = "Error interno del formulario.";
                Debug.WriteLine("LoginCommand: El parámetro no era un PasswordBox.");
                return;
            }
            string password = passwordBox.Password; // Extrae la contraseña como string

            // Validación básica de campos vacíos
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Debe ingresar usuario y contraseña.";
                return;
            }

            User? user = null; // Variable para guardar el usuario encontrado
            try
            {
                // Usa el DbContext para buscar al usuario
                using (var context = new AppDbContext())
                {
                    // Busca por nombre de usuario (insensible a mayúsculas/minúsculas y quitando espacios)
                    var tempUsername = Username?.Trim().ToLowerInvariant();
                    user = await context.Users
                                     .FirstOrDefaultAsync(u => u.Username.ToLower() == tempUsername);
                } 

                // Si no se encontró el usuario
                if (user == null)
                {
                    ErrorMessage = "Usuario o contraseña incorrectos.";
                    passwordBox.Clear(); // Limpiar contraseña
                    return;
                }

                // Si se encontró el usuario, verifica la contraseña usando BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (isPasswordValid)
                {
                    // Autenticación Exitosa
                    Debug.WriteLine($"Usuario '{user.Username}' autenticado con rol '{user.Role}'.");

                    // Dispara el evento LoginSuccess, pasando el objeto User autenticado
                    // El MainViewModel estará escuchando este evento
                    LoginSuccess?.Invoke(this, user);

                    // Limpiar los campos después de un login exitoso
                    Username = null;
                    passwordBox.Clear(); 
                    ErrorMessage = null; 
                }
                else
                {
                    // Contraseña incorrecta
                    ErrorMessage = "Usuario o contraseña incorrectos.";
                    passwordBox.Clear(); // Limpiar contraseña
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores inesperados 
                Debug.WriteLine($"Error durante el login: {ex}");
                ErrorMessage = $"Error inesperado: {ex.Message}";
                passwordBox?.Clear(); 
            }
            finally
            {
                // Asegurar que la variable local de contraseña se limpie
                password = string.Empty;
                
            }
        }

        // Método público para limpiar los campos visibles.
        public void ClearCredentials()
        {
            Username = null;
            ErrorMessage = null;

        }

    }
}