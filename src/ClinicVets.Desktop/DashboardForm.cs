using ClinicVets.Core.Entities;

namespace ClinicVets.Desktop;

/// <summary>
/// Simple post-login home screen for the course demo.
/// </summary>
public class DashboardForm : Form
{
    private readonly Employee _employee;

    public DashboardForm(Employee employee)
    {
        _employee = employee;
        Text = "ClinicVets — Dashboard";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 340);
        BackColor = Color.FromArgb(243, 247, 251);
        Font = new Font("Segoe UI", 10F);

        var header = new Panel
        {
            Height = 72,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(30, 95, 164)
        };
        header.Controls.Add(new Label
        {
            Text = "ClinicVets Dashboard",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(24, 16)
        });
        header.Controls.Add(new Label
        {
            Text = "You are signed in.",
            ForeColor = Color.FromArgb(220, 235, 255),
            AutoSize = true,
            Location = new Point(24, 46)
        });

        var card = new Panel
        {
            Location = new Point(32, 96),
            Size = new Size(456, 200),
            BackColor = Color.White
        };

        card.Controls.Add(new Label
        {
            Text = $"Welcome, {_employee.FullName}",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(38, 54, 74),
            AutoSize = true,
            Location = new Point(24, 20)
        });
        card.Controls.Add(new Label
        {
            Text = $"Email: {_employee.Email}",
            AutoSize = true,
            Location = new Point(24, 54),
            ForeColor = Color.FromArgb(90, 110, 130)
        });
        card.Controls.Add(new Label
        {
            Text = $"Role: {_employee.Role}",
            AutoSize = true,
            Location = new Point(24, 80),
            ForeColor = Color.FromArgb(90, 110, 130)
        });

        var info = new Label
        {
            Text = "This screen represents the clinic system home after login.\nFurther modules can be added here for your project scope.",
            Location = new Point(24, 112),
            Size = new Size(408, 48),
            ForeColor = Color.FromArgb(120, 135, 150)
        };
        card.Controls.Add(info);

        var logout = new Button
        {
            Text = "Logout",
            Location = new Point(24, 168),
            Size = new Size(120, 32)
        };
        logout.Click += (_, _) => Close();

        card.Controls.Add(logout);

        Controls.Add(card);
        Controls.Add(header);
    }
}
