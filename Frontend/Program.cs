using ClinicVets.Application.Services;
using ClinicVets.Desktop.Forms;
using ClinicVets.Infrastructure.Data;

namespace ClinicVets.Desktop;

static class Program
{
    /// <summary>
    /// Desktop entry point: no browser, no Kestrel — pure WinForms.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        System.Diagnostics.Trace.AutoFlush = true;
        try
        {
            var repository = new JsonFileEmployeeRepository();
            var auth = new EmployeeAuthenticationService(repository);
            var registration = new EmployeeRegistrationService(repository);
            var approvals = new EmployeeApprovalService(repository);
            global::System.Windows.Forms.Application.Run(new MainShellForm(auth, registration, approvals, repository));
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
