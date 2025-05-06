using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProyectoTiendaTPV.Data;
using ProyectoTiendaTPV.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore; 

namespace ProyectoTiendaTPV.ViewModels
{
    public partial class AddEditFamilyViewModel : ObservableValidator
    {
        [ObservableProperty]
        private string _windowTitle = "Añadir Familia";

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100)]
        private string? _familyName;

        [ObservableProperty]
        private string? _errorMessage;

        private readonly int? _familyIdToEdit = null;
        public bool DialogResult { get; private set; } = false;
        public event Action? RequestClose;

        // Constructor para Añadir
        public AddEditFamilyViewModel() { ValidateAllProperties(); }

        // Constructor para Editar
        public AddEditFamilyViewModel(Family familyToEdit) : this()
        {
            WindowTitle = "Editar Familia";
            _familyIdToEdit = familyToEdit.Id;
            FamilyName = familyToEdit.Name;
            ValidateAllProperties();
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            ErrorMessage = null;
            ValidateAllProperties();
            if (HasErrors) return;

            try
            {
                using (var context = new AppDbContext())
                {
                    if (_familyIdToEdit.HasValue) // Editar
                    {
                        var family = await context.Families.FindAsync(_familyIdToEdit.Value);
                        if (family != null)
                        {
                            family.Name = FamilyName!;
                            context.Families.Update(family);
                            await context.SaveChangesAsync();
                            MessageBox.Show("Familia actualizada.", "Éxito");
                        }
                        else { ErrorMessage = "Error: Familia no encontrada."; return; }
                    }
                    else // Añadir
                    {
                        var newFamily = new Family { Name = FamilyName! };
                        context.Families.Add(newFamily);
                        await context.SaveChangesAsync();
                        MessageBox.Show("Familia añadida.", "Éxito");
                    }
                }
                DialogResult = true;
                RequestClose?.Invoke();
            }
            catch (DbUpdateException dbEx) { ErrorMessage = $"Error BD: {dbEx.InnerException?.Message ?? dbEx.Message}"; }
            catch (Exception ex) { ErrorMessage = $"Error: {ex.Message}"; }
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = false;
            RequestClose?.Invoke();
        }
    }
}