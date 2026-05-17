using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Helpers;
using ClinicVets.Desktop.Stability;

namespace ClinicVets.Desktop.Views;

public partial class ClientsView : UserControl
{
    public Action? BackToMainMenu;
    private readonly List<Customer> _items = new();

    public ClientsView()
    {
        InitializeComponent();
        _ = SafeViewLoader.RunSafeAsync(this, LoadAsync, "Clients.Load");
    }

    private async Task LoadAsync()
    {
        _items.Clear();
        _items.AddRange(await AppServices.Customers.ListCustomersAsync());
        RefreshClientsList();
    }

    private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e) => ValidateInputs(false);

    private bool ValidateInputs(bool showEmpty)
    {
        if (string.IsNullOrWhiteSpace(FullNameInput.Text) && string.IsNullOrWhiteSpace(IdNumberInput.Text))
        {
            ValidationText.Text = showEmpty ? "יש למלא פרטים" : "";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(IdNumberInput.Text) && !UiFormValidation.IsValidNationalId(IdNumberInput.Text!))
        {
            SetValidationMessage("תעודת זהות חייבת להיות 9 ספרות", false);
            return false;
        }

        SetValidationMessage("הפרטים נראים תקינים", true);
        return true;
    }

    private async void AddClient_Click(object? sender, RoutedEventArgs e)
    {
        if (!ValidateInputs(true))
            return;

        var result = await AppServices.Customers.RegisterCustomerAsync(
            FullNameInput.Text!.Trim(),
            IdNumberInput.Text!.Trim(),
            PhoneInput.Text?.Trim() ?? "",
            EmailInput.Text?.Trim() ?? "");

        if (!result.Ok)
        {
            UIHelper.ShowMessage(this, result.Message);
            return;
        }

        UIHelper.ShowMessage(this, "הלקוח נוסף בהצלחה");
        ClearFields();
        await LoadAsync();
    }

    private async void SearchClient_Click(object? sender, RoutedEventArgs e)
    {
        _items.Clear();
        _items.AddRange(await AppServices.Customers.SearchCustomersAsync(ClientSearchInput.Text ?? ""));
        RefreshClientsList();
    }

    private void ClearFields_Click(object? sender, RoutedEventArgs e) => ClearFields();

    private void UpdateClient_Click(object? sender, RoutedEventArgs e) =>
        UIHelper.ShowMessage(this, "עדכון לקוח יתווסף בגרסה הבאה.");

    private void DeleteClient_Click(object? sender, RoutedEventArgs e) =>
        UIHelper.ShowMessage(this, "מחיקת לקוח אינה זמינה בגרסה זו.");

    private void OpenClientAnimals_Click(object? sender, RoutedEventArgs e) =>
        UIHelper.ShowMessage(this, "עבור לתפריט הראשי → בעלי חיים.");

    private void BackToClients_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientDetailsText is not null)
            ClientDetailsText.Text = string.Empty;
    }

    private void Back_Click(object? sender, RoutedEventArgs e) => BackToMainMenu?.Invoke();

    private void ClearFields()
    {
        FullNameInput.Text = "";
        IdNumberInput.Text = "";
        PhoneInput.Text = "";
        EmailInput.Text = "";
        ValidationText.Text = "";
    }

    private void SetValidationMessage(string message, bool isValid)
    {
        ValidationText.Foreground = isValid ? Brushes.ForestGreen : Brushes.Firebrick;
        ValidationText.Text = message;
    }

    private void RefreshClientsList()
    {
        if (ClientCardsPanel is null)
            return;

        ClientCardsPanel.Children.Clear();
        if (ClientDetailsText is not null)
            ClientDetailsText.Text = $"מציג {_items.Count} לקוחות";

        foreach (var client in _items.OrderBy(c => c.FullName))
            ClientCardsPanel.Children.Add(CreateClientCard(client));
    }

    private Button CreateClientCard(Customer client)
    {
        var card = new Button
        {
            Width = 220,
            MinHeight = 200,
            Margin = new Thickness(8),
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.Parse("#0097C7")),
            BorderThickness = new Thickness(2),
            Content = new StackPanel
            {
                Spacing = 6,
                Margin = new Thickness(14),
                Children =
                {
                    new TextBlock { Text = client.FullName, FontSize = 20, FontWeight = FontWeight.Bold, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = $"ת.ז: {client.NationalId}", FontSize = 13, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = client.Phone, FontSize = 13, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = client.Email, FontSize = 12, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap }
                }
            }
        };
        card.Click += (_, _) =>
        {
            FullNameInput.Text = client.FullName;
            IdNumberInput.Text = client.NationalId;
            PhoneInput.Text = client.Phone;
            EmailInput.Text = client.Email;
        };
        return card;
    }
}
