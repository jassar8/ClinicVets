namespace ClinicVets.Desktop.UI;

/// <summary>Responsive helpers for WinForms layout containers.</summary>
public static class ResponsiveLayout
{
    /// <summary>Legacy centering helper; prefer <see cref="ModernCenteredCardHost"/> for new pages.</summary>
    public static void CenterCard(
        Panel host,
        Control card,
        int horizontalPadding,
        int maxCardWidth,
        int topOffset,
        int bottomPadding)
    {
        if (host.ClientSize.Width <= 0 || host.ClientSize.Height <= 0)
            return;

        var minWidth = 420;
        var available = Math.Max(minWidth, host.ClientSize.Width - horizontalPadding * 2);
        var width = Math.Clamp(available, minWidth, maxCardWidth);
        card.Width = width;
        card.Left = (host.ClientSize.Width - width) / 2;
        card.Top = topOffset;
        card.Height = Math.Max(200, host.ClientSize.Height - topOffset - bottomPadding);
    }

    /// <summary>
    /// Keeps every child in a top-down <see cref="FlowLayoutPanel"/> at the same usable width (avoids clipped fields).
    /// </summary>
    public static void SyncFlowTopDownChildWidths(FlowLayoutPanel flow, int? innerWidthOverride = null)
    {
        var inner = innerWidthOverride ?? Math.Max(200, flow.ClientSize.Width - flow.Padding.Horizontal);
        foreach (Control c in flow.Controls)
        {
            if (c is Label l && l.AutoSize)
            {
                l.MaximumSize = new Size(inner, 0);
                continue;
            }

            c.Width = inner;
        }
    }

    public static void SyncSidebarNavButtonWidths(FlowLayoutPanel navHost, int horizontalMargin = 12)
    {
        var w = navHost.ClientSize.Width - navHost.Padding.Horizontal - horizontalMargin;
        if (w < 80)
            return;
        foreach (Control c in navHost.Controls)
            c.Width = w;
    }
}
