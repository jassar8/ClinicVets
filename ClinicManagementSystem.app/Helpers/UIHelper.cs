using System;
using AppKit;
using CoreGraphics;

namespace ClinicManagementSystem.app.Helpers
{
    public static class UIHelper
    {
        public static NSTextField CreateLabel(
            string text,
            double x,
            double y,
            double width,
            double height,
            bool isTitle = false)
        {
            return new NSTextField(new CGRect(x, y, width, height))
            {
                StringValue = text,
                Editable = false,
                Bezeled = false,
                DrawsBackground = false,
                Selectable = false,
                Alignment = NSTextAlignment.Center,
                Font = isTitle ? NSFont.BoldSystemFontOfSize(26) : NSFont.SystemFontOfSize(15)
            };
        }

        public static NSTextField CreateInput(
            string placeholder,
            double x,
            double y,
            double width,
            double height)
        {
            return new NSTextField(new CGRect(x, y, width, height))
            {
                PlaceholderString = placeholder,
                Font = NSFont.SystemFontOfSize(14)
            };
        }

        public static NSSecureTextField CreatePasswordInput(
            string placeholder,
            double x,
            double y,
            double width,
            double height)
        {
            return new NSSecureTextField(new CGRect(x, y, width, height))
            {
                PlaceholderString = placeholder,
                Font = NSFont.SystemFontOfSize(14)
            };
        }

        public static NSButton CreateButton(
            string title,
            double x,
            double y,
            double width,
            double height,
            EventHandler action)
        {
            var button = new NSButton(new CGRect(x, y, width, height))
            {
                Title = title,
                BezelStyle = NSBezelStyle.Rounded,
                Font = NSFont.SystemFontOfSize(14)
            };

            button.Activated += action;
            return button;
        }

        public static NSPopUpButton CreateRoleDropdown(
            double x,
            double y,
            double width,
            double height)
        {
            var dropdown = new NSPopUpButton(new CGRect(x, y, width, height), false);
            dropdown.AddItems(new string[] { "Secretary", "Vet" });
            return dropdown;
        }

        public static void ShowMessage(string message)
        {
            var alert = new NSAlert
            {
                MessageText = message
            };

            alert.RunModal();
        }
    }
}