using ClinicVets.Application.Services;
using ClinicVets.Desktop;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Employee registration — maximized layout with centered responsive form card.
/// </summary>
public class RegisterForm : Form
{
    private readonly EmployeeRegistrationService _registration;
    private readonly Panel _body = new();
    private readonly Panel _card = new();
    private readonly FlowLayoutPanel _flow = new();
    private readonly TextBox _fullName = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new();
    private readonly ComboBox _role = new();
    private readonly Label _error = new();
    private readonly Button _save = new();
    private readonly Button _cancel = new();
    private readonly Label _heroTitle;
    private readonly Label _heroSubtitle;

    public RegisterForm(EmployeeRegistrationService registration)
    {
        _registration = registration;

        Text = "ClinicVets — Employee Registration";
        Icon = AppBranding.CreateWindowIcon();
        MinimumSize = new Size(960, 640);
        MaximizeBox = true;
        MinimizeBox = true;
        BackColor = UiTheme.PageBackground;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;

        var header = new Panel
        {
            Height = 78,
            Dock = DockStyle.Top,
            BackColor = UiTheme.HeaderBlue
        };
        header.Controls.Add(new Label
        {
            Text = "ClinicVets",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(40, 14)
        });
        header.Controls.Add(new Label
        {
            Text = "Employee onboarding",
            ForeColor = UiTheme.SubtitleOnHeader,
            AutoSize = true,
            Location = new Point(40, 46),
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point)
        });

        _body.Dock = DockStyle.Fill;
        _body.BackColor = UiTheme.PageBackground;
        _body.Resize += (_, _) => Relayout();

        _card.BackColor = Color.Transparent;
        _card.Paint += (_, e) => UiChrome.PaintCardWithShadow(_card, e, UiTheme.CardCornerRadius);

        _flow.Dock = DockStyle.Fill;
        _flow.FlowDirection = FlowDirection.TopDown;
        _flow.WrapContents = false;
        _flow.AutoScroll = true;
        _flow.Padding = new Padding(44, 36, 44, 36);
        _flow.BackColor = Color.Transparent;
        _flow.SizeChanged += (_, _) => SyncWidths();

        _heroTitle = UiStyles.CreateHeroTitle("Create employee account");
        _heroSubtitle = UiStyles.CreateHeroSubtitle("Complete the fields below to add a colleague");

        UiStyles.ApplyTextBox(_fullName);
        UiStyles.ApplyTextBox(_email);
        _password.UseSystemPasswordChar = true;
        UiStyles.ApplyTextBox(_password);

        _role.DropDownStyle = ComboBoxStyle.DropDownList;
        _role.Items.AddRange(new object[] { "Veterinarian", "Secretary", "Administrator" });
        UiStyles.ApplyComboBox(_role);

        _error.ForeColor = UiTheme.ErrorText;
        _error.Text = string.Empty;
        _error.AutoSize = false;
        _error.Height = 56;
        _error.TextAlign = ContentAlignment.TopLeft;
        _error.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        _error.Margin = new Padding(0, 4, 0, 0);

        _save.Text = "Save employee";
        UiStyles.ApplyPrimaryButton(_save);
        _save.Click += async (_, _) => await SaveAsync();

        _cancel.Text = "Back to sign in";
        UiStyles.ApplySecondaryButton(_cancel);
        _cancel.DialogResult = DialogResult.Cancel;

        var buttonRow = new TableLayoutPanel
        {
            ColumnCount = 2,
            Height = UiTheme.PrimaryButtonHeight + 8,
            Margin = new Padding(0, 12, 0, 0)
        };
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttonRow.Controls.Add(_save, 0, 0);
        buttonRow.Controls.Add(_cancel, 1, 0);
        _save.Dock = DockStyle.Fill;
        _cancel.Dock = DockStyle.Fill;
        _save.Margin = new Padding(0, 0, 8, 0);
        _cancel.Margin = new Padding(8, 0, 0, 0);

        var hint = new Label
        {
            Text = "Password: 8–10 characters with a letter, digit, and special character.",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = false,
            Height = 40,
            TextAlign = ContentAlignment.TopCenter,
            Margin = new Padding(0, 8, 0, 0)
        };

        _flow.Controls.Add(_heroTitle);
        _flow.Controls.Add(_heroSubtitle);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Full name"));
        _flow.Controls.Add(_fullName);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Email"));
        _flow.Controls.Add(_email);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Password"));
        _flow.Controls.Add(_password);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Role"));
        _flow.Controls.Add(_role);
        _flow.Controls.Add(_error);
        _flow.Controls.Add(buttonRow);
        _flow.Controls.Add(hint);

        _card.Controls.Add(_flow);
        _body.Controls.Add(_card);
        Controls.Add(_body);
        Controls.Add(header);

        CancelButton = _cancel;
        Resize += (_, _) => Relayout();
        Shown += (_, _) =>
        {
            WindowState = FormWindowState.Maximized;
            Relayout();
            SyncWidths();
        };
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

            if (c is Label { AutoSize: true } lbl && lbl != _error && lbl != _heroTitle && lbl != _heroSubtitle)
                continue;

            c.Width = inner;
        }
    }

    private void Relayout()
    {
        ResponsiveLayout.CenterCard(_body, _card, 48, 640, 40, 48);
        SyncWidths();
    }

    private async Task SaveAsync()
    {
        _error.Text = string.Empty;
        var roleText = _role.SelectedIndex >= 0 ? _role.SelectedItem?.ToString() ?? string.Empty : string.Empty;
        var result = await _registration.RegisterAsync(_fullName.Text, _email.Text, _password.Text, roleText);
        if (!result.IsSuccess)
        {
            _error.Text = result.Message;
            return;
        }

        MessageBox.Show(this, result.Message, "ClinicVets", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }
}
