namespace ClinicVets.Desktop.UI;

/// <summary>Rounded banner for inline success / error messaging.</summary>
public sealed class FeedbackBannerPanel : Panel
{
    public readonly Label Message = new()
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(18, 14, 18, 14),
        AutoSize = false,
        TextAlign = ContentAlignment.MiddleLeft,
        BackColor = Color.Transparent
    };

    private UiFeedbackKind _kind = UiFeedbackKind.None;

    public FeedbackBannerPanel()
    {
        Height = 4;
        Margin = new Padding(0, 10, 0, 4);
        BackColor = UiTheme.PageBackground;
        Controls.Add(Message);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        UpdateStyles();
        Visible = false;
    }

    public void Clear()
    {
        _kind = UiFeedbackKind.None;
        Message.Text = string.Empty;
        Height = 4;
        Visible = false;
        Invalidate();
    }

    public void ShowMessage(UiFeedbackKind kind, string text)
    {
        _kind = kind;
        Message.Text = text;
        Message.ForeColor = kind switch
        {
            UiFeedbackKind.Error => UiTheme.ErrorText,
            UiFeedbackKind.Success => UiTheme.SuccessText,
            _ => UiTheme.TextMuted
        };
        var w = Math.Max(200, Width > 0 ? Width - 40 : 480);
        Message.MaximumSize = new Size(w, 0);
        Height = Math.Max(52, TextRenderer.MeasureText(text, Message.Font, new Size(w, int.MaxValue), TextFormatFlags.WordBreak).Height + 36);
        Visible = true;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (!Visible || string.IsNullOrEmpty(Message.Text))
            return;
        UiChrome.PaintRoundedBanner(this, e, _kind);
    }
}
