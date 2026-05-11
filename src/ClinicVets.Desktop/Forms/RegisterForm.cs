using ClinicVets.Application.Services;
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

    public RegisterForm(EmployeeRegistrationService registration)
    {
        _registration = registration;

        Text = "ClinicVets — Employee Registration";
        MinimumSize = new Size(960, 640);
        MaximizeBox = true;
        MinimizeBox = true;
        BackColor = UiTheme.PageBackground;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;

        var header = new Panel
        {
            Height = 96,
            Dock = DockStyle.Top,
            BackColor = UiTheme.HeaderBlue
        };
        header.Controls.Add(new Label
        {
            Text = "Create Employee Account",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(40, 28)
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
        _flow.SizeChanged += (_, _) => SyncWidths();

        _fullName.Height = 44;
        _fullName.Font = Font;
        _email.Height = 44;
        _email.Font = Font;
        _password.UseSystemPasswordChar = true;
        _password.Height = 44;
        _password.Font = Font;

        _role.DropDownStyle = ComboBoxStyle.DropDownList;
        _role.Items.AddRange(new object[] { "Veterinarian", "Secretary", "Administrator" });
        _role.Height = 44;
        _role.Font = Font;

        _error.ForeColor = UiTheme.ErrorText;
        _error.Text = string.Empty;
        _error.AutoSize = false;
        _error.Height = 56;
        _error.TextAlign = ContentAlignment.TopLeft;

        _save.Text = "Register";
        _save.Height = 52;
        _save.BackColor = UiTheme.HeaderBlue;
        _save.ForeColor = Color.White;
        _save.FlatStyle = FlatStyle.Flat;
        _save.FlatAppearance.BorderSize = 0;
        _save.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
        _save.Click += async (_, _) => await SaveAsync();

        _cancel.Text = "Close";
        _cancel.Height = 48;
        _cancel.FlatStyle = FlatStyle.Flat;
        _cancel.DialogResult = DialogResult.Cancel;

        var buttonRow = new TableLayoutPanel
        {
            ColumnCount = 2,
            Height = 56,
            Margin = new Padding(0, 8, 0, 0)
        };
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttonRow.Controls.Add(_save, 0, 0);
        buttonRow.Controls.Add(_cancel, 1, 0);
        _save.Dock = DockStyle.Fill;
        _cancel.Dock = DockStyle.Fill;
        _save.Margin = new Padding(0, 0, 8, 0);
        _cancel.Margin = new Padding(8, 0, 0, 0);

        void AddCaption(string text) =>
            _flow.Controls.Add(new Label
            {
                Text = text,
                ForeColor = UiTheme.TextDark,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 6)
            });

        AddCaption("Full Name");
        _flow.Controls.Add(_fullName);
        AddCaption("Email");
        _flow.Controls.Add(_email);
        AddCaption("Password");
        _flow.Controls.Add(_password);
        AddCaption("Role");
        _flow.Controls.Add(_role);
        _flow.Controls.Add(_error);
        _flow.Controls.Add(buttonRow);

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

            if (c is Label { AutoSize: true })
                continue;
            c.Width = inner;
        }
    }

    private void Relayout()
    {
        ResponsiveLayout.CenterCard(_body, _card, 48, 640, 56, 56);
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
