using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Administrator-only flow for creating any clinic role, including additional admins.
/// </summary>
public sealed class AdminCreateEmployeeForm : Form
{
    public AdminCreateEmployeeForm(Employee actingAdmin, EmployeeRegistrationService registration)
    {
        Text = "ClinicVets — add employee (administrator)";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = UiTheme.PageBackground;
        ClientSize = new Size(540, 700);
        Padding = new Padding(28);

        var panel = new AdminCreateEmployeePanel(actingAdmin, registration);
        panel.Dock = DockStyle.Fill;
        panel.Saved += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };
        panel.Cancelled += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        Controls.Add(panel);
    }
}
