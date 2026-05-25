using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ClinicVetsAvalonia.Views
{
    public partial class EmptyPageView : UserControl
    {
        public Action? BackToMainMenu;

        public EmptyPageView()
        {
            InitializeComponent();
        }

        public EmptyPageView(string title, string message)
        {
            InitializeComponent();

            TitleText.Text = title;
            MessageText.Text = message;
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            BackToMainMenu?.Invoke();
        }
    }
}