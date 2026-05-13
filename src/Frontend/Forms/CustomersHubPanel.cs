using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>Search and register customers in one workspace (tabs when both are allowed).</summary>
public sealed class CustomersHubPanel : UserControl
{
    public CustomersHubPanel(CustomerDirectoryService customers, Employee employee)
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        var canSearch = RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerSearch);
        var canRegister = RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerRegistration);

        if (canSearch && canRegister)
        {
            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
                Padding = new Point(12, 8)
            };
            var searchTab = new TabPage("Search customers");
            searchTab.Controls.Add(new CustomerSearchPanel(customers) { Dock = DockStyle.Fill });
            var regTab = new TabPage("Register customer");
            regTab.Controls.Add(new CustomerRegistrationPanel(customers) { Dock = DockStyle.Fill });
            tabs.TabPages.Add(searchTab);
            tabs.TabPages.Add(regTab);
            Controls.Add(tabs);
        }
        else if (canSearch)
        {
            Controls.Add(new CustomerSearchPanel(customers) { Dock = DockStyle.Fill });
        }
        else if (canRegister)
        {
            Controls.Add(new CustomerRegistrationPanel(customers) { Dock = DockStyle.Fill });
        }
        else
        {
            Controls.Add(new AdminPlaceholderPanel(
                "Customers",
                "You do not have access to customer records in this demo build."));
        }
    }
}
