using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace ClinicVetsAvalonia.Helpers
{
    public static class UIHelper
    {
        public static TextBlock CreateLabel(
            string text,
            bool isTitle = false)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = isTitle ? 26 : 15,
                FontWeight = isTitle ? FontWeight.Bold : FontWeight.Normal,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5)
            };
        }

        public static TextBox CreateInput(
            string placeholder,
            double width = 250)
        {
            return new TextBox
            {
                PlaceholderText = placeholder,
                Width = width,
                Margin = new Thickness(5)
            };
        }

        public static TextBox CreatePasswordInput(
            string placeholder,
            double width = 250)
        {
            return new TextBox
            {
                PlaceholderText = placeholder,
                Width = width,
                PasswordChar = '*',
                Margin = new Thickness(5)
            };
        }

        public static Button CreateButton(
            string title,
            EventHandler<RoutedEventArgs> action,
            double width = 150,
            double height = 35)
        {
            var button = new Button
            {
                Content = title,
                Width = width,
                Height = height,
                Margin = new Thickness(5),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            button.Click += action;

            return button;
        }

        public static ComboBox CreateRoleDropdown(
            double width = 250)
        {
            return new ComboBox
            {
                ItemsSource = new string[] { "מזכיר/ה", "וטרינר/ית" },
                SelectedIndex = 0,
                Width = width,
                Margin = new Thickness(5)
            };
        }

        public static Window CreateMessageWindow(string message)
        {
            var okButton = new Button
            {
                Content = "אישור",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            var window = new Window
            {
                Title = "הודעה",
                Width = 360,
                Height = 170,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(5)
                        },
                        okButton
                    }
                }
            };

            okButton.Click += (_, _) => window.Close();

            return window;
        }

        public static async void ShowMessage(Control owner, string message)
        {
            var window = CreateMessageWindow(message);
            var parentWindow = TopLevel.GetTopLevel(owner) as Window;

            if (parentWindow != null)
                await window.ShowDialog(parentWindow);
            else
                window.Show();
        }

        public static async Task<bool> ShowConfirmation(Control owner, string message)
        {
            var yesButton = new Button
            {
                Content = "כן",
                Width = 90,
                Background = Brushes.Firebrick,
                BorderBrush = Brushes.Firebrick,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(8)
            };

            var noButton = new Button
            {
                Content = "לא",
                Width = 90,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(8)
            };

            var window = new Window
            {
                Title = "אישור פעולה",
                Width = 390,
                Height = 190,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(22),
                    Spacing = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            FontSize = 16,
                            Margin = new Thickness(5)
                        },
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Children =
                            {
                                yesButton,
                                noButton
                            }
                        }
                    }
                }
            };

            yesButton.Click += (_, _) => window.Close(true);
            noButton.Click += (_, _) => window.Close(false);

            var parentWindow = TopLevel.GetTopLevel(owner) as Window;

            if (parentWindow != null)
                return await window.ShowDialog<bool>(parentWindow);

            window.Show();
            return false;
        }
    }
}