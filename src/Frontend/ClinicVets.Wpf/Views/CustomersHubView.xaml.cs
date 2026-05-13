using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClinicVets.Application.Services;

namespace ClinicVets.Wpf.Views;

public partial class CustomersHubView : UserControl
{
    private readonly CustomerDirectoryService _customers;
    private readonly bool _canSearch;
    private readonly bool _canRegister;

    public CustomersHubView(CustomerDirectoryService customers, bool canSearch, bool canRegister)
    {
        InitializeComponent();
        _customers = customers;
        _canSearch = canSearch;
        _canRegister = canRegister;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        if (!_canSearch && !_canRegister)
        {
            Tabs.Visibility = Visibility.Collapsed;
            Content = new PlaceholderView(
                "Customers",
                "You do not have access to customer records with the current role.");
            return;
        }

        if (!_canSearch)
            ((TabItem)Tabs.Items[0]).Visibility = Visibility.Collapsed;
        if (!_canRegister)
            ((TabItem)Tabs.Items[1]).Visibility = Visibility.Collapsed;
        if (_canSearch)
            Tabs.SelectedIndex = 0;
        else
            Tabs.SelectedIndex = 1;
    }

    private async void OnSearch(object sender, RoutedEventArgs e)
    {
        var list = await _customers.SearchCustomersAsync(Query.Text.Trim());
        Grid.ItemsSource = list;
    }

    private async void OnSaveCustomer(object sender, RoutedEventArgs e)
    {
        RegMessage.Visibility = Visibility.Collapsed;
        var r = await _customers.RegisterCustomerAsync(RegName.Text, RegNationalId.Text, RegPhone.Text, RegEmail.Text);
        RegMessage.Text = r.Message;
        RegMessage.Foreground = new SolidColorBrush(r.Ok ? Color.FromRgb(0x16, 0x65, 0x34) : Color.FromRgb(0xEF, 0x44, 0x44));
        RegMessage.Visibility = Visibility.Visible;
    }
}
