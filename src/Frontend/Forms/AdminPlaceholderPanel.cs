using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>Reserved module placeholder for the administrator shell.</summary>
public sealed class AdminPlaceholderPanel : UserControl
{
    public AdminPlaceholderPanel(string title, string subtitle)
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Padding = new Padding(UiTheme.Layout.PageGutter, UiTheme.Layout.CardInset, UiTheme.Layout.PageGutter, UiTheme.Layout.CardInset);
        Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = UiTheme.CardWhite
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var titleLbl = UiStyles.CreateHeroTitle(title);
        var subLbl = UiStyles.CreateHeroSubtitle(subtitle);

        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = UiTheme.CardWhite,
            Padding = new Padding(4, 8, 4, 8)
        };
        stack.Controls.Add(titleLbl);
        stack.Controls.Add(subLbl);

        void Sync()
        {
            var inner = Math.Max(200, stack.ClientSize.Width - stack.Padding.Horizontal);
            titleLbl.MaximumSize = new Size(inner, 0);
            subLbl.MaximumSize = new Size(inner, 0);
        }

        stack.SizeChanged += (_, _) => Sync();
        stack.HandleCreated += (_, _) => Sync();

        root.Controls.Add(stack, 0, 0);
        Controls.Add(root);
    }
}
