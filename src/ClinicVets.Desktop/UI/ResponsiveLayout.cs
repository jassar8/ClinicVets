namespace ClinicVets.Desktop.UI;

/// <summary>
/// Centers a content card inside a host panel and keeps width within bounds for different screen sizes.
/// </summary>
public static class ResponsiveLayout
{
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
}
