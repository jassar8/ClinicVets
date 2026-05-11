using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;

namespace ClinicVets.Desktop;

/// <summary>
/// Employee login — first screen when the desktop app starts.
/// </summary>
public class LoginForm : Form
{
    private readonly EmployeeAuthenticationService _auth;
    private readonly EmployeeRegistrationService _registration;
    private readonly TextBox _email = new();
    private readonly TextBox _password = new();
    private readonly Label _error = new();
    private readonly Button _login = new();
    private readonly Button _register = new();

    public LoginForm(EmployeeAuthenticationService auth, EmployeeRegistrationService registration)
    {
        _auth = auth;
        _registration = registration;
        Text = "ClinicVets — Login";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(440, 400);
        BackColor = Color.FromArgb(243, 247, 251);
        Font = new Font("Segoe UI", 10F);

        var header = new Panel
        {
            Height = 76,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(30, 95, 164)
        };
        var title = new Label
        {
            Text = "ClinicVets",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(24, 18)
        };
        var subtitle = new Label
        {
            Text = "Veterinary Clinic Management — Employee Login",
            ForeColor = Color.FromArgb(220, 235, 255),
            AutoSize = true,
            Location = new Point(24, 50)
        };
        header.Controls.Add(title);
        header.Controls.Add(subtitle);

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 20, 28, 20),
            BackColor = Color.FromArgb(243, 247, 251)
        };

        var card = new Panel
        {
            Location = new Point(28, 20),
            Size = new Size(384, 280),
            BackColor = Color.White,
            BorderStyle = BorderStyle.None
        };

        var lblEmail = new Label { Text = "Email", Location = new Point(20, 20), AutoSize = true };
        _email.Location = new Point(20, 44);
        _email.Size = new Size(344, 28);
        _email.PlaceholderText = "name@clinicvets.com";

        var lblPassword = new Label { Text = "Password", Location = new Point(20, 84), AutoSize = true };
        _password.Location = new Point(20, 108);
        _password.Size = new Size(344, 28);
        _password.UseSystemPasswordChar = true;

        _error.Location = new Point(20, 148);
        _error.Size = new Size(344, 40);
        _error.ForeColor = Color.FromArgb(180, 40, 40);
        _error.Text = string.Empty;

        _login.Text = "Login";
        _login.Location = new Point(20, 198);
        _login.Size = new Size(344, 36);
        _login.BackColor = Color.FromArgb(30, 95, 164);
        _login.ForeColor = Color.White;
        _login.FlatStyle = FlatStyle.Flat;
        _login.FlatAppearance.BorderSize = 0;
        _login.Click += async (_, _) => await LoginAsync();

        _register.Text = "Employee Registration";
        _register.Location = new Point(20, 242);
        _register.Size = new Size(344, 32);
        _register.FlatStyle = FlatStyle.Flat;
        _register.Click += (_, _) =>
        {
            using var reg = new RegisterForm(_registration);
            reg.ShowDialog(this);
        };

        var hint = new Label
        {
            Text = "Demo: vet@clinicvets.com / Vet123!",
            ForeColor = Color.FromArgb(100, 120, 140),
            AutoSize = true,
            Location = new Point(20, 278),
            Font = new Font("Segoe UI", 8.5F)
        };

        card.Controls.AddRange(new Control[]
        {
            lblEmail, _email, lblPassword, _password, _error, _login, _register, hint
        });
        body.Controls.Add(card);

        Controls.Add(body);
        Controls.Add(header);
        AcceptButton = _login;
    }

    private async Task LoginAsync()
    {
        _error.Text = string.Empty;
        _login.Enabled = false;
        try
        {
            var result = await _auth.LoginAsync(_email.Text, _password.Text);
            if (!result.IsSuccess)
            {
                _error.Text = result.Message;
                return;
            }

            Hide();
            using var dash = new DashboardForm(result.Employee!);
            dash.ShowDialog();
            _password.Clear();
            Show();
        }
        finally
        {
            _login.Enabled = true;
        }
    }
}
