using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Views
{
    public partial class VisitsView : UserControl
    {
        public Action? BackToMainMenu;

        public VisitsView()
        {
            InitializeComponent();
            VisitDatePicker.SelectedDate = DateTime.Today;
            VisitTimeInput.Text = DateTime.Now.ToString("HH:mm");
            RefreshMedicationDropdown();
            RefreshVisitsList();
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            string chipNumber = AnimalChipInput.Text?.Trim() ?? "";
            string timeText = VisitTimeInput.Text?.Trim() ?? "";
            string reason = ReasonInput.Text?.Trim() ?? "";
            string symptoms = SymptomsInput.Text?.Trim() ?? "";
            string diagnosis = DiagnosisInput.Text?.Trim() ?? "";
            string veterinarianName = VeterinarianInput.Text?.Trim() ?? "";
            string baseCostText = BaseCostInput.Text?.Trim() ?? "";
            string medicationQuantityText = MedicationQuantityInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(chipNumber) &&
                string.IsNullOrWhiteSpace(reason) &&
                string.IsNullOrWhiteSpace(symptoms) &&
                string.IsNullOrWhiteSpace(diagnosis) &&
                string.IsNullOrWhiteSpace(veterinarianName) &&
                string.IsNullOrWhiteSpace(baseCostText) &&
                string.IsNullOrWhiteSpace(medicationQuantityText))
            {
                ValidationText.Text = "";
                return;
            }

            if (!string.IsNullOrWhiteSpace(chipNumber) && !ValidationService.IsValidChipNumber(chipNumber))
            {
                SetValidationMessage("מספר שבב חייב להכיל ספרות בלבד", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(timeText) &&
                !TimeSpan.TryParse(timeText, CultureInfo.InvariantCulture, out _))
            {
                SetValidationMessage("שעה חייבת להיות בפורמט תקין, לדוגמה 14:30", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(baseCostText))
            {
                if (!double.TryParse(baseCostText, NumberStyles.Number, CultureInfo.InvariantCulture, out double baseCost))
                {
                    SetValidationMessage("מחיר ביקור חייב להיות מספר", isValid: false);
                    return;
                }

                if (!ValidationService.IsValidMoney(baseCost))
                {
                    SetValidationMessage("מחיר ביקור לא יכול להיות שלילי", isValid: false);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(medicationQuantityText))
            {
                if (!int.TryParse(medicationQuantityText, out int medicationQuantity))
                {
                    SetValidationMessage("כמות תרופה חייבת להיות מספר שלם", isValid: false);
                    return;
                }

                if (medicationQuantity < 0)
                {
                    SetValidationMessage("כמות תרופה לא יכולה להיות שלילית", isValid: false);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(reason) ||
                string.IsNullOrWhiteSpace(symptoms) ||
                string.IsNullOrWhiteSpace(diagnosis) ||
                string.IsNullOrWhiteSpace(veterinarianName))
            {
                SetValidationMessage("יש למלא סיבת הגעה, סימפטומים, אבחנה ושם וטרינר", isValid: false);
                return;
            }

            SetValidationMessage("הפרטים נראים תקינים", isValid: true);
        }

        private void SearchAnimal_Click(object? sender, RoutedEventArgs e)
        {
            string search = AnimalSearchInput.Text?.Trim() ?? "";

            var animal = AppData.Animals.FirstOrDefault(a =>
                a.ChipNumber == search || a.Name == search);

            if (animal == null)
            {
                UIHelper.ShowMessage(this, "לא נמצאה חיה לפי שם או מספר שבב");
                return;
            }

            AnimalChipInput.Text = animal.ChipNumber;

            if (ValidationService.IsVaccinationDue(animal.LastVaccinationDate))
            {
                UIHelper.ShowMessage(this, "תזכורת: החיה צריכה חיסון שנתי");
            }
        }

        private void CalculateCost_Click(object? sender, RoutedEventArgs e)
        {
            if (TryCalculateTotalCost(out double totalCost, out _, out _))
                TotalCostText.Text = $"עלות כוללת: {totalCost:0.00}";
        }

        private void SaveVisit_Click(object? sender, RoutedEventArgs e)
        {
            string chipNumber = AnimalChipInput.Text?.Trim() ?? "";
            string reason = ReasonInput.Text?.Trim() ?? "";
            string symptoms = SymptomsInput.Text?.Trim() ?? "";
            string diagnosis = DiagnosisInput.Text?.Trim() ?? "";
            string veterinarianName = VeterinarianInput.Text?.Trim() ?? "";

            var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == chipNumber);

            if (animal == null)
            {
                UIHelper.ShowMessage(this, "יש לבחור חיה קיימת לפני שמירת ביקור");
                return;
            }

            if (!TryGetVisitDate(out DateTime visitDate))
                return;

            if (!ValidationService.IsValidVisitDate(visitDate))
            {
                UIHelper.ShowMessage(this, "תאריך ביקור לא יכול להיות עתידי");
                return;
            }

            if (!ValidationService.IsRequiredText(reason) ||
                !ValidationService.IsRequiredText(symptoms) ||
                !ValidationService.IsRequiredText(diagnosis) ||
                !ValidationService.IsRequiredText(veterinarianName))
            {
                UIHelper.ShowMessage(this, "יש למלא סיבת הגעה, סימפטומים, אבחנה ושם וטרינר");
                return;
            }

            if (!TryCalculateTotalCost(out double totalCost, out Medication? medication, out int medicationQuantity))
                return;

            if (medication != null && medicationQuantity > 0)
            {
                medication.StockQuantity -= medicationQuantity;
                AppData.SaveMedicationsToDatabase();
            }

            AppData.Visits.Add(new Visit
            {
                AnimalChipNumber = chipNumber,
                VisitDate = visitDate,
                Reason = reason,
                Symptoms = symptoms,
                Diagnosis = diagnosis,
                VeterinarianName = veterinarianName,
                BaseCost = double.Parse(BaseCostInput.Text?.Trim() ?? "0", CultureInfo.InvariantCulture),
                MedicationName = medication?.Name ?? "",
                MedicationQuantity = medicationQuantity,
                TotalCost = totalCost
            });

            AppData.SaveVisitsToDatabase();

            UIHelper.ShowMessage(this, "הביקור נשמר בהצלחה");
            ClearFields();
            RefreshMedicationDropdown();
            RefreshVisitsList();
        }

        private bool TryGetVisitDate(out DateTime visitDate)
        {
            DateTime selectedDate = VisitDatePicker.SelectedDate?.DateTime ?? DateTime.Today;
            string timeText = VisitTimeInput.Text?.Trim() ?? "";

            if (!TimeSpan.TryParse(timeText, CultureInfo.InvariantCulture, out TimeSpan time))
            {
                UIHelper.ShowMessage(this, "שעה חייבת להיות בפורמט תקין, לדוגמה 14:30");
                visitDate = selectedDate;
                return false;
            }

            visitDate = selectedDate.Date.Add(time);
            return true;
        }

        private bool TryCalculateTotalCost(
            out double totalCost,
            out Medication? selectedMedication,
            out int medicationQuantity)
        {
            totalCost = 0;
            selectedMedication = null;
            medicationQuantity = 0;

            string baseCostText = BaseCostInput.Text?.Trim() ?? "";

            if (!double.TryParse(baseCostText, NumberStyles.Number, CultureInfo.InvariantCulture, out double baseCost))
            {
                UIHelper.ShowMessage(this, "מחיר ביקור חייב להיות מספר");
                return false;
            }

            if (!ValidationService.IsValidMoney(baseCost))
            {
                UIHelper.ShowMessage(this, "מחיר ביקור לא יכול להיות שלילי");
                return false;
            }

            totalCost = baseCost;

            string medicationName = MedicationDropdown.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(medicationName) || medicationName == "ללא תרופה")
                return true;

            selectedMedication = AppData.Medications.FirstOrDefault(m => m.Name == medicationName);

            if (selectedMedication == null)
            {
                UIHelper.ShowMessage(this, "התרופה שנבחרה לא קיימת במלאי");
                return false;
            }

            if (!int.TryParse(MedicationQuantityInput.Text?.Trim(), out medicationQuantity))
            {
                UIHelper.ShowMessage(this, "כמות תרופה חייבת להיות מספר שלם");
                return false;
            }

            if (medicationQuantity <= 0)
            {
                UIHelper.ShowMessage(this, "כמות תרופה חייבת להיות גדולה מאפס");
                return false;
            }

            if (medicationQuantity > selectedMedication.StockQuantity)
            {
                UIHelper.ShowMessage(this, "אין מספיק מלאי לתרופה שנבחרה");
                return false;
            }

            totalCost += selectedMedication.UnitPrice * medicationQuantity;
            return true;
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
            AnimalSearchInput.Text = "";
            AnimalChipInput.Text = "";
            VisitDatePicker.SelectedDate = DateTime.Today;
            VisitTimeInput.Text = DateTime.Now.ToString("HH:mm");
            ReasonInput.Text = "";
            SymptomsInput.Text = "";
            DiagnosisInput.Text = "";
            VeterinarianInput.Text = "";
            BaseCostInput.Text = "";
            MedicationQuantityInput.Text = "";
            MedicationDropdown.SelectedIndex = 0;
            TotalCostText.Text = "עלות כוללת: 0";
            ValidationText.Text = "";
        }

        private void SetValidationMessage(string message, bool isValid)
        {
            ValidationText.Foreground = isValid
                ? Avalonia.Media.Brushes.ForestGreen
                : Avalonia.Media.Brushes.Firebrick;
            ValidationText.Text = message;
        }

        private void RefreshMedicationDropdown()
        {
            var items = new List<string> { "ללא תרופה" };
            items.AddRange(AppData.Medications.Select(m => m.Name));

            MedicationDropdown.ItemsSource = items;
            MedicationDropdown.SelectedIndex = 0;
        }

        private void RefreshVisitsList()
        {
            if (AppData.Visits.Count == 0)
            {
                VisitsTextBlock.Text = "אין ביקורים במערכת";
                return;
            }

            string text = "";

            foreach (var visit in AppData.Visits.OrderByDescending(v => v.VisitDate))
            {
                var animal = AppData.Animals.FirstOrDefault(a => a.ChipNumber == visit.AnimalChipNumber);
                var owner = animal == null
                    ? null
                    : AppData.Clients.FirstOrDefault(c => c.IdNumber == animal.OwnerIdNumber);

                text += $"תאריך: {visit.VisitDate:dd/MM/yyyy HH:mm}\n";
                text += $"חיה: {(animal != null ? animal.Name : visit.AnimalChipNumber)}\n";
                text += $"בעלים: {(owner != null ? owner.FullName : "לא נמצא")}\n";
                text += $"סיבת הגעה: {visit.Reason}\n";
                text += $"סימפטומים: {visit.Symptoms}\n";
                text += $"אבחנה: {visit.Diagnosis}\n";
                text += $"וטרינר מטפל: {visit.VeterinarianName}\n";

                if (!string.IsNullOrWhiteSpace(visit.MedicationName))
                {
                    text += $"תרופה: {visit.MedicationName} x {visit.MedicationQuantity}\n";
                }

                text += $"עלות כוללת: {visit.TotalCost:0.00}\n";
                text += "-----------------------------\n";
            }

            VisitsTextBlock.Text = text;
        }
    }
}
