using System;
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
    public partial class MedicationsView : UserControl
    {
        public Action? BackToMainMenu;

        public MedicationsView()
        {
            InitializeComponent();
            ExpirationDatePicker.SelectedDate = DateTime.Today.AddMonths(6);
            RefreshMedicationsList();
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";
            string stockText = StockInput.Text?.Trim() ?? "";
            string unitPriceText = UnitPriceInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(stockText) &&
                string.IsNullOrWhiteSpace(unitPriceText))
            {
                ValidationText.Text = "";
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                SetValidationMessage("שם תרופה הוא שדה חובה", isValid: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(stockText))
            {
                if (!int.TryParse(stockText, out int stockQuantity))
                {
                    SetValidationMessage("כמות מלאי חייבת להיות מספר שלם", isValid: false);
                    return;
                }

                if (!ValidationService.IsValidStockQuantity(stockQuantity))
                {
                    SetValidationMessage("כמות מלאי לא יכולה להיות שלילית", isValid: false);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(unitPriceText))
            {
                if (!double.TryParse(unitPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out double unitPrice))
                {
                    SetValidationMessage("מחיר יחידה חייב להיות מספר", isValid: false);
                    return;
                }

                if (!ValidationService.IsValidMoney(unitPrice))
                {
                    SetValidationMessage("מחיר יחידה לא יכול להיות שלילי", isValid: false);
                    return;
                }
            }

            SetValidationMessage("הפרטים נראים תקינים", isValid: true);
        }

        private void AddMedication_Click(object? sender, RoutedEventArgs e)
        {
            if (!TryReadMedicationFields(out Medication medication))
                return;

            bool exists = AppData.Medications.Any(m =>
                string.Equals(m.Name, medication.Name, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                UIHelper.ShowMessage(this, "תרופה בשם זה כבר קיימת במערכת");
                return;
            }

            AppData.Medications.Add(medication);
            AppData.SaveMedicationsToDatabase();

            UIHelper.ShowMessage(this, "התרופה נוספה בהצלחה");
            ClearFields();
            RefreshMedicationsList();
        }

        private void SearchMedication_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";

            var medication = AppData.Medications.FirstOrDefault(m =>
                string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));

            if (medication == null)
            {
                UIHelper.ShowMessage(this, "לא נמצאה תרופה בשם זה");
                return;
            }

            NameInput.Text = medication.Name;
            StockInput.Text = medication.StockQuantity.ToString();
            UnitPriceInput.Text = medication.UnitPrice.ToString(CultureInfo.InvariantCulture);
            ExpirationDatePicker.SelectedDate = medication.ExpirationDate;
            NotesInput.Text = medication.Notes;
        }

        private void UpdateMedication_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";

            var medication = AppData.Medications.FirstOrDefault(m =>
                string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));

            if (medication == null)
            {
                UIHelper.ShowMessage(this, "לא נמצאה תרופה לעדכון");
                return;
            }

            if (!TryReadMedicationFields(out Medication updatedMedication))
                return;

            medication.StockQuantity = updatedMedication.StockQuantity;
            medication.UnitPrice = updatedMedication.UnitPrice;
            medication.ExpirationDate = updatedMedication.ExpirationDate;
            medication.Notes = updatedMedication.Notes;

            AppData.SaveMedicationsToDatabase();

            UIHelper.ShowMessage(this, "התרופה עודכנה בהצלחה");
            RefreshMedicationsList();
        }

        private void DeleteMedication_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameInput.Text?.Trim() ?? "";

            var medication = AppData.Medications.FirstOrDefault(m =>
                string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));

            if (medication == null)
            {
                UIHelper.ShowMessage(this, "לא נמצאה תרופה למחיקה");
                return;
            }

            AppData.Medications.Remove(medication);
            AppData.SaveMedicationsToDatabase();

            UIHelper.ShowMessage(this, "התרופה נמחקה בהצלחה");
            ClearFields();
            RefreshMedicationsList();
        }

        private bool TryReadMedicationFields(out Medication medication)
        {
            medication = new Medication();

            string name = NameInput.Text?.Trim() ?? "";
            string stockText = StockInput.Text?.Trim() ?? "";
            string unitPriceText = UnitPriceInput.Text?.Trim() ?? "";
            string notes = NotesInput.Text?.Trim() ?? "";
            DateTime expirationDate = ExpirationDatePicker.SelectedDate?.DateTime ?? DateTime.Today;

            if (!ValidationService.IsRequiredText(name))
            {
                UIHelper.ShowMessage(this, "שם תרופה הוא שדה חובה");
                return false;
            }

            if (!int.TryParse(stockText, out int stockQuantity))
            {
                UIHelper.ShowMessage(this, "כמות מלאי חייבת להיות מספר שלם");
                return false;
            }

            if (!ValidationService.IsValidStockQuantity(stockQuantity))
            {
                UIHelper.ShowMessage(this, "כמות מלאי לא יכולה להיות שלילית");
                return false;
            }

            if (!double.TryParse(unitPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out double unitPrice))
            {
                UIHelper.ShowMessage(this, "מחיר יחידה חייב להיות מספר");
                return false;
            }

            if (!ValidationService.IsValidMoney(unitPrice))
            {
                UIHelper.ShowMessage(this, "מחיר יחידה לא יכול להיות שלילי");
                return false;
            }

            if (!ValidationService.IsValidExpirationDate(expirationDate))
            {
                UIHelper.ShowMessage(this, "תאריך תפוגה לא יכול להיות בעבר");
                return false;
            }

            medication = new Medication
            {
                Name = name,
                StockQuantity = stockQuantity,
                UnitPrice = unitPrice,
                ExpirationDate = expirationDate,
                Notes = notes
            };

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
            NameInput.Text = "";
            StockInput.Text = "";
            UnitPriceInput.Text = "";
            NotesInput.Text = "";
            ExpirationDatePicker.SelectedDate = DateTime.Today.AddMonths(6);
            ValidationText.Text = "";
        }

        private void SetValidationMessage(string message, bool isValid)
        {
            ValidationText.Foreground = isValid
                ? Avalonia.Media.Brushes.ForestGreen
                : Avalonia.Media.Brushes.Firebrick;
            ValidationText.Text = message;
        }

        private void RefreshMedicationsList()
        {
            if (AppData.Medications.Count == 0)
            {
                MedicationsTextBlock.Text = "אין תרופות במערכת";
                return;
            }

            string text = "";

            foreach (var medication in AppData.Medications.OrderBy(m => m.Name))
            {
                text += $"שם תרופה: {medication.Name}\n";
                text += $"מלאי: {medication.StockQuantity}\n";
                text += $"מחיר ליחידה: {medication.UnitPrice:0.00}\n";
                text += $"תאריך תפוגה: {medication.ExpirationDate:dd/MM/yyyy}\n";

                if (!string.IsNullOrWhiteSpace(medication.Notes))
                {
                    text += $"הערות: {medication.Notes}\n";
                }

                if (medication.IsLowStock)
                {
                    text += "התראה: מלאי נמוך\n";
                }

                if (medication.IsExpiringSoon)
                {
                    text += "התראה: תאריך תפוגה קרוב\n";
                }

                text += "-----------------------------\n";
            }

            MedicationsTextBlock.Text = text;
        }
    }
}
