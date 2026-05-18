using Avalonia.Controls;
using ClinicVets.Desktop.Helpers;

namespace ClinicVets.Desktop.Helpers.Stability;

public static class SafeViewLoader
{
    public static async Task RunSafeAsync(Control view, Func<Task> action, string context)
    {
        try
        {
            await action().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AppStability.LogException(context, ex);
            UIHelper.ShowMessage(view, FriendlyMessage(context));
        }
    }

    public static void RunSafe(Control view, Action action, string context)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            AppStability.LogException(context, ex);
            UIHelper.ShowMessage(view, FriendlyMessage(context));
        }
    }

    public static string FriendlyMessage(string context) =>
        $"אירעה שגיאה בטעינת המסך ({context}). המערכת ממשיכה לפעול — נסה שוב או חזור לתפריט הראשי.";
}
