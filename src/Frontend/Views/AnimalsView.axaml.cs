using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Helpers;

namespace ClinicVets.Desktop.Views;

public sealed record AnimalListItem(Animal Animal, Customer Owner);

public partial class AnimalsView : UserControl
{
    public Action? BackToMainMenu;
    private readonly List<AnimalListItem> _items = new();
    private bool _isAdding;

    public AnimalsView()
    {
        InitializeComponent();
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _items.Clear();
        foreach (var owner in await AppServices.Customers.ListCustomersAsync())
        {
            foreach (var animal in await AppServices.Customers.GetAnimalsForCustomerAsync(owner.Id))
                _items.Add(new AnimalListItem(animal, owner));
        }

        RefreshAnimalsList();
    }

    private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e) { }

    private void AnimalSearch_Changed(object? sender, TextChangedEventArgs e) => RefreshAnimalsList();
    private void AnimalFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e) => RefreshAnimalsList();

    private void ClearAnimalSearch_Click(object? sender, RoutedEventArgs e)
    {
        AnimalSearchInput.Text = "";
        AnimalFilterDropdown.SelectedIndex = 0;
        RefreshAnimalsList();
    }

    private void StartAddAnimal_Click(object? sender, RoutedEventArgs e)
    {
        _isAdding = true;
        ClearFields();
        ShowAnimalForm("הוספת חיה חדשה", "מלא שם, סוג ותעודת זהות של הבעלים (9 ספרות)");
        UpdateAnimalActionMode();
    }

    private void CloseAnimalForm_Click(object? sender, RoutedEventArgs e) => HideAnimalForm();

    private async void AddAnimal_Click(object? sender, RoutedEventArgs e)
    {
        var name = NameInput.Text?.Trim() ?? "";
        var species = SpeciesDropdown.SelectedItem is ComboBoxItem item ? item.Content?.ToString() ?? "כלב" : "כלב";
        var ownerId = OwnerIdInput.Text?.Trim() ?? "";

        if (!UiFormValidation.IsValidAnimalName(name))
        {
            UIHelper.ShowMessage(this, "שם החיה חייב להכיל אותיות");
            return;
        }

        if (!UiFormValidation.IsValidNationalId(ownerId))
        {
            UIHelper.ShowMessage(this, "תעודת זהות בעלים חייבת להיות 9 ספרות");
            return;
        }

        var matchedOwner = await AppServices.CustomerStore.GetByNationalIdAsync(ownerId);
        if (matchedOwner is null)
        {
            UIHelper.ShowMessage(this, "לא נמצא לקוח עם תעודת זהות זו. רשום את הלקוח קודם.");
            return;
        }

        await AppServices.CustomerStore.AddAnimalAsync(new Animal
        {
            CustomerId = matchedOwner.Id,
            Name = name,
            Species = species
        });

        UIHelper.ShowMessage(this, "החיה נוספה בהצלחה");
        HideAnimalForm();
        await LoadAsync();
    }

    private void DeleteAnimal_Click(object? sender, RoutedEventArgs e) =>
        UIHelper.ShowMessage(this, "מחיקת חיה אינה זמינה בגרסה זו.");

    private void ClearFields_Click(object? sender, RoutedEventArgs e) => ClearFields();

    private void Back_Click(object? sender, RoutedEventArgs e) => BackToMainMenu?.Invoke();

    private void ClearFields()
    {
        NameInput.Text = "";
        OwnerIdInput.Text = "";
        WeightInput.Text = "";
        ValidationText.Text = "";
    }

    private void ShowAnimalForm(string title, string hint)
    {
        AnimalStartPanel.IsVisible = false;
        AnimalFormPanel.IsVisible = true;
        AnimalFormTitle.Text = title;
        AnimalFormHint.Text = hint;
    }

    private void HideAnimalForm()
    {
        _isAdding = false;
        AnimalFormPanel.IsVisible = false;
        AnimalStartPanel.IsVisible = true;
        UpdateAnimalActionMode();
    }

    private void UpdateAnimalActionMode()
    {
        if (SaveAnimalButton is not null)
            SaveAnimalButton.Content = _isAdding ? "שמור חיה" : "עדכן חיה";
    }

    private void RefreshAnimalsList()
    {
        if (AnimalCardsPanel is null)
            return;

        AnimalCardsPanel.Children.Clear();
        var q = AnimalSearchInput.Text?.Trim() ?? "";
        var filtered = _items
            .Where(i => string.IsNullOrWhiteSpace(q) ||
                        i.Animal.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        i.Owner.FullName.Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Animal.Name)
            .ToList();

        if (AnimalResultsText is not null)
            AnimalResultsText.Text = $"מציג {filtered.Count} מתוך {_items.Count} בעלי חיים";

        foreach (var row in filtered)
            AnimalCardsPanel.Children.Add(CreateAnimalCard(row));

        if (filtered.Count == 0)
        {
            AnimalCardsPanel.Children.Add(new TextBlock
            {
                Text = "לא נמצאו בעלי חיים",
                FontSize = 18,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20)
            });
        }
    }

    private Button CreateAnimalCard(AnimalListItem row)
    {
        var card = new Button
        {
            Width = 230,
            MinHeight = 220,
            Margin = new Thickness(8),
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#22A06B")),
            BorderThickness = new Thickness(2),
            Content = new StackPanel
            {
                Spacing = 6,
                Margin = new Thickness(14),
                Children =
                {
                    new TextBlock { Text = row.Animal.Name, FontSize = 20, FontWeight = FontWeight.Bold, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = row.Animal.Species, FontSize = 14, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = $"בעלים: {row.Owner.FullName}", FontSize = 12, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center }
                }
            }
        };
        card.Click += (_, _) =>
        {
            _isAdding = false;
            NameInput.Text = row.Animal.Name;
            OwnerIdInput.Text = row.Owner.NationalId;
            ShowAnimalForm("עריכת חיה", "שם וסוג נשמרים במערכת v2");
            UpdateAnimalActionMode();
        };
        return card;
    }
}
