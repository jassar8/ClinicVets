using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Administrator-only flow for creating any clinic role, including additional admins.
/// </summary>
public sealed class AdminCreateEmployeeForm : Form
{
    private readonly Employee _actingAdmin;
    private readonly EmployeeRegistrationService _registration;
    private readonly TextBox _fullName = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new();
    private readonly TextBox _username = new();
    private readonly ComboBox _role = new();
    private readonly RoundedInputHost _fullNameHost;
    private readonly RoundedInputHost _emailHost;
    private readonly RoundedInputHost _passwordHost;
    private readonly RoundedInputHost _usernameHost;
    private readonly RoundedComboHost _roleHost;
    private readonly FeedbackBannerPanel _feedback = new();
    private readonly ModernPrimaryButton _save = new();
    private readonly ModernOutlineButton _cancel = new();

    public AdminCreateEmployeeForm(Employee actingAdmin, EmployeeRegistrationService registration)
    {
        _actingAdmin = actingAdmin;
        _registration = registration;

        Text = "ClinicVets — add employee (administrator)";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = UiTheme.PageBackground;
        ClientSize = new Size(540, 620);
        Padding = new Padding(28);

        _fullName.PlaceholderText = "Full name";
        _email.PlaceholderText = "Work email (sign-in)";
        _password.PlaceholderText = "Temporary password (8–10 chars, letter, digit, special)";
        _password.UseSystemPasswordChar = true;
        _username.PlaceholderText = "Optional username (e.g. jdoe)";

        _fullNameHost = new RoundedInputHost(_fullName);
        _emailHost = new RoundedInputHost(_email);
        _passwordHost = new RoundedInputHost(_password);
        _usernameHost = new RoundedInputHost(_username);

        _role.DropDownStyle = ComboBoxStyle.DropDownList;
        _role.Items.AddRange(new object[] { EmployeeRoleNames.Admin, EmployeeRoleNames.Secretary, EmployeeRoleNames.Veterinarian });
        _role.SelectedIndex = 1;
        UiStyles.ApplyComboInner(_role);
        _roleHost = new RoundedComboHost(_role);

        _feedback.Clear();

        _save.Text = "Create account";
        _save.Margin = new Padding(0, 16, 12, 0);
        _save.Click += async (_, _) => await SaveAsync();

        _cancel.Text = "Cancel";
        _cancel.Margin = new Padding(0, 16, 0, 0);
        _cancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(4),
            BackColor = Color.Transparent
        };
        flow.SizeChanged += (_, _) =>
        {
            var inner = Math.Max(320, flow.ClientSize.Width - flow.Padding.Horizontal);
            foreach (Control c in flow.Controls)
            {
                if (c is FlowLayoutPanel row && row.FlowDirection == FlowDirection.LeftToRight)
                {
                    row.Width = inner;
                    continue;
                }

                if (c is Label { AutoSize: true })
                    continue;
                c.Width = inner;
            }
        };

        var title = UiStyles.CreateHeroTitle("New employee account");
        var subtitle = UiStyles.CreateHeroSubtitle(
            "Administrators can assign any role. Passwords must follow the same clinic rules as self-registration.");

        var buttonRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 8, 0, 0),
            BackColor = Color.Transparent
        };
        buttonRow.Controls.Add(_save);
        buttonRow.Controls.Add(_cancel);

        flow.Controls.Add(title);
        flow.Controls.Add(subtitle);
        flow.Controls.Add(UiStyles.CreateFieldCaption("Full name"));
        flow.Controls.Add(_fullNameHost);
        flow.Controls.Add(UiStyles.CreateFieldCaption("Email"));
        flow.Controls.Add(_emailHost);
        flow.Controls.Add(UiStyles.CreateFieldCaption("Password"));
        flow.Controls.Add(_passwordHost);
        flow.Controls.Add(UiStyles.CreateFieldCaption("Username (optional)"));
        flow.Controls.Add(_usernameHost);
        flow.Controls.Add(UiStyles.CreateFieldCaption("Role"));
        flow.Controls.Add(_roleHost);
        flow.Controls.Add(_feedback);
        flow.Controls.Add(buttonRow);

        Controls.Add(flow);

        Shown += (_, _) => ActiveControl = _fullName;
    }

    private async Task SaveAsync()
    {
        _save.Enabled = false;
        try
        {
            _feedback.Clear();
            var roleText = _role.SelectedItem?.ToString() ?? string.Empty;
            var username = string.IsNullOrWhiteSpace(_username.Text) ? null : _username.Text.Trim();
            var result = await _registration.RegisterAsync(
                _fullName.Text,
                _email.Text,
                _password.Text,
                roleText,
                _actingAdmin,
                username);

            if (!result.IsSuccess)
            {
                _feedback.ShowMessage(
                    UiFeedbackKind.Error,
                    "Unable to create the account" + Environment.NewLine + Environment.NewLine + result.Message);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            if (_save.IsHandleCreated)
                _save.Enabled = true;
        }
    }
}
