using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>Reserved module placeholder for the administrator shell.</summary>
public sealed class AdminPlaceholderPanel : UserControl
{
    public AdminPlaceholderPanel(string title, string subtitle)
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Padding = new Padding(32, 28, 32, 28);
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = UiTheme.CardWhite
        };

        stack.Controls.Add(UiStyles.CreateHeroTitle(title));
        stack.Controls.Add(UiStyles.CreateHeroSubtitle(subtitle));

        Controls.Add(stack);
    }
}
