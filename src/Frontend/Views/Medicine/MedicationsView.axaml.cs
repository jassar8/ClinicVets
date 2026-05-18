using ClinicVets.Desktop.Services;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Helpers;
using ClinicVets.Desktop.Helpers.Stability;

namespace ClinicVets.Desktop.Views.Medicine;

public partial class MedicationsView : UserControl
{
    public Action? BackToMainMenu;
    private readonly List<Medication> _items = new();
    private IReadOnlyList<string> _selectorNames = Array.Empty<string>();
    private bool _isEditing;
    private bool _isClearingFields;
    private bool _suppressEvents = true;

    public MedicationsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        SafeViewLoader.RunSafe(this, () =>
        {
            SetDefaultExpirationDate();
            _suppressEvents = false;
        }, "Medications.Init");

        _ = SafeViewLoader.RunSafeAsync(this, LoadAsync, "Medications.Load");
    }

    private void SetDefaultExpirationDate() =>
        ExpirationDatePicker.SelectedDate = ToDateOffset(DateTime.Today.AddMonths(6));

    private static DateTimeOffset ToDateOffset(DateTime date) =>
        new DateTimeOffset(date.Date);

    private async Task LoadAsync()
    {
        if (AppServices.Medications is null)
            throw new InvalidOperationException("Medication service is not initialized.");

        _items.Clear();
        _items.AddRange(await AppServices.Medications.SearchAsync(
            MedicationSearchInput?.Text,
            MapFilterLabel()));
        RefreshMedicationSelector();
        RefreshMedicationsList();
    }

    private async Task RunLoadSafeAsync(string context)
    {
        if (_suppressEvents)
            return;
        await SafeViewLoader.RunSafeAsync(this, LoadAsync, context);
    }

    private void StartAddMedication_Click(object? sender, RoutedEventArgs e)
    {
        _isEditing = false;
        ClearFields();
        ShowMedicationForm("????? ????? ????", "??? ?? ???? ?????? ??? ??? ???? ?????");
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
            ValidationText.Text = showEmpty ? "?? ???? ?????" : "";
            return;
        }

        if (!UiFormValidation.IsRequiredText(name))
        {
            SetValidationMessage("?? ????? ??? ??? ????", false);
            return;
        }

        if (!string.IsNullOrWhiteSpace(stockText) && (!int.TryParse(stockText, out var stock) || !UiFormValidation.IsValidStockQuantity(stock)))
        {
            SetValidationMessage("???? ???? ????? ????? ???? ???", false);
            return;
        }

        if (!string.IsNullOrWhiteSpace(unitPriceText) &&
            (!double.TryParse(unitPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ||
             !UiFormValidation.IsValidMoney(price)))
        {
            SetValidationMessage("???? ????? ???? ????? ????", false);
            return;
        }

        SetValidationMessage("?????? ????? ??????", true);
    }

    private async void AddMedication_Click(object? sender, RoutedEventArgs e)
    {
        try
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

            UIHelper.ShowMessage(this, "?????? ????? ??????");
            HideMedicationForm();
            await RunLoadSafeAsync("Medications.AfterAdd");
        }
        catch (Exception ex)
        {
            AppStability.LogException("Medications.Add", ex);
            UIHelper.ShowMessage(this, SafeViewLoader.FriendlyMessage("??????"));
        }
    }

    private void SearchMedication_Click(object? sender, RoutedEventArgs e)
    {
        var medication = FindSelectedOrTyped();
        if (medication is null)
        {
            UIHelper.ShowMessage(this, "??? ????? ??????? ?? ???? ?? ????? ????");
            return;
        }

        FillFields(medication);
    }

    private async void UpdateMedication_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await UpdateMedicationAsync();
        }
        catch (Exception ex)
        {
            AppStability.LogException("Medications.Update", ex);
            UIHelper.ShowMessage(this, SafeViewLoader.FriendlyMessage("??????"));
        }
    }

    private async Task UpdateMedicationAsync()
    {
        var medication = FindSelectedOrTyped();
        if (medication is null)
        {
            UIHelper.ShowMessage(this, "??? ????? ??????? ???? ?????");
            return;
        }

        if (!TryReadFields(out _, out var stock, out var price, out var expiration, out var notes, allowEmptyName: true))
            return;

        var result = await AppServices.Medications.UpdateAsync(medication.Id, stock, price, expiration, notes);
        UIHelper.ShowMessage(this, TranslateMessage(result.Message));
        if (!result.IsSuccess)
            return;

        UIHelper.ShowMessage(this, "?????? ?????? ??????");
        _isEditing = true;
        ShowMedicationForm("????? ????? ?????", "??????? ??????. ???? ?????? ????? ?? ????? ?? ??????");
        UpdateMedicationActionMode();
        await RunLoadSafeAsync("Medications.AfterUpdate");
    }

    private async void DeleteMedication_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var medication = FindSelectedOrTyped();
            if (medication is null)
            {
                UIHelper.ShowMessage(this, "??? ????? ??????? ???? ?????");
                return;
            }

            var result = await AppServices.Medications.DeleteAsync(medication.Id);
            UIHelper.ShowMessage(this, TranslateMessage(result.Message));
            if (!result.IsSuccess)
                return;

            UIHelper.ShowMessage(this, "?????? ????? ??????");
            HideMedicationForm();
            await RunLoadSafeAsync("Medications.AfterDelete");
        }
        catch (Exception ex)
        {
            AppStability.LogException("Medications.Delete", ex);
            UIHelper.ShowMessage(this, SafeViewLoader.FriendlyMessage("??????"));
        }
    }

    private void MedicationSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressEvents || _isClearingFields)
            return;

        var medication = FindSelected();
        if (medication is not null)
            FillFields(medication);
        else
            ResetFieldsForNewEntry();
    }

    private async void MedicationSearch_Changed(object? sender, TextChangedEventArgs e) =>
        await RunLoadSafeAsync("Medications.Search");

    private async void MedicationFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        await RunLoadSafeAsync("Medications.Filter");

    private void ClearFields_Click(object? sender, RoutedEventArgs e) => ClearFields();

    private async void ClearMedicationSearch_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _suppressEvents = true;
            MedicationSearchInput.Text = "";
            MedicationFilterDropdown.SelectedIndex = 0;
            _suppressEvents = false;
            await RunLoadSafeAsync("Medications.ClearSearch");
        }
        catch (Exception ex)
        {
            _suppressEvents = false;
            AppStability.LogException("Medications.ClearSearch", ex);
            UIHelper.ShowMessage(this, SafeViewLoader.FriendlyMessage("??????"));
        }
    }

    private void Back_Click(object? sender, RoutedEventArgs e) => BackToMainMenu?.Invoke();

    private void ClearFields() => ResetFieldsForNewEntry();

    private void ResetFieldsForNewEntry()
    {
        _isClearingFields = true;
        _isEditing = false;
        NameInput.Text = "";
        StockInput.Text = "";
        UnitPriceInput.Text = "";
        NotesInput.Text = "";
        SetDefaultExpirationDate();
        if (_selectorNames.Count > 0)
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
            UIHelper.ShowMessage(this, "?? ????? ??? ??? ????");
            stock = 0;
            price = 0;
            return false;
        }

        if (!int.TryParse(stockText, out stock) || !UiFormValidation.IsValidStockQuantity(stock))
        {
            UIHelper.ShowMessage(this, "???? ???? ????? ????? ???? ???");
            price = 0;
            return false;
        }

        if (!double.TryParse(unitPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out price) ||
            !UiFormValidation.IsValidMoney(price))
        {
            UIHelper.ShowMessage(this, "???? ????? ???? ????? ????");
            return false;
        }

        if (expiration.Date < DateTime.Today)
        {
            UIHelper.ShowMessage(this, "????? ????? ?? ???? ????? ????");
            return false;
        }

        return true;
    }

    private Medication? FindSelected()
    {
        var selectedName = MedicationSelector.SelectedItem?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(selectedName) || selectedName == "??? ????? ?????")
            return null;

        return _items.FirstOrDefault(m => string.Equals(m.Name, selectedName, StringComparison.OrdinalIgnoreCase));
    }

    private Medication? FindSelectedOrTyped() =>
        FindSelected() ?? _items.FirstOrDefault(m =>
            string.Equals(m.Name, NameInput.Text?.Trim(), StringComparison.OrdinalIgnoreCase));

    private void FillFields(Medication medication)
    {
        _isEditing = true;
        ShowMedicationForm("????? ????? ?????", "??????? ????? ???????. ???? ????? ?? ?????");
        NameInput.Text = medication.Name;
        StockInput.Text = medication.StockQuantity.ToString();
        UnitPriceInput.Text = medication.UnitPrice.ToString(CultureInfo.InvariantCulture);
        ExpirationDatePicker.SelectedDate = ToDateOffset(medication.ExpirationDate);
        NotesInput.Text = medication.Notes;
        SetValidationMessage("?????? ????? ????? ????? ????", true);
        UpdateMedicationActionMode();
        SelectInDropdown(medication.Name);
    }

    private void UpdateMedicationActionMode()
    {
        if (SaveMedicationButton is null || DeleteMedicationButton is null)
            return;

        SaveMedicationButton.Content = _isEditing ? "???? ?????" : "???? ?????";
        DeleteMedicationButton.IsVisible = _isEditing;
    }

    private void RefreshMedicationSelector(string selectedName = "")
    {
        var names = _items.OrderBy(m => m.Name).Select(m => m.Name).ToList();
        names.Insert(0, "??? ????? ?????");
        _selectorNames = names;

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
        MedicationResultsText.Text = $"???? {filtered.Count} ??????";

        if (filtered.Count == 0)
        {
            MedicationCardsPanel.Children.Add(new TextBlock
            {
                Text = "??? ?????? ??????",
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
                    new TextBlock { Text = $"????: {medication.StockQuantity}", FontSize = 14, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = $"???? ??????: {medication.UnitPrice:0.00}", FontSize = 14, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = $"????: {medication.ExpirationDate:dd/MM/yyyy}", FontSize = 13, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = statusText, FontSize = 12, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse(statusColor)), TextAlignment = TextAlignment.Center }
                }
            }
        };

        card.Click += (_, _) => SafeViewLoader.RunSafe(this, () => FillFields(medication), "Medications.CardClick");
        return card;
    }

    private static string GetStatusText(Medication medication)
    {
        if (medication.IsLowStock && medication.IsExpiringSoon)
            return "???? + ????";
        if (medication.IsLowStock)
            return "???? ????";
        if (medication.IsExpiringSoon)
            return "???? ????";
        return "????";
    }

    private static string GetStatusColor(Medication medication) =>
        medication.IsLowStock || medication.IsExpiringSoon ? "#D64545" : "#1E8F4D";

    private void SelectInDropdown(string name)
    {
        var index = _selectorNames.ToList().FindIndex(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
            MedicationSelector.SelectedIndex = index;
    }

    private string MapFilterLabel() =>
        MedicationFilterDropdown?.SelectedIndex switch
        {
            1 => MedicationSearchFilter.FilterLowStock,
            2 => MedicationSearchFilter.FilterExpiringSoon,
            _ => MedicationSearchFilter.FilterAll
        };

    private static string TranslateMessage(string message) => message switch
    {
        "Medicine added successfully." => "?????? ????? ??????",
        "Medicine updated successfully." => "?????? ?????? ??????",
        "Medicine removed successfully." => "?????? ????? ??????",
        "A medicine with this name already exists." => "????? ??? ?? ??? ????? ??????",
        "Medicine not found." => "?????? ?? ?????",
        "Medicine name is required." => "?? ????? ??? ??? ????",
        "Stock quantity must be zero or greater." => "???? ???? ????? ????? ??? ?? ????",
        "Unit price must be zero or greater." => "???? ????? ???? ????? ??? ?? ????",
        _ => message
    };
}
