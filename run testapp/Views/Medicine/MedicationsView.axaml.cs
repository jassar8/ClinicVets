using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ClinicVetsAvalonia.Repositories;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Views.Medicine
{
    public partial class MedicationsView : UserControl
    {
        public Action? BackToMainMenu;

        public MedicationsView()
        {
            InitializeComponent();
            ExpirationDatePicker.SelectedDate = DateTime.Today.AddMonths(6);
            RefreshMedicationSelector();
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
            RefreshMedicationSelector();
            RefreshMedicationsList();
        }

        private void SearchMedication_Click(object? sender, RoutedEventArgs e)
        {
            var medication = FindSelectedOrTypedMedication();

            if (medication == null)
            {
                UIHelper.ShowMessage(this, "בחר תרופה מהרשימה או הקלד שם תרופה קיים");
                return;
            }

            FillMedicationFields(medication);
        }

        private void UpdateMedication_Click(object? sender, RoutedEventArgs e)
        {
            var medication = FindSelectedOrTypedMedication();

            if (medication == null)
            {
                UIHelper.ShowMessage(this, "בחר תרופה מהרשימה לפני עדכון");
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
            RefreshMedicationSelector();
            RefreshMedicationsList();
        }

        private void DeleteMedication_Click(object? sender, RoutedEventArgs e)
        {
            var medication = FindSelectedOrTypedMedication();

            if (medication == null)
            {
                UIHelper.ShowMessage(this, "בחר תרופה מהרשימה לפני מחיקה");
                return;
            }

            AppData.Medications.Remove(medication);
            AppData.SaveMedicationsToDatabase();

            UIHelper.ShowMessage(this, "התרופה נמחקה בהצלחה");
            ClearFields();
            RefreshMedicationSelector();
            RefreshMedicationsList();
        }

        private void MedicationSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var medication = FindSelectedMedication();

            if (medication != null)
            {
                FillMedicationFields(medication);
            }
        }

        private void MedicationSearch_Changed(object? sender, TextChangedEventArgs e)
        {
            RefreshMedicationsList();
        }

        private void MedicationFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
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

        private void ClearMedicationSearch_Click(object? sender, RoutedEventArgs e)
        {
            MedicationSearchInput.Text = "";
            MedicationFilterDropdown.SelectedIndex = 0;
            RefreshMedicationsList();
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
            MedicationSelector.SelectedIndex = 0;
            ValidationText.Text = "";
        }

        private void SetValidationMessage(string message, bool isValid)
        {
            ValidationText.Foreground = isValid
                ? Avalonia.Media.Brushes.ForestGreen
                : Avalonia.Media.Brushes.Firebrick;
            ValidationText.Text = message;
        }

        private Medication? FindSelectedMedication()
        {
            string selectedName = MedicationSelector.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(selectedName) || selectedName == "בחר תרופה קיימת")
                return null;

            return AppData.Medications.FirstOrDefault(m =>
                string.Equals(m.Name, selectedName, StringComparison.OrdinalIgnoreCase));
        }

        private Medication? FindSelectedOrTypedMedication()
        {
            var selectedMedication = FindSelectedMedication();

            if (selectedMedication != null)
                return selectedMedication;

            string typedName = NameInput.Text?.Trim() ?? "";

            return AppData.Medications.FirstOrDefault(m =>
                string.Equals(m.Name, typedName, StringComparison.OrdinalIgnoreCase));
        }

        private void FillMedicationFields(Medication medication)
        {
            NameInput.Text = medication.Name;
            StockInput.Text = medication.StockQuantity.ToString();
            UnitPriceInput.Text = medication.UnitPrice.ToString(CultureInfo.InvariantCulture);
            ExpirationDatePicker.SelectedDate = medication.ExpirationDate;
            NotesInput.Text = medication.Notes;
            SetValidationMessage("התרופה נטענה ואפשר לעדכן אותה", isValid: true);
        }

        private void RefreshMedicationSelector()
        {
            var medicationNames = AppData.Medications
                .OrderBy(m => m.Name)
                .Select(m => m.Name)
                .ToList();

            medicationNames.Insert(0, "בחר תרופה קיימת");
            MedicationSelector.ItemsSource = medicationNames;
            MedicationSelector.SelectedIndex = 0;
        }

        private void RefreshMedicationsList()
        {
            if (MedicationCardsPanel == null || MedicationResultsText == null)
                return;

            MedicationCardsPanel.Children.Clear();

            var filteredMedications = AppData.Medications
                .Where(MatchesSearchAndFilter)
                .OrderBy(m => m.Name)
                .ToList();

            MedicationResultsText.Text =
                $"מציג {filteredMedications.Count} מתוך {AppData.Medications.Count} תרופות";

            if (AppData.Medications.Count == 0)
            {
                MedicationCardsPanel.Children.Add(new TextBlock
                {
                    Text = "אין תרופות במערכת",
                    FontSize = 18,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Avalonia.Thickness(20)
                });
                return;
            }

            if (filteredMedications.Count == 0)
            {
                MedicationCardsPanel.Children.Add(new TextBlock
                {
                    Text = "לא נמצאו תרופות שמתאימות לחיפוש",
                    FontSize = 18,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Avalonia.Thickness(20)
                });
                return;
            }

            foreach (var medication in filteredMedications)
            {
                MedicationCardsPanel.Children.Add(CreateMedicationCard(medication));
            }
        }

        private bool MatchesSearchAndFilter(Medication medication)
        {
            string searchText = MedicationSearchInput?.Text?.Trim() ?? "";
            string filter = GetSelectedMedicationFilter();

            bool matchesSearch = string.IsNullOrWhiteSpace(searchText) ||
                                 medication.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);

            bool matchesFilter = filter switch
            {
                "מלאי נמוך" => medication.IsLowStock,
                "תוקף קרוב" => medication.IsExpiringSoon,
                _ => true
            };

            return matchesSearch && matchesFilter;
        }

        private string GetSelectedMedicationFilter()
        {
            if (MedicationFilterDropdown?.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Content != null)
            {
                return selectedItem.Content.ToString() ?? "הכל";
            }

            return "הכל";
        }

        private Button CreateMedicationCard(Medication medication)
        {
            string alertText = "";

            if (medication.IsLowStock)
                alertText += "מלאי נמוך  ";

            if (medication.IsExpiringSoon)
                alertText += "תוקף קרוב";

            var card = new Button
            {
                Width = 190,
                MinHeight = 210,
                Margin = new Avalonia.Thickness(8),
                Padding = new Avalonia.Thickness(12),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse("#69C5D8")),
                BorderThickness = new Avalonia.Thickness(2),
                Foreground = new SolidColorBrush(Color.Parse("#2D3748")),
                Content = new StackPanel
                {
                    Spacing = 6,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children =
                    {
                        new Border
                        {
                            Width = 68,
                            Height = 68,
                            CornerRadius = new Avalonia.CornerRadius(34),
                            Background = new SolidColorBrush(Color.Parse("#E9F8FC")),
                            Child = new TextBlock
                            {
                                Text = "💊",
                                FontSize = 34,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                TextAlignment = TextAlignment.Center
                            }
                        },
                        new TextBlock
                        {
                            Text = medication.Name,
                            FontSize = 18,
                            FontWeight = FontWeight.Bold,
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = new SolidColorBrush(Color.Parse("#2D3748"))
                        },
                        new TextBlock
                        {
                            Text = $"מלאי: {medication.StockQuantity}",
                            FontSize = 14,
                            TextAlignment = TextAlignment.Center,
                            Foreground = new SolidColorBrush(Color.Parse("#526172"))
                        },
                        new TextBlock
                        {
                            Text = $"מחיר: {medication.UnitPrice:0.00}",
                            FontSize = 14,
                            TextAlignment = TextAlignment.Center,
                            Foreground = new SolidColorBrush(Color.Parse("#526172"))
                        },
                        new TextBlock
                        {
                            Text = $"תוקף: {medication.ExpirationDate:dd/MM/yyyy}",
                            FontSize = 13,
                            TextAlignment = TextAlignment.Center,
                            Foreground = new SolidColorBrush(Color.Parse("#526172"))
                        },
                        new TextBlock
                        {
                            Text = alertText,
                            FontSize = 13,
                            FontWeight = FontWeight.Bold,
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = medication.IsLowStock || medication.IsExpiringSoon
                                ? Brushes.Firebrick
                                : Brushes.ForestGreen
                        }
                    }
                }
            };

            ToolTip.SetTip(card, "לחץ כדי לראות ולעדכן את התרופה");

            card.Click += (_, _) =>
            {
                SelectMedicationInDropdown(medication);
                FillMedicationFields(medication);
            };

            return card;
        }

        private void SelectMedicationInDropdown(Medication medication)
        {
            for (int i = 0; i < MedicationSelector.ItemCount; i++)
            {
                if (MedicationSelector.Items[i]?.ToString() == medication.Name)
                {
                    MedicationSelector.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
