using System.Drawing.Drawing2D;
using ClinicVets.Application.Services;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public sealed class RegisterPage : UserControl
{
    private readonly EmployeeRegistrationService _registration;
    private readonly MainShellForm _shell;
    private readonly Panel _body = new();
    private readonly Panel _card = new();
    private readonly FlowLayoutPanel _flow = new();
    private readonly TextBox _fullName = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new();
    private readonly ComboBox _role = new();
    private readonly RoundedInputHost _fullNameHost;
    private readonly RoundedInputHost _emailHost;
    private readonly RoundedInputHost _passwordHost;
    private readonly RoundedComboHost _roleHost;
    private readonly FeedbackBannerPanel _feedback = new();
    private readonly ModernPrimaryButton _save = new();
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

        var header = UiHeaderBar.Create("Add a staff member to your clinic workspace");

        _body.Dock = DockStyle.Fill;
        _body.BackColor = UiTheme.PageBackground;
        _body.Resize += (_, _) => Relayout();

        _card.BackColor = UiTheme.PageBackground;
        _card.Paint += (_, e) => UiChrome.PaintCardWithShadow(_card, e, UiTheme.CardCornerRadius);

        _flow.Dock = DockStyle.Fill;
        _flow.FlowDirection = FlowDirection.TopDown;
        _flow.WrapContents = false;
        _flow.AutoScroll = true;
        _flow.Padding = new Padding(52, 44, 52, 44);
        _flow.BackColor = UiTheme.PageBackground;
        _flow.SizeChanged += (_, _) => SyncWidths();

        _heroTitle = UiStyles.CreateHeroTitle("New employee");
        _heroSubtitle = UiStyles.CreateHeroSubtitle(
            "All fields are required. Passwords must meet clinic security rules. " +
            "An administrator must approve your account before you can sign in.");

        _fullName.PlaceholderText = "Full name (e.g. Dr. Jane Doe)";
        _email.PlaceholderText = "Work email (used to sign in)";
        _password.PlaceholderText = "8–10 characters: letter, digit, and special character";
        _password.UseSystemPasswordChar = true;

        _fullNameHost = new RoundedInputHost(_fullName);
        _emailHost = new RoundedInputHost(_email);
        _passwordHost = new RoundedInputHost(_password, showPasswordRevealToggle: true);

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
        using var brush = new LinearGradientBrush(
            c.ClientRectangle,
            UiTheme.PageBackground,
            Color.FromArgb(232, 242, 238),
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
        ResponsiveLayout.CenterCard(_body, _card, 56, 640, 48, 56);
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
