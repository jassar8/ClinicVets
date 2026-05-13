using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Views
{
    public partial class AnimalsView : UserControl
    {
        public Action? BackToMainMenu;

        public AnimalsView()
        {
            InitializeComponent();
            RefreshAnimalsList();
        }

        private void ShowMessage(string message)
        {
            UIHelper.ShowMessage(this, message);
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";
            string weightText = WeightInput.Text?.Trim() ?? "";
            string ownerId = OwnerIdInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(chipNumber) &&
                string.IsNullOrWhiteSpace(weightText) &&
                string.IsNullOrWhiteSpace(ownerId))
            {
                ValidationText.Text = "";
                return;
            }

            if (!string.IsNullOrWhiteSpace(name) && !ValidationService.IsValidAnimalName(name))
            {
                SetValidationMessage("שם החיה חייב להכיל אותיות בלבד", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(chipNumber) && !ValidationService.IsValidChipNumber(chipNumber))
            {
                SetValidationMessage("מספר שבב חייב להכיל ספרות בלבד", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(weightText))
            {
                if (!double.TryParse(weightText, out double weight))
                {
                    SetValidationMessage("משקל חייב להיות מספר", isValid: false);
                    return;
                }

                if (!ValidationService.IsValidWeight(weight))
                {
                    SetValidationMessage("משקל חייב להיות בין 0.1 ל-100 קג", isValid: false);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(ownerId) && !ValidationService.IsValidIdNumber(ownerId))
            {
                SetValidationMessage("תעודת זהות בעלים חייבת להיות 9 ספרות", isValid: false);
                return;
            }

            SetValidationMessage("הפרטים נראים תקינים", isValid: true);
        }

        private void AddAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";
            string ownerId = OwnerIdInput.Text?.Trim() ?? "";
            string weightText = WeightInput.Text?.Trim() ?? "";

            string species = "כלב";

            if (SpeciesDropdown.SelectedItem is ComboBoxItem selectedSpecies &&
                selectedSpecies.Content != null)
            {
                species = selectedSpecies.Content.ToString() ?? "כלב";
            }

            if (!double.TryParse(weightText, out double weight))
            {
                ShowMessage("משקל חייב להיות מספר");
                return;
            }

            DateTime birthDate = BirthDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
            DateTime vaccinationDate = VaccinationDatePicker.SelectedDate?.DateTime ?? DateTime.Today;

            if (!ValidationService.IsValidAnimalName(name))
            {
                ShowMessage("שם החיה חייב להכיל אותיות בלבד");
                return;
            }

            if (!ValidationService.IsValidAnimalSpecies(species))
            {
                ShowMessage("סוג חיה לא תקין");
                return;
            }

            if (!ValidationService.IsValidChipNumber(chipNumber))
            {
                ShowMessage("מספר שבב חייב להכיל ספרות בלבד");
                return;
            }

            if (!ValidationService.IsValidWeight(weight))
            {
                ShowMessage("משקל חייב להיות בין 0.1 ל־100 קג");
                return;
            }

            if (!ValidationService.IsValidBirthDate(birthDate))
            {
                ShowMessage("תאריך לידה לא יכול להיות עתידי או לפני שנת 2000");
                return;
            }

            if (!ValidationService.IsValidIdNumber(ownerId))
            {
                ShowMessage("תעודת זהות בעלים חייבת להיות 9 ספרות");
                return;
            }

            if (!ValidationService.IsValidVaccinationDate(vaccinationDate))
            {
                ShowMessage("תאריך חיסון לא יכול להיות עתידי");
                return;
            }

            bool ownerExists = AppData.Clients.Any(client => client.IdNumber == ownerId);

            if (!ownerExists)
            {
                ShowMessage("לא נמצא לקוח עם תעודת זהות זו. קודם צריך להוסיף לקוח");
                return;
            }

            bool chipExists = AppData.Animals.Any(animal => animal.ChipNumber == chipNumber);

            if (chipExists)
            {
                ShowMessage("מספר שבב כבר קיים במערכת");
                return;
            }

            AppData.Animals.Add(new Animal
            {
                Name = name,
                Species = species,
                ChipNumber = chipNumber,
                Weight = weight,
                BirthDate = birthDate,
                OwnerIdNumber = ownerId,
                LastVaccinationDate = vaccinationDate
            });

            AppData.SaveAnimalsToDatabase();

            ShowMessage("החיה נוספה בהצלחה");
            ClearFields();
            RefreshAnimalsList();
        }

        private void SearchAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";

            var animal = AppData.Animals.FirstOrDefault(a =>
                a.Name == name || a.ChipNumber == chipNumber);

            if (animal == null)
            {
                ShowMessage("לא נמצאה חיה");
                return;
            }

            NameInput.Text = animal.Name;
            ChipNumberInput.Text = animal.ChipNumber;
            WeightInput.Text = animal.Weight.ToString();
            OwnerIdInput.Text = animal.OwnerIdNumber;
            BirthDatePicker.SelectedDate = animal.BirthDate;
            VaccinationDatePicker.SelectedDate = animal.LastVaccinationDate;

            for (int i = 0; i < SpeciesDropdown.ItemCount; i++)
            {
                if (SpeciesDropdown.Items[i] is ComboBoxItem item &&
                    item.Content?.ToString() == animal.Species)
                {
                    SpeciesDropdown.SelectedIndex = i;
                    break;
                }
            }
        }

        private void DeleteAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";

            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == chipNumber);

            if (animal == null)
            {
                ShowMessage("לא נמצאה חיה למחיקה");
                return;
            }

            AppData.Animals.Remove(animal);
            AppData.SaveAnimalsToDatabase();

            ShowMessage("החיה נמחקה בהצלחה");
            ClearFields();
            RefreshAnimalsList();
        }

        private void UpdateAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string chipNumber = ChipNumberInput.Text?.Trim() ?? "";
            string ownerId = OwnerIdInput.Text?.Trim() ?? "";
            string weightText = WeightInput.Text?.Trim() ?? "";

            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == chipNumber);

            if (animal == null)
            {
                ShowMessage("לא נמצאה חיה לעדכון לפי מספר השבב");
                return;
            }

            string species = "כלב";

            if (SpeciesDropdown.SelectedItem is ComboBoxItem selectedSpecies &&
                selectedSpecies.Content != null)
            {
                species = selectedSpecies.Content.ToString() ?? "כלב";
            }

            if (!double.TryParse(weightText, out double weight))
            {
                ShowMessage("משקל חייב להיות מספר");
                return;
            }

            DateTime birthDate = BirthDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
            DateTime vaccinationDate = VaccinationDatePicker.SelectedDate?.DateTime ?? DateTime.Today;

            if (!ValidationService.IsValidAnimalName(name))
            {
                ShowMessage("שם החיה חייב להכיל אותיות בלבד");
                return;
            }

            if (!ValidationService.IsValidAnimalSpecies(species))
            {
                ShowMessage("סוג חיה לא תקין");
                return;
            }

            if (!ValidationService.IsValidWeight(weight))
            {
                ShowMessage("משקל חייב להיות בין 0.1 ל־100 קג");
                return;
            }

            if (!ValidationService.IsValidBirthDate(birthDate))
            {
                ShowMessage("תאריך לידה לא יכול להיות עתידי או לפני שנת 2000");
                return;
            }

            if (!ValidationService.IsValidIdNumber(ownerId))
            {
                ShowMessage("תעודת זהות בעלים חייבת להיות 9 ספרות");
                return;
            }

            if (!ValidationService.IsValidVaccinationDate(vaccinationDate))
            {
                ShowMessage("תאריך חיסון לא יכול להיות עתידי");
                return;
            }

            bool ownerExists = AppData.Clients.Any(client => client.IdNumber == ownerId);

            if (!ownerExists)
            {
                ShowMessage("לא נמצא לקוח עם תעודת זהות זו");
                return;
            }

            animal.Name = name;
            animal.Species = species;
            animal.Weight = weight;
            animal.BirthDate = birthDate;
            animal.OwnerIdNumber = ownerId;
            animal.LastVaccinationDate = vaccinationDate;

            AppData.SaveAnimalsToDatabase();

            ShowMessage("פרטי החיה עודכנו בהצלחה");
            RefreshAnimalsList();
        }

        private void ClearFields_Click(object? sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            BackToMainMenu?.Invoke();
        }

        private void ClearFields()
        {
            NameInput.Text = "";
            ChipNumberInput.Text = "";
            WeightInput.Text = "";
            OwnerIdInput.Text = "";
            SpeciesDropdown.SelectedIndex = 0;
            BirthDatePicker.SelectedDate = DateTime.Today;
            VaccinationDatePicker.SelectedDate = DateTime.Today;
            ValidationText.Text = "";
        }

        private void SetValidationMessage(string message, bool isValid)
        {
            ValidationText.Foreground = isValid
                ? Avalonia.Media.Brushes.ForestGreen
                : Avalonia.Media.Brushes.Firebrick;
            ValidationText.Text = message;
        }

        private void RefreshAnimalsList()
        {
            if (AppData.Animals.Count == 0)
            {
                AnimalsTextBlock.Text = "אין בעלי חיים במערכת";
                return;
            }

            string text = "";

            foreach (var animal in AppData.Animals)
            {
                var owner = AppData.Clients.FirstOrDefault(c => c.IdNumber == animal.OwnerIdNumber);

                text += $"שם: {animal.Name}\n";
                text += $"סוג: {animal.Species}\n";
                text += $"שבב: {animal.ChipNumber}\n";
                text += $"משקל: {animal.Weight} קג\n";
                text += $"תאריך לידה: {animal.BirthDate:dd/MM/yyyy}\n";
                text += $"בעלים: {(owner != null ? owner.FullName : animal.OwnerIdNumber)}\n";
                text += $"חיסון אחרון: {animal.LastVaccinationDate:dd/MM/yyyy}\n";

                if (ValidationService.IsVaccinationDue(animal.LastVaccinationDate))
                {
                    text += "תזכורת: יש לקבוע חיסון שנתי\n";
                }

                text += "-----------------------------\n";
            }

            AnimalsTextBlock.Text = text;
        }
    }
}