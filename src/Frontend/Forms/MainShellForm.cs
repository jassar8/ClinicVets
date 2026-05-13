using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;
using ClinicVets.Infrastructure.Demo;

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

    private IEmployeeRepository _demoEmployees = null!;
    private CustomerDirectoryService _demoCustomerDirectory = null!;
    private EmployeeRegistrationService _demoRegistration = null!;
    private EmployeeApprovalService _demoApprovals = null!;

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
        DemoModeSession.Exit();
        Text = "ClinicVets";
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

    /// <summary>In-memory demo workspace for UI review; does not authenticate or touch JSON stores.</summary>
    public void NavigateToDemo()
    {
#pragma warning disable CS0162 // False branch is used when EnableDemoMode is set to false for teacher builds.
        if (!DesktopBuildOptions.EnableDemoMode)
            return;
#pragma warning restore CS0162

        try
        {
            if (!DemoWorkspace.TryInitializeDemoData(out var emp, out var cust, out var admin, out var err))
            {
                MessageBox.Show(
                    this,
                    err,
                    "ClinicVets — Demo Mode",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _demoEmployees = emp;
            _demoCustomerDirectory = cust;
            _demoRegistration = new EmployeeRegistrationService(_demoEmployees);
            _demoApprovals = new EmployeeApprovalService(_demoEmployees);
            DemoModeSession.Enter();
            Text = "ClinicVets — Demo Mode (not real login)";
            NavigateToDashboard(admin, useQuickAccessData: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                "Demo Mode failed unexpectedly." + Environment.NewLine + Environment.NewLine + ex.Message,
                "ClinicVets — Demo Mode",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    public void NavigateToDashboard(Employee employee, bool useQuickAccessData = false)
    {
        AcceptButton = null;
        CancelButton = null;
        var repository = useQuickAccessData ? _demoEmployees : _employees;
        var customerDirectory = useQuickAccessData ? _demoCustomerDirectory : _customerDirectory;
        var registration = useQuickAccessData ? _demoRegistration : _registration;
        var approvals = useQuickAccessData ? _demoApprovals : _approvals;
        var page = new DashboardPage(employee, this, registration, approvals, repository, customerDirectory, useQuickAccessData);
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
