using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

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
        BackColor = UiTheme.PageBackground;
        Font = new Font("Segoe UI", 10F);

        var header = new Panel
        {
            Height = 72,
            Dock = DockStyle.Top,
            BackColor = UiTheme.HeaderBlue
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
            ForeColor = UiTheme.SubtitleOnHeader,
            AutoSize = true,
            Location = new Point(24, 46)
        });

        var card = new Panel
        {
            Location = new Point(32, 96),
            Size = new Size(456, 200),
            BackColor = UiTheme.CardWhite
        };

        card.Controls.Add(new Label
        {
            Text = $"Welcome, {_employee.FullName}",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Location = new Point(24, 20)
        });
        card.Controls.Add(new Label
        {
            Text = $"Email: {_employee.Email}",
            AutoSize = true,
            Location = new Point(24, 54),
            ForeColor = UiTheme.TextMuted
        });
        card.Controls.Add(new Label
        {
            Text = $"Role: {_employee.Role}",
            AutoSize = true,
            Location = new Point(24, 80),
            ForeColor = UiTheme.TextMuted
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
