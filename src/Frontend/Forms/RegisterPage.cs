using System.Drawing;
using System.Drawing.Drawing2D;
using ClinicVets.Application.Services;
using ClinicVets.Desktop;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public sealed class RegisterPage : UserControl
{
    private readonly EmployeeRegistrationService _registration;
    private readonly MainShellForm _shell;
    private readonly Panel _rightHost = new() { Dock = DockStyle.Fill };
    private readonly Panel _body = new();
    private readonly ModernCardPanel _card = new() { Padding = new Padding(4) };
    private readonly FlowLayoutPanel _flow = new();
    private readonly TextBox _fullName = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new();
    private readonly ComboBox _role = new();
    private readonly ModernTextField _fullNameHost;
    private readonly ModernTextField _emailHost;
    private readonly ModernTextField _passwordHost;
    private readonly RoundedComboHost _roleHost;
    private readonly ModernAlertBanner _feedback = new();
    private readonly ModernButton _save = new();
    private readonly ModernOutlineButton _cancel = new();
    private readonly Label _heroTitle;
    private readonly Label _heroSubtitle;

    public Button BackButton => _cancel;

    public RegisterPage(EmployeeRegistrationService registration, MainShellForm shell)
    {
        _registration = registration;
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
        brandTable.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
        brandTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        brandTable.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));

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
            Text = "Join the clinic team",
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

        _card.BackColor = Color.Transparent;

        _flow.Dock = DockStyle.Fill;
        _flow.FlowDirection = FlowDirection.TopDown;
        _flow.WrapContents = false;
        _flow.AutoScroll = true;
        _flow.Padding = new Padding(44, 36, 44, 36);
        _flow.BackColor = UiTheme.CardWhite;
        _flow.SizeChanged += (_, _) => SyncWidths();

        _heroTitle = UiStyles.CreateHeroTitle("New employee");
        _heroSubtitle = UiStyles.CreateHeroSubtitle(
            "All fields are required. Passwords must meet clinic security rules. " +
            "An administrator must approve your account before you can sign in.");

        _fullName.PlaceholderText = "Full name (e.g. Dr. Jane Doe)";
        _email.PlaceholderText = "Work email (used to sign in)";
        _password.PlaceholderText = "8–10 characters: letter, digit, and special character";
        _password.UseSystemPasswordChar = true;

        _fullNameHost = new ModernTextField(_fullName);
        _emailHost = new ModernTextField(_email);
        _passwordHost = new ModernTextField(_password, showPasswordRevealToggle: true);

        _role.DropDownStyle = ComboBoxStyle.DropDownList;
        _role.Items.AddRange(new object[] { "Secretary", "Veterinarian" });
        UiStyles.ApplyComboInner(_role);
        _roleHost = new RoundedComboHost(_role);
        _role.SelectedIndex = 0;

        _feedback.Clear();

        _save.Text = "Save employee";
        _save.Margin = new Padding(0, 14, 0, 0);
        _save.Click += async (_, _) => await SaveAsync();

        _cancel.Text = "Back to sign in";
        _cancel.Margin = new Padding(0, 8, 0, 0);
        _cancel.Click += (_, _) => _shell.NavigateToLogin();

        var buttonRow = new TableLayoutPanel
        {
            ColumnCount = 2,
            Height = UiTheme.PrimaryButtonHeight + 12,
            Margin = new Padding(0, 10, 0, 0)
        };
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttonRow.Controls.Add(_save, 0, 0);
        buttonRow.Controls.Add(_cancel, 1, 0);
        _save.Dock = DockStyle.Fill;
        _cancel.Dock = DockStyle.Fill;
        _save.Margin = new Padding(0, 0, 10, 0);
        _cancel.Margin = new Padding(10, 0, 0, 0);

        var hint = new Label
        {
            Text =
                "Tip: choose a memorable password that still meets the rules above. " +
                "You will be able to sign in only after an administrator approves your registration and assigns your Employee ID.",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = false,
            Height = 56,
            TextAlign = ContentAlignment.TopCenter,
            Margin = new Padding(0, 14, 0, 0)
        };

        _flow.Controls.Add(_heroTitle);
        _flow.Controls.Add(_heroSubtitle);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Full name"));
        _flow.Controls.Add(_fullNameHost);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Email"));
        _flow.Controls.Add(_emailHost);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Password"));
        _flow.Controls.Add(_passwordHost);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Role"));
        _flow.Controls.Add(_roleHost);
        _flow.Controls.Add(_feedback);
        _flow.Controls.Add(buttonRow);
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

    private void SyncWidths()
    {
        var inner = Math.Max(320, _flow.ClientSize.Width - _flow.Padding.Horizontal);
        foreach (Control c in _flow.Controls)
        {
            if (c is TableLayoutPanel row)
            {
                row.Width = inner;
                continue;
            }

            if (c is Label { AutoSize: true } lbl && c != _heroTitle && c != _heroSubtitle)
                continue;

            c.Width = inner;
        }
    }

    private void Relayout()
    {
        ResponsiveLayout.CenterCard(_body, _card, 40, 640, 36, 40);
        SyncWidths();
    }

    private async Task SaveAsync()
    {
        _save.Enabled = false;
        try
        {
            _feedback.Clear();
            var roleText = _role.SelectedIndex >= 0 ? _role.SelectedItem?.ToString() ?? string.Empty : string.Empty;
            var result = await _registration.RegisterAsync(_fullName.Text, _email.Text, _password.Text, roleText);
            if (!result.IsSuccess)
            {
                _feedback.ShowMessage(
                    UiFeedbackKind.Error,
                    "Registration could not be completed" + Environment.NewLine + Environment.NewLine + result.Message);
                return;
            }

            var owner = FindForm();
            MessageBox.Show(
                owner,
                result.Message + Environment.NewLine + Environment.NewLine + "Return to the sign-in screen once an administrator has approved the account.",
                "Success — ClinicVets",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            _shell.NavigateToLogin();
        }
        finally
        {
            if (_save.IsHandleCreated)
                _save.Enabled = true;
        }
    }
}
