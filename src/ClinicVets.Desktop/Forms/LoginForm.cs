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
    private readonly Label _heroTitle;
    private readonly Label _heroSubtitle;
    private readonly Label _demoHint = new();

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

        var header = UiHeaderBar.Create("Veterinary clinic management — employee sign in");

        _body.Dock = DockStyle.Fill;
        _body.BackColor = UiTheme.PageBackground;
        _body.Resize += (_, _) => Relayout();

        _card.BackColor = Color.Transparent;
        _card.Paint += (_, e) => UiChrome.PaintCardWithShadow(_card, e, UiTheme.CardCornerRadius);

        _flow.Dock = DockStyle.Fill;
        _flow.FlowDirection = FlowDirection.TopDown;
        _flow.WrapContents = false;
        _flow.AutoScroll = true;
        _flow.Padding = new Padding(48, 44, 48, 44);
        _flow.BackColor = Color.Transparent;
        _flow.SizeChanged += (_, _) => SyncFlowChildWidths();

        _heroTitle = UiStyles.CreateHeroTitle("Welcome back");
        _heroSubtitle = UiStyles.CreateHeroSubtitle("Sign in with your clinic email and password");

        _email.PlaceholderText = "Work email (e.g. name@clinicvets.com)";
        UiStyles.ApplyTextBox(_email);

        _password.PlaceholderText = "Your password";
        _password.UseSystemPasswordChar = true;
        UiStyles.ApplyTextBox(_password);

        UiStyles.ApplyFeedbackLabel(_error, UiFeedbackKind.None);
        _error.Text = string.Empty;
        _error.Height = 60;
        _error.Margin = new Padding(0, 8, 0, 0);

        _login.Text = "Sign in";
        UiStyles.ApplyPrimaryButton(_login);
        _login.Margin = new Padding(0, 16, 0, 8);
        _login.Click += async (_, _) => await LoginAsync();

        _register.Text = "Register new employee";
        UiStyles.ApplySecondaryButton(_register);
        _register.Margin = new Padding(0, 4, 0, 0);
        _register.Click += (_, _) =>
        {
            using var reg = new RegisterForm(_registration);
            reg.ShowDialog(this);
        };

        var hint = _demoHint;
        hint.Text = "Demo: vet@clinicvets.com  ·  Vet12!ab";
        hint.ForeColor = UiTheme.SuccessText;
        hint.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        hint.AutoSize = false;
        hint.TextAlign = ContentAlignment.MiddleCenter;
        hint.Margin = new Padding(0, 18, 0, 0);
        hint.Height = 40;
        hint.BackColor = Color.FromArgb(236, 246, 241);

        _flow.Controls.Add(_heroTitle);
        _flow.Controls.Add(_heroSubtitle);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Email"));
        _flow.Controls.Add(_email);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Password"));
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

    private void SyncFlowChildWidths()
    {
        var inner = Math.Max(300, _flow.ClientSize.Width - _flow.Padding.Horizontal);
        foreach (Control c in _flow.Controls)
        {
            if (c is Label { AutoSize: true } lbl && lbl != _error && lbl != _heroTitle && lbl != _heroSubtitle)
                continue;
            c.Width = inner;
        }
    }

    private void Relayout()
    {
        ResponsiveLayout.CenterCard(_body, _card, horizontalPadding: 52, maxCardWidth: 560, topOffset: 44, bottomPadding: 52);
        SyncFlowChildWidths();
    }

    private void ClearFeedback()
    {
        _error.Text = string.Empty;
        UiStyles.ApplyFeedbackLabel(_error, UiFeedbackKind.None);
    }

    private async Task LoginAsync()
    {
        ClearFeedback();
        _login.Enabled = false;
        try
        {
            var result = await _auth.LoginAsync(_email.Text, _password.Text);
            if (!result.IsSuccess)
            {
                _error.Text = result.Message;
                UiStyles.ApplyFeedbackLabel(_error, UiFeedbackKind.Error);
                return;
            }

            Hide();
            using var dash = new DashboardForm(result.Employee!);
            dash.ShowDialog();
            _password.Clear();
            ClearFeedback();
            Show();
        }
        finally
        {
            _login.Enabled = true;
        }
    }
}
