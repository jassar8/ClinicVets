using ClinicVets.Application.Services;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public sealed class CustomerRegistrationPanel : UserControl
{
    private readonly CustomerDirectoryService _customers;
    private readonly FlowLayoutPanel _flow = new();
    private readonly TextBox _fullName = new();
    private readonly TextBox _nationalId = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _email = new();
    private readonly RoundedInputHost _nameHost;
    private readonly RoundedInputHost _idHost;
    private readonly RoundedInputHost _phoneHost;
    private readonly RoundedInputHost _emailHost;
    private readonly ModernAlertBanner _feedback = new();
    private readonly ModernPrimaryButton _save = new();
    private readonly Label _heroTitle;
    private readonly Label _heroSubtitle;

    public CustomerRegistrationPanel(CustomerDirectoryService customers)
    {
        _customers = customers;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        _flow.Dock = DockStyle.Fill;
        _flow.FlowDirection = FlowDirection.TopDown;
        _flow.WrapContents = false;
        _flow.AutoScroll = true;
        _flow.Padding = new Padding(8, 4, 8, 24);
        _flow.BackColor = UiTheme.CardWhite;
        _flow.SizeChanged += (_, _) => SyncWidths();

        _heroTitle = UiStyles.CreateHeroTitle("Customer registration");
        _heroSubtitle = UiStyles.CreateHeroSubtitle(
            "Register pet owners for the clinic. National ID must be nine digits; phone must include 9–15 digits.");

        _fullName.PlaceholderText = "Letters only (e.g. Sara Levi)";
        _nationalId.PlaceholderText = "9 digits (national ID)";
        _phone.PlaceholderText = "Mobile or clinic line";
        _email.PlaceholderText = "name@domain.com";

        _nameHost = new RoundedInputHost(_fullName);
        _idHost = new RoundedInputHost(_nationalId);
        _phoneHost = new RoundedInputHost(_phone);
        _emailHost = new RoundedInputHost(_email);

        _nationalId.MaxLength = 9;

        _feedback.Clear();

        _save.Text = "Save customer";
        _save.Margin = new Padding(0, 14, 0, 0);
        _save.Click += async (_, _) => await SaveAsync();

        _flow.Controls.Add(_heroTitle);
        _flow.Controls.Add(_heroSubtitle);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Full name"));
        _flow.Controls.Add(_nameHost);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("National ID"));
        _flow.Controls.Add(_idHost);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Phone"));
        _flow.Controls.Add(_phoneHost);
        _flow.Controls.Add(UiStyles.CreateFieldCaption("Email"));
        _flow.Controls.Add(_emailHost);
        _flow.Controls.Add(_feedback);
        _flow.Controls.Add(_save);

        Controls.Add(_flow);
    }

    private void SyncWidths()
    {
        var inner = Math.Max(320, _flow.ClientSize.Width - _flow.Padding.Horizontal);
        foreach (Control c in _flow.Controls)
        {
            if (c is Label { AutoSize: true } && c != _heroTitle && c != _heroSubtitle)
                continue;
            c.Width = inner;
        }
    }

    private async Task SaveAsync()
    {
        _save.Enabled = false;
        try
        {
            _feedback.Clear();
            var result = await _customers.RegisterCustomerAsync(
                _fullName.Text,
                _nationalId.Text,
                _phone.Text,
                _email.Text);
            if (!result.Ok)
            {
                _feedback.ShowMessage(UiFeedbackKind.Error, result.Message);
                return;
            }

            _feedback.ShowMessage(UiFeedbackKind.Success, result.Message);
            _fullName.Clear();
            _nationalId.Clear();
            _phone.Clear();
            _email.Clear();
        }
        finally
        {
            if (_save.IsHandleCreated)
                _save.Enabled = true;
        }
    }
}
