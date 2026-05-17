using ClinicVets.Application.Services;
using ClinicVets.Desktop.Forms;
using ClinicVets.Infrastructure.Data;

namespace ClinicVets.Desktop;

static class Program
{
    /// <summary>
    /// ClinicVets v2: single WinForms window, modern C# UI (no XAML). Entry uses JSON-backed services.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        System.Diagnostics.Trace.AutoFlush = true;
        try
        {
            var repository = new JsonFileEmployeeRepository();
            var customerStore = new JsonFileCustomerDirectoryRepository();
            var auth = new EmployeeAuthenticationService(repository);
            var registration = new EmployeeRegistrationService(repository);
            var approvals = new EmployeeApprovalService(repository);
            var customers = new CustomerDirectoryService(customerStore);
            global::System.Windows.Forms.Application.Run(new MainShellForm(auth, registration, approvals, repository, customers));
        }
        catch (Exception ex)
        {
            global::System.Windows.Forms.MessageBox.Show(
                "ClinicVets could not start.\n\n" + ex.Message,
                "ClinicVets",
                global::System.Windows.Forms.MessageBoxButtons.OK,
                global::System.Windows.Forms.MessageBoxIcon.Error);
        }
    }
}
