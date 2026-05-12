using ClinicVets.Application.Services;
using ClinicVets.Desktop;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Employee login — opens maximized with a centered responsive card.
/// </summary>
public class LoginForm : Form
{
    private readonly EmployeeAuthenticationService _auth;
    private readonly EmployeeRegistrationService _registration;
    private readonly Panel _body = new();
    private readonly Panel _card = new();
    private readonly FlowLayoutPanel _flow = new();
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
        Icon = AppBranding.CreateWindowIcon();
        MinimumSize = new Size(960, 640);
        MaximizeBox = true;
        MinimizeBox = true;
        BackColor = UiTheme.PageBackground;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        StartPosition = FormStartPosition.Manual;
        Bounds = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 800);
        WindowState = FormWindowState.Maximized;

        var header = new Panel
        {
            Height = 100,
            Dock = DockStyle.Top,
            BackColor = UiTheme.HeaderBlue
        };
        header.Controls.Add(new Label
        {
            Text = "ClinicVets",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 26F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(40, 22)
        });
        header.Controls.Add(new Label
        {
            Text = "Veterinary Clinic Management — Employee Login",
            ForeColor = UiTheme.SubtitleOnHeader,
            AutoSize = true,
            Location = new Point(40, 64),
            Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point)
        });

        _body.Dock = DockStyle.Fill;
        _body.BackColor = UiTheme.PageBackground;
        _body.Resize += (_, _) => Relayout();

        _card.BackColor = UiTheme.CardWhite;
        _card.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, _card.Width - 1, _card.Height - 1);
        };

        _flow.Dock = DockStyle.Fill;
        _flow.FlowDirection = FlowDirection.TopDown;
        _flow.WrapContents = false;
        _flow.AutoScroll = true;
        _flow.Padding = new Padding(40, 36, 40, 36);
        _flow.BackColor = UiTheme.CardWhite;
        _flow.SizeChanged += (_, _) => SyncFlowChildWidths();

        var lblEmail = HeaderLabel("Email");
        var lblPassword = HeaderLabel("Password");

        _email.PlaceholderText = "name@clinicvets.com";
        _email.Font = Font;
        _email.Height = 44;

        _password.UseSystemPasswordChar = true;
        _password.Font = Font;
        _password.Height = 44;

        _error.ForeColor = UiTheme.ErrorText;
        _error.Text = string.Empty;
        _error.AutoSize = false;
        _error.Height = 52;
        _error.TextAlign = ContentAlignment.TopLeft;

        _login.Text = "Sign in";
        _login.Height = 52;
        _login.BackColor = UiTheme.HeaderBlue;
        _login.ForeColor = Color.White;
        _login.FlatStyle = FlatStyle.Flat;
        _login.FlatAppearance.BorderSize = 0;
        _login.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
        _login.Click += async (_, _) => await LoginAsync();

        _register.Text = "Employee Registration";
        _register.Height = 48;
        _register.FlatStyle = FlatStyle.Flat;
        _register.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        _register.Click += (_, _) =>
        {
            using var reg = new RegisterForm(_registration);
            reg.ShowDialog(this);
        };

        var hint = new Label
        {
            Text = "Demo account: vet@clinicvets.com / Vet123!",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 0)
        };

        _flow.Controls.Add(lblEmail);
        _flow.Controls.Add(_email);
        _flow.Controls.Add(lblPassword);
        _flow.Controls.Add(_password);
        _flow.Controls.Add(_error);
        _flow.Controls.Add(_login);
        _flow.Controls.Add(_register);
        _flow.Controls.Add(hint);

        _card.Controls.Add(_flow);
        _body.Controls.Add(_card);
        Controls.Add(_body);
        Controls.Add(header);

        AcceptButton = _login;
        Resize += (_, _) => Relayout();
        Shown += (_, _) =>
        {
            WindowState = FormWindowState.Maximized;
            Relayout();
            SyncFlowChildWidths();
        };
    }

    private static Label HeaderLabel(string text) =>
        new()
        {
            Text = text,
            ForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6)
        };

    private void SyncFlowChildWidths()
    {
        var inner = Math.Max(280, _flow.ClientSize.Width - _flow.Padding.Horizontal);
        foreach (Control c in _flow.Controls)
        {
            if (c is Label { AutoSize: true } && c != _error)
                continue;
            c.Width = inner;
        }
    }

    private void Relayout()
    {
        ResponsiveLayout.CenterCard(_body, _card, horizontalPadding: 48, maxCardWidth: 580, topOffset: 56, bottomPadding: 56);
        SyncFlowChildWidths();
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
