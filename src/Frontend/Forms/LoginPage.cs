using System.Drawing;
using System.Drawing.Drawing2D;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public sealed class LoginPage : UserControl
{
    private readonly EmployeeAuthenticationService _auth;
    private readonly MainShellForm _shell;
    private readonly Panel _rightHost = new() { Dock = DockStyle.Fill };
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
    private readonly ModernOutlineButton _demoMode = new();
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

        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.PageBackground
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));

        var left = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.HeaderPrimary };
        left.Paint += PaintBrandPanel;

        var brandTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.HeaderPrimary,
            Padding = new Padding(28, 0, 28, 0)
        };
        brandTable.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        brandTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        brandTable.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));

        var brandCell = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = UiTheme.HeaderPrimary,
            Padding = new Padding(8, 8, 8, 8)
        };

        var logo = new PictureBox
        {
            Size = new Size(72, 72),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = UiTheme.HeaderPrimary,
            Margin = new Padding(0, 0, 0, 16)
        };
        try
        {
            logo.Image = AppBranding.GetHeaderImage();
        }
        catch
        {
            logo.Visible = false;
        }

        var clinic = new Label
        {
            Text = "ClinicVets",
            Font = new Font("Segoe UI", 28F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8),
            BackColor = UiTheme.HeaderPrimary
        };
        var tag = new Label
        {
            Text = "Veterinary clinic system",
            Font = new Font("Segoe UI", 12.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.SubtitleOnHeader,
            AutoSize = true,
            MaximumSize = new Size(320, 0),
            BackColor = UiTheme.HeaderPrimary
        };

        if (logo.Visible)
            brandCell.Controls.Add(logo);
        brandCell.Controls.Add(clinic);
        brandCell.Controls.Add(tag);

        brandTable.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.HeaderPrimary }, 0, 0);
        brandTable.Controls.Add(brandCell, 0, 1);
        brandTable.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.HeaderPrimary }, 0, 2);
        left.Controls.Add(brandTable);

        _rightHost.BackColor = UiTheme.PageBackground;
        _rightHost.Paint += PaintBodyGradient;

        _body.Dock = DockStyle.Fill;
        _body.BackColor = Color.Transparent;
        _body.Resize += (_, _) => Relayout();

        _card.BackColor = UiTheme.CardWhite;
        _card.Paint += (_, e) => UiChrome.PaintCardWithShadow(_card, e, UiTheme.CardCornerRadius);

        _flow.Dock = DockStyle.Fill;
        _flow.FlowDirection = FlowDirection.TopDown;
        _flow.WrapContents = false;
        _flow.AutoScroll = true;
        _flow.Padding = new Padding(44, 40, 44, 40);
        _flow.BackColor = UiTheme.CardWhite;
        _flow.SizeChanged += (_, _) => SyncFlowChildWidths();

        _heroTitle = UiStyles.CreateHeroTitle("Welcome back");
        _heroSubtitle = UiStyles.CreateHeroSubtitle("Sign in with your clinic email (or username) and password");

        _email.PlaceholderText = "Email or username (e.g. admin or name@clinicvets.com)";
        _password.PlaceholderText = "Your password";
        _password.UseSystemPasswordChar = true;

        _emailHost = new RoundedInputHost(_email);
        _passwordHost = new RoundedInputHost(_password, showPasswordRevealToggle: true);

        _feedback.Clear();

        _login.Text = "Sign in";
        _login.Margin = new Padding(0, 18, 0, 10);
        _login.Click += async (_, _) => await LoginAsync();

        _register.Text = "Register new employee";
        _register.Margin = new Padding(0, 4, 0, 0);
        _register.Click += (_, _) => _shell.NavigateToRegister();

        _demoMode.Text = "Enter Demo Mode";
        _demoMode.Margin = new Padding(0, 10, 0, 0);
        _demoMode.Click += (_, _) => _shell.NavigateToDemo();
        _demoMode.Visible = DesktopBuildOptions.EnableDemoMode;

        var hint = _demoHint;
        hint.Text =
            "Default administrator: admin  ·  Admin123!" + Environment.NewLine +
            "Demo staff: vet@clinicvets.com  ·  Vet12!ab   |   secretary@clinicvets.com  ·  Sec12!ab";
        hint.ForeColor = UiTheme.SuccessText;
        hint.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        hint.AutoSize = false;
        hint.TextAlign = ContentAlignment.MiddleCenter;
        hint.Margin = new Padding(0, 20, 0, 0);
        hint.Height = 56;
        hint.BackColor = UiTheme.SuccessBackground;

        _flow.Controls.Add(_heroTitle);
        _flow.Controls.Add(_heroSubtitle);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Email or username"));
        _flow.Controls.Add(_emailHost);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Password"));
        _flow.Controls.Add(_passwordHost);
        _flow.Controls.Add(_feedback);
        _flow.Controls.Add(_login);
        _flow.Controls.Add(_register);
        if (DesktopBuildOptions.EnableDemoMode)
            _flow.Controls.Add(_demoMode);
        _flow.Controls.Add(hint);

        _card.Controls.Add(_flow);
        _body.Controls.Add(_card);
        _rightHost.Controls.Add(_body);

        split.Controls.Add(left, 0, 0);
        split.Controls.Add(_rightHost, 1, 0);
        Controls.Add(split);

        Resize += (_, _) => Relayout();
        Load += (_, _) => Relayout();
    }

    private static void PaintBrandPanel(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel p)
            return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var brush = new LinearGradientBrush(
            p.ClientRectangle,
            UiTheme.HeaderPrimaryDark,
            UiTheme.HeaderPrimary,
            LinearGradientMode.Vertical);
        g.FillRectangle(brush, p.ClientRectangle);
    }

    private static void PaintBodyGradient(object? sender, PaintEventArgs e)
    {
        if (sender is not Control c)
            return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var brush = new LinearGradientBrush(
            c.ClientRectangle,
            UiTheme.PageBackground,
            UiTheme.PageGradientBottom,
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
        ResponsiveLayout.CenterCard(_body, _card, horizontalPadding: 40, maxCardWidth: 520, topOffset: 40, bottomPadding: 40);
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
