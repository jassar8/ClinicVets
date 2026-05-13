using System.Windows.Controls;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Core.Entities;

namespace ClinicVets.Wpf.Views;

public partial class DashboardHomeView : UserControl
{
    public DashboardHomeView(Employee sessionEmployee, CustomerDirectoryService? customers)
    {
        InitializeComponent();
        var first = sessionEmployee.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(0) ?? "there";
        Welcome.Text = $"Welcome back, {first}";

        Loaded += async (_, _) =>
        {
            var eff = DemoModeSession.GetEffectiveEmployee(sessionEmployee);
            var canCust = RolePermissions.CanAccessDashboardSection(eff, DashboardSection.CustomerSearch) ||
                          RolePermissions.CanAccessDashboardSection(eff, DashboardSection.CustomerRegistration);
            if (!canCust || customers is null)
            {
                MetricCustomers.Text = "—";
                MetricAnimals.Text = "—";
                return;
            }

            try
            {
                var list = await customers.ListCustomersAsync();
                MetricCustomers.Text = list.Count.ToString("D0");
                var n = 0;
                foreach (var c in list)
                    n += (await customers.GetAnimalsForCustomerAsync(c.Id)).Count;
                MetricAnimals.Text = n.ToString("D0");
            }
            catch
            {
                MetricCustomers.Text = "—";
                MetricAnimals.Text = "—";
            }
        };
    }
}
