using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Post-login host: unified modern shell for every employee role.
/// </summary>
public sealed class DashboardPage : UserControl
{
    public DashboardPage(
        Employee employee,
        MainShellForm shell,
        EmployeeRegistrationService registration,
        EmployeeApprovalService approvals,
        IEmployeeRepository repository,
        CustomerDirectoryService customerDirectory)
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
        Dock = DockStyle.Fill;
        BackColor = UiTheme.PageBackground;
        Font = shell.Font;

        var shellView = new ClinicShellView(employee, shell, registration, approvals, repository, customerDirectory);
        shellView.Dock = DockStyle.Fill;
        Controls.Add(shellView);
    }
}
