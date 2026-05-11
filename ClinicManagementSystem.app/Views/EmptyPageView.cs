using System;
using AppKit;
using CoreGraphics;
using ClinicManagementSystem.app.Helpers;
using ClinicManagementSystem.app.Helpers;

namespace ClinicManagementSystem.app.Views
{
    public class EmptyPageView : NSView
    {
        public Action BackToMainMenu;

        public EmptyPageView(string title, string message) : base(new CGRect(0, 0, 900, 650))
        {
            AddSubview(UIHelper.CreateLabel(title, 220, 520, 460, 50, true));
            AddSubview(UIHelper.CreateLabel(message, 220, 450, 460, 40));

            AddSubview(UIHelper.CreateButton("חזרה למסך ראשי", 330, 350, 240, 45, (sender, e) =>
            {
                BackToMainMenu?.Invoke();
            }));
        }
    }
}
