using ClinicVets.Application.Services;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// New employee registration — saves via application service to local JSON store.
/// </summary>
public class RegisterForm : Form
{
    private readonly EmployeeRegistrationService _registration;
    private readonly TextBox _fullName = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new();
    private readonly ComboBox _role = new();
    private readonly Label _error = new();

    public RegisterForm(EmployeeRegistrationService registration)
    {
        _registration = registration;
        Text = "ClinicVets — Employee Registration";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(440, 480);
        BackColor = UiTheme.PageBackground;
        Font = new Font("Segoe UI", 10F);

        var header = new Panel
        {
            Height = 64,
            Dock = DockStyle.Top,
            BackColor = UiTheme.HeaderBlue
        };
        header.Controls.Add(new Label
        {
            Text = "Create Employee Account",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        });

        var card = new Panel
        {
            Location = new Point(28, 84),
            Size = new Size(384, 360),
            BackColor = UiTheme.CardWhite
        };

        var y = 16;
        void AddRow(string caption, Control field, int fieldHeight = 28)
        {
            card.Controls.Add(new Label { Text = caption, Location = new Point(20, y), AutoSize = true });
            y += 22;
            field.Location = new Point(20, y);
            field.Width = 344;
            field.Height = fieldHeight;
            card.Controls.Add(field);
            y += fieldHeight + 14;
        }

        AddRow("Full Name", _fullName);
        AddRow("Email", _email);
        _password.UseSystemPasswordChar = true;
        AddRow("Password", _password);

        card.Controls.Add(new Label { Text = "Role", Location = new Point(20, y), AutoSize = true });
        y += 22;
        _role.DropDownStyle = ComboBoxStyle.DropDownList;
        _role.Items.AddRange(new object[] { "Veterinarian", "Secretary", "Administrator" });
        _role.Location = new Point(20, y);
        _role.Width = 344;
        _role.Height = 28;
        card.Controls.Add(_role);
        y += 42;

        _error.Location = new Point(20, y);
        _error.Size = new Size(344, 52);
        _error.ForeColor = UiTheme.ErrorText;
        y += 60;

        var save = new Button
        {
            Text = "Register",
            Location = new Point(20, y),
            Size = new Size(160, 34),
            BackColor = UiTheme.HeaderBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        save.FlatAppearance.BorderSize = 0;
        save.Click += async (_, _) => await SaveAsync();

        var cancel = new Button
        {
            Text = "Close",
            Location = new Point(204, y),
            Size = new Size(160, 34),
            DialogResult = DialogResult.Cancel
        };

        card.Controls.AddRange(new Control[] { _error, save, cancel });
        Controls.Add(card);
        Controls.Add(header);
        CancelButton = cancel;
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
