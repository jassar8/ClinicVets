using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Helpers;
using ClinicVets.Desktop.Stability;

namespace ClinicVets.Desktop.Views;

public partial class MedicationsView : UserControl
{
    public Action? BackToMainMenu;
    private readonly List<Medication> _items = new();
    private bool _isEditing;
    private int _editingId;
    private bool _isClearingFields;

    public MedicationsView()
    {
        InitializeComponent();
        SafeViewLoader.RunSafe(this, () =>
            ExpirationDatePicker.SelectedDate = DateTime.Today.AddMonths(6), "Medications.Init");
        _ = SafeViewLoader.RunSafeAsync(this, LoadAsync, "Medications.Load");
    }

    private async Task LoadAsync()
    {
        _items.Clear();
        _items.AddRange(await AppServices.Medications.SearchAsync(
            MedicationSearchInput?.Text,
            MapFilterLabel()));
        RefreshMedicationSelector();
        RefreshMedicationsList();
    }

    private void StartAddMedication_Click(object? sender, RoutedEventArgs e)
    {
        _isEditing = false;
        _editingId = 0;
        ClearFields();
        ShowMedicationForm("הוספת תרופה חדשה", "מלא את פרטי התרופה ואז לחץ שמור תרופה");
        UpdateMedicationActionMode();
    }

    private void CloseMedicationForm_Click(object? sender, RoutedEventArgs e) => HideMedicationForm();

    private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e) => ValidateInputs(false);

    private void ValidateInputs(bool showEmpty)
    {
        var name = NameInput.Text?.Trim() ?? "";
        var stockText = StockInput.Text?.Trim() ?? "";
        var unitPriceText = UnitPriceInput.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(stockText) && string.IsNullOrWhiteSpace(unitPriceText))
        {
            ValidationText.Text = showEmpty ? "יש למלא פרטים" : "";
            return;
        }

        if (!UiFormValidation.IsRequiredText(name))
        {
            SetValidationMessage("שם תרופה הוא שדה חובה", false);
            return;
        }

        if (!string.IsNullOrWhiteSpace(stockText) && (!int.TryParse(stockText, out var stock) || !UiFormValidation.IsValidStockQuantity(stock)))
        {
            SetValidationMessage("כמות מלאי חייבת להיות מספר שלם", false);
            return;
        }

        if (!string.IsNullOrWhiteSpace(unitPriceText) &&
            (!double.TryParse(unitPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ||
             !UiFormValidation.IsValidMoney(price)))
        {
            SetValidationMessage("מחיר יחידה חייב להיות מספר", false);
            return;
        }

        SetValidationMessage("הפרטים נראים תקינים", true);
    }

    private async void AddMedication_Click(object? sender, RoutedEventArgs e)
    {
        if (_isEditing)
        {
            await UpdateMedicationAsync();
            return;
        }

        if (!TryReadFields(out var name, out var stock, out var price, out var expiration, out var notes))
            return;

        var result = await AppServices.Medications.AddAsync(name, stock, price, expiration, notes);
        UIHelper.ShowMessage(this, TranslateMessage(result.Message));
        if (!result.IsSuccess)
            return;

        UIHelper.ShowMessage(this, "התרופה נוספה בהצלחה");
        HideMedicationForm();
        await LoadAsync();
    }

    private void SearchMedication_Click(object? sender, RoutedEventArgs e)
    {
        var medication = FindSelectedOrTyped();
        if (medication is null)
        {
            UIHelper.ShowMessage(this, "בחר תרופה מהרשימה או הקלד שם תרופה קיים");
            return;
        }

        FillFields(medication);
    }

    private async void UpdateMedication_Click(object? sender, RoutedEventArgs e) => await UpdateMedicationAsync();

    private async Task UpdateMedicationAsync()
    {
        var medication = FindSelectedOrTyped();
        if (medication is null)
        {
            UIHelper.ShowMessage(this, "בחר תרופה מהרשימה לפני עדכון");
            return;
        }

        if (!TryReadFields(out _, out var stock, out var price, out var expiration, out var notes, allowEmptyName: true))
            return;

        var result = await AppServices.Medications.UpdateAsync(medication.Id, stock, price, expiration, notes);
        UIHelper.ShowMessage(this, TranslateMessage(result.Message));
        if (!result.IsSuccess)
            return;

        UIHelper.ShowMessage(this, "התרופה עודכנה בהצלחה");
        _isEditing = true;
        _editingId = medication.Id;
        ShowMedicationForm("עריכת תרופה קיימת", "הנתונים עודכנו. אפשר להמשיך לערוך או לסגור את הפרטים");
        UpdateMedicationActionMode();
        await LoadAsync();
    }

    private async void DeleteMedication_Click(object? sender, RoutedEventArgs e)
    {
        var medication = FindSelectedOrTyped();
        if (medication is null)
        {
            UIHelper.ShowMessage(this, "בחר תרופה מהרשימה לפני מחיקה");
            return;
        }

        var result = await AppServices.Medications.DeleteAsync(medication.Id);
        UIHelper.ShowMessage(this, TranslateMessage(result.Message));
        if (!result.IsSuccess)
            return;

        UIHelper.ShowMessage(this, "התרופה נמחקה בהצלחה");
        HideMedicationForm();
        await LoadAsync();
    }

    private void MedicationSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isClearingFields)
            return;

        var medication = FindSelected();
        if (medication is not null)
            FillFields(medication);
        else
            ResetFieldsForNewEntry();
    }

    private async void MedicationSearch_Changed(object? sender, TextChangedEventArgs e) => await LoadAsync();
    private async void MedicationFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e) => await LoadAsync();

    private void ClearFields_Click(object? sender, RoutedEventArgs e) => ClearFields();

    private async void ClearMedicationSearch_Click(object? sender, RoutedEventArgs e)
    {
        MedicationSearchInput.Text = "";
        MedicationFilterDropdown.SelectedIndex = 0;
        await LoadAsync();
    }

    private void Back_Click(object? sender, RoutedEventArgs e) => BackToMainMenu?.Invoke();

    private void ClearFields() => ResetFieldsForNewEntry();

    private void ResetFieldsForNewEntry()
    {
        _isClearingFields = true;
        _isEditing = false;
        _editingId = 0;
        NameInput.Text = "";
        StockInput.Text = "";
        UnitPriceInput.Text = "";
        NotesInput.Text = "";
        ExpirationDatePicker.SelectedDate = DateTime.Today.AddMonths(6);
        if (MedicationSelector.ItemCount > 0)
            MedicationSelector.SelectedIndex = 0;
        ValidationText.Text = "";
        _isClearingFields = false;
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
        _isEditing = false;
        MedicationFormPanel.IsVisible = false;
        MedicationStartPanel.IsVisible = true;
        ValidationText.Text = "";
        UpdateMedicationActionMode();
    }

    private void SetValidationMessage(string message, bool isValid)
    {
        ValidationText.Foreground = isValid ? Brushes.ForestGreen : Brushes.Firebrick;
        ValidationText.Text = message;
    }

    private bool TryReadFields(
        out string name,
        out int stock,
        out double price,
        out DateTime expiration,
        out string notes,
        bool allowEmptyName = false)
    {
        name = NameInput.Text?.Trim() ?? "";
        var stockText = StockInput.Text?.Trim() ?? "";
        var unitPriceText = UnitPriceInput.Text?.Trim() ?? "";
        notes = NotesInput.Text?.Trim() ?? "";
        expiration = ExpirationDatePicker.SelectedDate?.DateTime.Date ?? DateTime.Today;

        if (!allowEmptyName && !UiFormValidation.IsRequiredText(name))
        {
            UIHelper.ShowMessage(this, "שם תרופה הוא שדה חובה");
            stock = 0;
            price = 0;
            return false;
        }

        if (!int.TryParse(stockText, out stock) || !UiFormValidation.IsValidStockQuantity(stock))
        {
            UIHelper.ShowMessage(this, "כמות מלאי חייבת להיות מספר שלם");
            price = 0;
            return false;
        }

        if (!double.TryParse(unitPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out price) ||
            !UiFormValidation.IsValidMoney(price))
        {
            UIHelper.ShowMessage(this, "מחיר יחידה חייב להיות מספר");
            return false;
        }

        if (expiration.Date < DateTime.Today)
        {
            UIHelper.ShowMessage(this, "תאריך תפוגה לא יכול להיות בעבר");
            return false;
        }

        return true;
    }

    private Medication? FindSelected()
    {
        var selectedName = MedicationSelector.SelectedItem?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(selectedName) || selectedName == "בחר תרופה קיימת")
            return null;

        return _items.FirstOrDefault(m => string.Equals(m.Name, selectedName, StringComparison.OrdinalIgnoreCase));
    }

    private Medication? FindSelectedOrTyped()
    {
        return FindSelected() ?? _items.FirstOrDefault(m =>
            string.Equals(m.Name, NameInput.Text?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private void FillFields(Medication medication)
    {
        _isEditing = true;
        _editingId = medication.Id;
        ShowMedicationForm("עריכת תרופה קיימת", "הנתונים נטענו מהכרטיס. אפשר לעדכן או למחוק");
        NameInput.Text = medication.Name;
        StockInput.Text = medication.StockQuantity.ToString();
        UnitPriceInput.Text = medication.UnitPrice.ToString(CultureInfo.InvariantCulture);
        ExpirationDatePicker.SelectedDate = medication.ExpirationDate;
        NotesInput.Text = medication.Notes;
        SetValidationMessage("התרופה נטענה ואפשר לעדכן אותה", true);
        UpdateMedicationActionMode();
        SelectInDropdown(medication.Name);
    }

    private void UpdateMedicationActionMode()
    {
        if (SaveMedicationButton is null || DeleteMedicationButton is null)
            return;

        SaveMedicationButton.Content = _isEditing ? "עדכן תרופה" : "שמור תרופה";
        DeleteMedicationButton.IsVisible = _isEditing;
    }

    private void RefreshMedicationSelector(string selectedName = "")
    {
        var names = _items.OrderBy(m => m.Name).Select(m => m.Name).ToList();
        names.Insert(0, "בחר תרופה קיימת");
        _isClearingFields = true;
        MedicationSelector.ItemsSource = names;
        var index = string.IsNullOrWhiteSpace(selectedName)
            ? 0
            : names.FindIndex(n => string.Equals(n, selectedName, StringComparison.OrdinalIgnoreCase));
        MedicationSelector.SelectedIndex = index >= 0 ? index : 0;
        _isClearingFields = false;
    }

    private void RefreshMedicationsList()
    {
        if (MedicationCardsPanel is null || MedicationResultsText is null)
            return;

        MedicationCardsPanel.Children.Clear();
        var filtered = _items.OrderBy(m => m.Name).ToList();
        MedicationResultsText.Text = $"מציג {filtered.Count} תרופות";

        if (filtered.Count == 0)
        {
            MedicationCardsPanel.Children.Add(new TextBlock
            {
                Text = "אין תרופות במערכת",
                FontSize = 18,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20)
            });
            return;
        }

        foreach (var medication in filtered)
            MedicationCardsPanel.Children.Add(CreateMedicationCard(medication));
    }

    private Button CreateMedicationCard(Medication medication)
    {
        var statusText = GetStatusText(medication);
        var statusColor = GetStatusColor(medication);

        var card = new Button
        {
            Width = 220,
            MinHeight = 245,
            Margin = new Thickness(8),
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse(statusColor)),
            BorderThickness = new Thickness(3),
            Content = new StackPanel
            {
                Spacing = 7,
                Margin = new Thickness(14),
                Children =
                {
                    new TextBlock { Text = medication.Name, FontSize = 21, FontWeight = FontWeight.Bold, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap },
                    new TextBlock { Text = $"מלאי: {medication.StockQuantity}", FontSize = 14, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = $"מחיר ליחידה: {medication.UnitPrice:0.00}", FontSize = 14, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = $"תוקף: {medication.ExpirationDate:dd/MM/yyyy}", FontSize = 13, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = statusText, FontSize = 12, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse(statusColor)), TextAlignment = TextAlignment.Center }
                }
            }
        };

        card.Click += async (_, _) =>
        {
            FillFields(medication);
            await Task.CompletedTask;
        };
        return card;
    }

    private static string GetStatusText(Medication medication)
    {
        if (medication.IsLowStock && medication.IsExpiringSoon)
            return "מלאי + תוקף";
        if (medication.IsLowStock)
            return "מלאי נמוך";
        if (medication.IsExpiringSoon)
            return "תוקף קרוב";
        return "תקין";
    }

    private static string GetStatusColor(Medication medication) =>
        medication.IsLowStock || medication.IsExpiringSoon ? "#D64545" : "#1E8F4D";

    private void SelectInDropdown(string name)
    {
        for (var i = 0; i < MedicationSelector.ItemCount; i++)
        {
            if (MedicationSelector.Items[i]?.ToString() == name)
            {
                MedicationSelector.SelectedIndex = i;
                return;
            }
        }
    }

    private string MapFilterLabel()
    {
        if (MedicationFilterDropdown?.SelectedItem is not ComboBoxItem item || item.Content is null)
            return MedicationSearchFilter.FilterAll;

        var label = item.Content.ToString() ?? "";
        return label switch
        {
            "מלאי נמוך בלבד" => MedicationSearchFilter.FilterLowStock,
            "תוקף קרוב בלבד" => MedicationSearchFilter.FilterExpiringSoon,
            _ => MedicationSearchFilter.FilterAll
        };
    }

    private static string TranslateMessage(string message) => message switch
    {
        "Medicine added successfully." => "התרופה נוספה בהצלחה",
        "Medicine updated successfully." => "התרופה עודכנה בהצלחה",
        "Medicine removed successfully." => "התרופה נמחקה בהצלחה",
        "A medicine with this name already exists." => "תרופה בשם זה כבר קיימת במערכת",
        "Medicine not found." => "התרופה לא נמצאה",
        "Medicine name is required." => "שם תרופה הוא שדה חובה",
        "Stock quantity must be zero or greater." => "כמות מלאי חייבת להיות אפס או יותר",
        "Unit price must be zero or greater." => "מחיר יחידה חייב להיות אפס או יותר",
        _ => message
    };
}
