using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Single top-level window; all screens are swapped inside <see cref="_host"/>.
/// </summary>
public sealed class MainShellForm : Form
{
    private readonly EmployeeAuthenticationService _auth;
    private readonly EmployeeRegistrationService _registration;
    private readonly EmployeeApprovalService _approvals;
    private readonly IEmployeeRepository _employees;
    private readonly CustomerDirectoryService _customerDirectory;
    private readonly Panel _host = new() { Dock = DockStyle.Fill };

    public MainShellForm(
        EmployeeAuthenticationService auth,
        EmployeeRegistrationService registration,
        EmployeeApprovalService approvals,
        IEmployeeRepository employees,
        CustomerDirectoryService customerDirectory)
    {
        _auth = auth;
        _registration = registration;
        _approvals = approvals;
        _employees = employees;
        _customerDirectory = customerDirectory;

        Text = "ClinicVets";
        Icon = AppBranding.CreateWindowIcon();
        MinimumSize = new Size(960, 640);
        MaximizeBox = true;
        MinimizeBox = true;
        BackColor = UiTheme.PageBackground;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        StartPosition = FormStartPosition.Manual;
        Bounds = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 800);
        WindowState = FormWindowState.Maximized;

        Controls.Add(_host);
        NavigateToLogin();
    }

    public void NavigateToLogin()
    {
        AcceptButton = null;
        CancelButton = null;
        var page = new LoginPage(_auth, this);
        AcceptButton = page.SubmitButton;
        SwapContent(page);
    }

    public void NavigateToRegister()
    {
        AcceptButton = null;
        CancelButton = null;
        var page = new RegisterPage(_registration, this);
        CancelButton = page.BackButton;
        SwapContent(page);
    }

    public void NavigateToDashboard(Employee employee)
    {
        AcceptButton = null;
        CancelButton = null;
        var page = new DashboardPage(employee, this, _registration, _approvals, _employees, _customerDirectory);
        SwapContent(page);
    }

    private void SwapContent(Control next)
    {
        next.Dock = DockStyle.Fill;
        _host.SuspendLayout();
        while (_host.Controls.Count > 0)
        {
            var old = _host.Controls[0];
            _host.Controls.Remove(old);
            old.Dispose();
        }

        _host.Controls.Add(next);
        _host.ResumeLayout(true);
    }
}
