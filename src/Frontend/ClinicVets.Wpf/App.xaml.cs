using System.Windows;
using ClinicVets.Application.Services;
using ClinicVets.Infrastructure.Data;

namespace ClinicVets.Wpf;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var repository = new JsonFileEmployeeRepository();
            var customerStore = new JsonFileCustomerDirectoryRepository();
            var auth = new EmployeeAuthenticationService(repository);
            var registration = new EmployeeRegistrationService(repository);
            var approvals = new EmployeeApprovalService(repository);
            var customers = new CustomerDirectoryService(customerStore);
            MainWindow = new MainWindow(auth, registration, approvals, repository, customers);
            MainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "ClinicVets could not start.\n\n" + ex.Message,
                "ClinicVets",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}
