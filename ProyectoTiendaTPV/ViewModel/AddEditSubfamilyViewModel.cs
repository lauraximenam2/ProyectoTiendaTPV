using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProyectoTiendaTPV.Data;
using ProyectoTiendaTPV.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProyectoTiendaTPV.ViewModels
{
    public partial class AddEditSubfamilyViewModel : ObservableValidator
    {
        [ObservableProperty]
        private string _windowTitle = "Añadir Subfamilia";

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100)]
        private string? _subfamilyName;

        // Para el ComboBox de Familias
        [ObservableProperty]
        private ObservableCollection<Family> _families = new ObservableCollection<Family>();

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Debe seleccionar una familia padre")]
        private Family? _selectedFamily;

        [ObservableProperty]
        private string? _errorMessage;

        private readonly int? _subfamilyIdToEdit = null;
        private int? _familyIdToSelectInitially = null; // Para preseleccionar en modo Editar

        public bool DialogResult { get; private set; } = false;
        public event Action? RequestClose;

        // Constructor para Añadir
        public AddEditSubfamilyViewModel()
        {
            _ = LoadFamiliesAsync(); // Cargar familias para el ComboBox
            ValidateAllProperties();
        }

        // Constructor para Editar
        public AddEditSubfamilyViewModel(Subfamily subfamilyToEdit) : this()
        {
            WindowTitle = "Editar Subfamilia";
            _subfamilyIdToEdit = subfamilyToEdit.Id;
            SubfamilyName = subfamilyToEdit.Name;
            _familyIdToSelectInitially = subfamilyToEdit.FamilyId; // Guarda el ID para seleccionar después
            ValidateAllProperties();
        }

        private async Task LoadFamiliesAsync()
        {
            Families.Clear();
            try
            {
                using (var context = new AppDbContext())
                {
                    var familiesFromDb = await context.Families.OrderBy(f => f.Name).ToListAsync();
                    foreach (var family in familiesFromDb) Families.Add(family);
                }

                // Preseleccionar familia si estamos editando
                if (_familyIdToSelectInitially.HasValue)
                {
                    SelectedFamily = Families.FirstOrDefault(f => f.Id == _familyIdToSelectInitially.Value);
                }
            }
            catch (Exception ex) { ErrorMessage = $"Error al cargar familias: {ex.Message}"; }
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
                    if (_subfamilyIdToEdit.HasValue) // Editar
                    {
                        var subfamily = await context.Subfamilies.FindAsync(_subfamilyIdToEdit.Value);
                        if (subfamily != null)
                        {
                            subfamily.Name = SubfamilyName!;
                            subfamily.FamilyId = SelectedFamily!.Id; // Actualizar FK
                            context.Subfamilies.Update(subfamily);
                            await context.SaveChangesAsync();
                            MessageBox.Show("Subfamilia actualizada.", "Éxito");
                        }
                        else { ErrorMessage = "Error: Subfamilia no encontrada."; return; }
                    }
                    else // Añadir
                    {
                        var newSubfamily = new Subfamily
                        {
                            Name = SubfamilyName!,
                            FamilyId = SelectedFamily!.Id // Asignar FK
                        };
                        context.Subfamilies.Add(newSubfamily);
                        await context.SaveChangesAsync();
                        MessageBox.Show("Subfamilia añadida.", "Éxito");
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