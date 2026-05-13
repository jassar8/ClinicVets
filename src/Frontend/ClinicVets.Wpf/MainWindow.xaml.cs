using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Core.Entities;
using ClinicVets.Infrastructure.Demo;
using ClinicVets.Wpf.Views;

namespace ClinicVets.Wpf;

public partial class MainWindow : Window
{
    private readonly EmployeeAuthenticationService _auth;
    private readonly EmployeeRegistrationService _registration;
    private readonly EmployeeApprovalService _approvals;
    private readonly IEmployeeRepository _employees;
    private readonly CustomerDirectoryService _customers;

    private IEmployeeRepository? _demoEmployees;
    private CustomerDirectoryService? _demoCustomers;
    private EmployeeApprovalService? _demoApprovals;

    public MainWindow(
        EmployeeAuthenticationService auth,
        EmployeeRegistrationService registration,
        EmployeeApprovalService approvals,
        IEmployeeRepository employees,
        CustomerDirectoryService customers)
    {
        InitializeComponent();
        _auth = auth;
        _registration = registration;
        _approvals = approvals;
        _employees = employees;
        _customers = customers;

        if (WpfBranding.OpenIconStream() is Stream iconStream)
        {
            Icon = BitmapFrame.Create(iconStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            iconStream.Dispose();
        }

        ShowLogin();
    }

    public void ShowLogin()
    {
        DemoModeSession.Exit();
        Title = "ClinicVets";
        Root.Content = new LoginView(_auth, this);
    }

    public void ShowRegister()
    {
        Root.Content = new RegisterView(_registration, this);
    }

    public void ShowShell(Employee employee, bool useQuickAccessData)
    {
        Title = useQuickAccessData ? "ClinicVets — Demo Mode (not real login)" : "ClinicVets";
        var empRepo = useQuickAccessData ? _demoEmployees! : _employees;
        var cust = useQuickAccessData ? _demoCustomers! : _customers;
        var appr = useQuickAccessData ? _demoApprovals! : _approvals;
        Root.Content = new ShellView(employee, useQuickAccessData, empRepo, cust, appr, this);
    }

    public void EnterDemo()
    {
#pragma warning disable CS0162
        if (!DesktopBuildOptions.EnableDemoMode)
            return;
#pragma warning restore CS0162

        if (!DemoWorkspace.TryInitializeDemoData(out var emp, out var cust, out var admin, out var err))
        {
            MessageBox.Show(this, err, "ClinicVets — Demo Mode", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _demoEmployees = emp;
        _demoCustomers = cust;
        _demoApprovals = new EmployeeApprovalService(_demoEmployees);
        DemoModeSession.Enter();
        ShowShell(admin, useQuickAccessData: true);
    }
}
