using System.Drawing.Drawing2D;
using ClinicVets.Application.Services;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public sealed class LoginPage : UserControl
{
    private readonly EmployeeAuthenticationService _auth;
    private readonly MainShellForm _shell;
    private readonly Panel _body = new();
    private readonly Panel _card = new();
    private readonly FlowLayoutPanel _flow = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new();
    private readonly RoundedInputHost _emailHost;
    private readonly RoundedInputHost _passwordHost;
    private readonly FeedbackBannerPanel _feedback = new();
    private readonly ModernPrimaryButton _login = new();
    private readonly ModernOutlineButton _register = new();
    private readonly Label _heroTitle;
    private readonly Label _heroSubtitle;
    private readonly Label _demoHint = new();

    public Button SubmitButton => _login;

    public LoginPage(EmployeeAuthenticationService auth, MainShellForm shell)
    {
        _auth = auth;
        _shell = shell;

        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
        BackColor = UiTheme.PageBackground;
        Font = shell.Font;

        var header = UiHeaderBar.Create("Veterinary clinic management — employee sign in");

        _body.Dock = DockStyle.Fill;
        _body.BackColor = Color.Transparent;
        _body.Resize += (_, _) => Relayout();

        _card.BackColor = Color.Transparent;
        _card.Paint += (_, e) => UiChrome.PaintCardWithShadow(_card, e, UiTheme.CardCornerRadius);

        _flow.Dock = DockStyle.Fill;
        _flow.FlowDirection = FlowDirection.TopDown;
        _flow.WrapContents = false;
        _flow.AutoScroll = true;
        _flow.Padding = new Padding(52, 48, 52, 48);
        _flow.BackColor = Color.Transparent;
        _flow.SizeChanged += (_, _) => SyncFlowChildWidths();

        _heroTitle = UiStyles.CreateHeroTitle("Welcome back");
        _heroSubtitle = UiStyles.CreateHeroSubtitle("Sign in with your clinic email and password");

        _email.PlaceholderText = "Work email (e.g. name@clinicvets.com)";
        _password.PlaceholderText = "Your password";
        _password.UseSystemPasswordChar = true;

        _emailHost = new RoundedInputHost(_email);
        _passwordHost = new RoundedInputHost(_password);

        _feedback.Clear();

        _login.Text = "Sign in";
        _login.Margin = new Padding(0, 18, 0, 10);
        _login.Click += async (_, _) => await LoginAsync();

        _register.Text = "Register new employee";
        _register.Margin = new Padding(0, 4, 0, 0);
        _register.Click += (_, _) => _shell.NavigateToRegister();

        var hint = _demoHint;
        hint.Text = "Demo account: vet@clinicvets.com  ·  Vet12!ab";
        hint.ForeColor = UiTheme.SuccessText;
        hint.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        hint.AutoSize = false;
        hint.TextAlign = ContentAlignment.MiddleCenter;
        hint.Margin = new Padding(0, 20, 0, 0);
        hint.Height = 44;
        hint.BackColor = Color.FromArgb(232, 246, 238);

        _flow.Controls.Add(_heroTitle);
        _flow.Controls.Add(_heroSubtitle);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Email"));
        _flow.Controls.Add(_emailHost);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Password"));
        _flow.Controls.Add(_passwordHost);
        _flow.Controls.Add(_feedback);
        _flow.Controls.Add(_login);
        _flow.Controls.Add(_register);
        _flow.Controls.Add(hint);

        _card.Controls.Add(_flow);
        _body.Controls.Add(_card);

        var root = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.PageBackground };
        root.Paint += PaintBodyGradient;
        root.Controls.Add(_body);
        root.Controls.Add(header);

        Controls.Add(root);

        Resize += (_, _) => Relayout();
        Load += (_, _) => Relayout();
    }

    private static void PaintBodyGradient(object? sender, PaintEventArgs e)
    {
        if (sender is not Control c)
            return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var top = UiTheme.PageBackground;
        var bottom = Color.FromArgb(232, 242, 238);
        using var brush = new LinearGradientBrush(
            c.ClientRectangle,
            top,
            bottom,
            LinearGradientMode.Vertical);
        g.FillRectangle(brush, c.ClientRectangle);
    }

    private void SyncFlowChildWidths()
    {
        var inner = Math.Max(300, _flow.ClientSize.Width - _flow.Padding.Horizontal);
        foreach (Control ctrl in _flow.Controls)
        {
            if (ctrl is Label { AutoSize: true } lbl && ctrl != _heroTitle && ctrl != _heroSubtitle && ctrl != _demoHint)
                continue;
            ctrl.Width = inner;
        }
    }

    private void Relayout()
    {
        ResponsiveLayout.CenterCard(_body, _card, horizontalPadding: 56, maxCardWidth: 520, topOffset: 48, bottomPadding: 56);
        SyncFlowChildWidths();
    }

    private void ClearFeedback() => _feedback.Clear();

    private async Task LoginAsync()
    {
        ClearFeedback();
        _login.Enabled = false;
        try
        {
            var result = await _auth.LoginAsync(_email.Text, _password.Text);
            if (!result.IsSuccess)
            {
                _feedback.ShowMessage(
                    UiFeedbackKind.Error,
                    "Unable to sign in" + Environment.NewLine + Environment.NewLine + result.Message);
                return;
            }

            _password.Clear();
            ClearFeedback();
            _shell.NavigateToDashboard(result.Employee!);
        }
        finally
        {
            if (_login.IsHandleCreated)
                _login.Enabled = true;
        }
    }
}
