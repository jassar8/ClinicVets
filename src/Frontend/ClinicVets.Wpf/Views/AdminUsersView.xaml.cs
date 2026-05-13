using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Core.Entities;

namespace ClinicVets.Wpf.Views;

public enum UsersHubTab
{
    All,
    Pending,
    Rejected
}

public partial class AdminUsersView : UserControl
{
    private readonly Employee _sessionEmployee;
    private readonly IEmployeeRepository _repository;
    private readonly EmployeeApprovalService _approvals;
    private readonly UsersHubTab _initialTab;
    private readonly Action _onChanged;

    private IReadOnlyList<Employee> _all = Array.Empty<Employee>();
    private Employee? _selected;

    public AdminUsersView(
        Employee sessionEmployee,
        IEmployeeRepository repository,
        EmployeeApprovalService approvals,
        UsersHubTab initialTab,
        Action onChanged)
    {
        InitializeComponent();
        _sessionEmployee = sessionEmployee;
        _repository = repository;
        _approvals = approvals;
        _initialTab = initialTab;
        _onChanged = onChanged;

        FilterCombo.Items.Clear();
        FilterCombo.Items.Add(new ComboBoxItem { Content = "All employees", Tag = "All" });
        FilterCombo.Items.Add(new ComboBoxItem { Content = "Pending", Tag = "Pending" });
        FilterCombo.Items.Add(new ComboBoxItem { Content = "Approved", Tag = "Approved" });
        FilterCombo.Items.Add(new ComboBoxItem { Content = "Rejected", Tag = "Rejected" });
        FilterCombo.SelectedIndex = _initialTab switch
        {
            UsersHubTab.Pending => 1,
            UsersHubTab.Rejected => 3,
            _ => 0
        };

        Loaded += async (_, _) => await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _all = await _repository.GetAllAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var tag = (FilterCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All";
        IEnumerable<Employee> q = _all;
        q = tag switch
        {
            "Pending" => q.Where(e => string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Pending, StringComparison.OrdinalIgnoreCase)),
            "Approved" => q.Where(e => string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Approved, StringComparison.OrdinalIgnoreCase)),
            "Rejected" => q.Where(e => string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Rejected, StringComparison.OrdinalIgnoreCase)),
            _ => q
        };
        Grid.ItemsSource = q.OrderBy(e => e.FullName).ToList();
    }

    private void OnFilterChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

    private async void OnRefresh(object sender, RoutedEventArgs e) => await ReloadAsync();

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ActionMessage.Visibility = Visibility.Collapsed;
        _selected = Grid.SelectedItem as Employee;
        if (_selected is null ||
            !string.Equals(_selected.Status?.Trim(), EmployeeAccountStatusNames.Pending, StringComparison.OrdinalIgnoreCase))
        {
            ActionPanel.Visibility = Visibility.Collapsed;
            return;
        }

        ActionPanel.Visibility = Visibility.Visible;
        ActionTitle.Text = $"Pending: {_selected.FullName} ({_selected.Email}) — requested role: {_selected.RequestedRole}";
    }

    private async void OnApprove(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
            return;
        var roleItem = FinalRole.SelectedItem as ComboBoxItem;
        var roleText = roleItem?.Content?.ToString() ?? "Secretary";
        var admin = DemoModeSession.GetEffectiveEmployee(_sessionEmployee);
        var result = await _approvals.ApproveAsync(_selected.Id, roleText, admin);
        ShowActionMessage(result.Ok, result.Message);
        if (result.Ok)
        {
            await ReloadAsync();
            _onChanged();
        }
    }

    private async void OnReject(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
            return;
        var admin = DemoModeSession.GetEffectiveEmployee(_sessionEmployee);
        var result = await _approvals.RejectAsync(_selected.Id, admin);
        ShowActionMessage(result.Ok, result.Message);
        if (result.Ok)
        {
            await ReloadAsync();
            _onChanged();
        }
    }

    private void ShowActionMessage(bool ok, string message)
    {
        ActionMessage.Text = message;
        ActionMessage.Foreground = new SolidColorBrush(ok ? Color.FromRgb(0x16, 0x65, 0x34) : Color.FromRgb(0xEF, 0x44, 0x44));
        ActionMessage.Visibility = Visibility.Visible;
    }
}
