using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Views
{
    public partial class MedicationsView : UserControl
    {
        public Action? BackToMainMenu;
        private bool isEditingMedication;
        private bool isClearingMedicationFields;

        public MedicationsView()
        {
            InitializeComponent();
            ExpirationDatePicker.SelectedDate = DateTime.Today.AddMonths(6);
            RefreshMedicationSelector();
            RefreshMedicationsList();
        }

        private void StartAddMedication_Click(object? sender, RoutedEventArgs e)
        {
            isEditingMedication = false;
            ClearFields();
            ShowMedicationForm(
                "הוספת תרופה חדשה",
                "מלא את פרטי התרופה ואז לחץ שמור תרופה");
            UpdateMedicationActionMode();
        }

        private void CloseMedicationForm_Click(object? sender, RoutedEventArgs e)
        {
            HideMedicationForm();
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
            if (isEditingMedication)
            {
                UpdateMedication();
                return;
            }

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
            HideMedicationForm();
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
            UpdateMedication();
        }

        private void UpdateMedication()
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
            ShowMedicationForm(
                "עריכת תרופה קיימת",
                "הנתונים עודכנו. אפשר להמשיך לערוך או לסגור את הפרטים");
            isEditingMedication = true;
            UpdateMedicationActionMode();
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

            bool medicationUsedInVisit = AppData.Visits.Any(visit =>
                visit.MedicationName == medication.Name &&
                visit.MedicationQuantity > 0);

            if (medicationUsedInVisit)
            {
                UIHelper.ShowMessage(
                    this,
                    "לא ניתן למחוק תרופה שמשויכת לביקור או תור קיים. אפשר לעדכן מלאי/מחיר, או להסיר את התרופה מהביקור קודם.");
                return;
            }

            AppData.Medications.Remove(medication);
            AppData.SaveMedicationsToDatabase();

            UIHelper.ShowMessage(this, "התרופה נמחקה בהצלחה");
            ClearFields();
            HideMedicationForm();
            RefreshMedicationSelector();
            RefreshMedicationsList();
        }

        private void MedicationSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (isClearingMedicationFields)
                return;

            var medication = FindSelectedMedication();

            if (medication != null)
            {
                FillMedicationFields(medication);
                return;
            }

            ResetMedicationFieldsForNewEntry();
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
            ResetMedicationFieldsForNewEntry();
        }

        private void ResetMedicationFieldsForNewEntry()
        {
            isClearingMedicationFields = true;
            isEditingMedication = false;
            NameInput.Text = "";
            StockInput.Text = "";
            UnitPriceInput.Text = "";
            NotesInput.Text = "";
            ExpirationDatePicker.SelectedDate = DateTime.Today.AddMonths(6);
            MedicationSelector.SelectedIndex = 0;
            ValidationText.Text = "";
            isClearingMedicationFields = false;
            UpdateMedicationActionMode();
        }

        private void ShowMedicationForm(string title, string hint)
        {
            MedicationStartPanel.IsVisible = false;
            MedicationFormPanel.IsVisible = true;
            MedicationFormTitle.Text = title;
            MedicationFormHint.Text = hint;
        }

        private void HideMedicationForm()
        {
            isEditingMedication = false;
            MedicationFormPanel.IsVisible = false;
            MedicationStartPanel.IsVisible = true;
            ValidationText.Text = "";
            UpdateMedicationActionMode();
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
            isEditingMedication = true;
            ShowMedicationForm(
                "עריכת תרופה קיימת",
                "הנתונים נטענו מהכרטיס. אפשר לעדכן או למחוק");

            NameInput.Text = medication.Name;
            StockInput.Text = medication.StockQuantity.ToString();
            UnitPriceInput.Text = medication.UnitPrice.ToString(CultureInfo.InvariantCulture);
            ExpirationDatePicker.SelectedDate = medication.ExpirationDate;
            NotesInput.Text = medication.Notes;
            SetValidationMessage("התרופה נטענה ואפשר לעדכן אותה", isValid: true);
            UpdateMedicationActionMode();
        }

        private void UpdateMedicationActionMode()
        {
            if (SaveMedicationButton == null || DeleteMedicationButton == null)
                return;

            SaveMedicationButton.Content = isEditingMedication ? "עדכן תרופה" : "שמור תרופה";
            ToolTip.SetTip(
                SaveMedicationButton,
                isEditingMedication ? "מעדכן את התרופה שנבחרה" : "שומר תרופה חדשה");

            DeleteMedicationButton.IsVisible = isEditingMedication;
        }

        private void RefreshMedicationSelector(string selectedMedicationName = "")
        {
            var medicationNames = AppData.Medications
                .OrderBy(m => m.Name)
                .Select(m => m.Name)
                .ToList();

            medicationNames.Insert(0, "בחר תרופה קיימת");
            isClearingMedicationFields = true;
            MedicationSelector.ItemsSource = medicationNames;

            int selectedIndex = string.IsNullOrWhiteSpace(selectedMedicationName)
                ? 0
                : medicationNames.FindIndex(name =>
                    string.Equals(name, selectedMedicationName, StringComparison.OrdinalIgnoreCase));

            MedicationSelector.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
            isClearingMedicationFields = false;
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
                "מלאי נמוך בלבד" => medication.IsLowStock,
                "תוקף קרוב בלבד" => medication.IsExpiringSoon,
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
            string statusText = GetMedicationStatusText(medication);
            string statusColor = GetMedicationStatusColor(medication);

            var card = new Button
            {
                Width = 220,
                MinHeight = 245,
                Margin = new Avalonia.Thickness(8),
                Padding = new Avalonia.Thickness(0),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse(statusColor)),
                BorderThickness = new Avalonia.Thickness(3),
                Foreground = new SolidColorBrush(Color.Parse("#2D3748")),
                Content = new StackPanel
                {
                    Spacing = 0,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children =
                    {
                        new Border
                        {
                            Height = 86,
                            Width = 214,
                            CornerRadius = new Avalonia.CornerRadius(14, 14, 26, 26),
                            Background = new SolidColorBrush(Color.Parse("#E9F8FC")),
                            Child = new Grid
                            {
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = "💊",
                                        FontSize = 42,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        TextAlignment = TextAlignment.Center
                                    },
                                    new Border
                                    {
                                        Background = new SolidColorBrush(Color.Parse(statusColor)),
                                        CornerRadius = new Avalonia.CornerRadius(12),
                                        Padding = new Avalonia.Thickness(10, 4),
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Top,
                                        Margin = new Avalonia.Thickness(10),
                                        Child = new TextBlock
                                        {
                                            Text = statusText,
                                            FontSize = 12,
                                            FontWeight = FontWeight.Bold,
                                            Foreground = Brushes.White,
                                            TextAlignment = TextAlignment.Center
                                        }
                                    }
                                }
                            }
                        },
                        new StackPanel
                        {
                            Spacing = 7,
                            Margin = new Avalonia.Thickness(14),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = medication.Name,
                                    FontSize = 21,
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
                                    Text = $"מחיר ליחידה: {medication.UnitPrice:0.00}",
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
                                    Text = "לחץ לפתיחת תרופה",
                                    FontSize = 12,
                                    FontWeight = FontWeight.Bold,
                                    TextAlignment = TextAlignment.Center,
                                    Foreground = new SolidColorBrush(Color.Parse(statusColor))
                                }
                            }
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

        private string GetMedicationStatusText(Medication medication)
        {
            if (medication.IsLowStock && medication.IsExpiringSoon)
                return "מלאי + תוקף";

            if (medication.IsLowStock)
                return "מלאי נמוך";

            if (medication.IsExpiringSoon)
                return "תוקף קרוב";

            return "תקין";
        }

        private string GetMedicationStatusColor(Medication medication)
        {
            if (medication.IsLowStock || medication.IsExpiringSoon)
                return "#D64545";

            return "#1E8F4D";
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
