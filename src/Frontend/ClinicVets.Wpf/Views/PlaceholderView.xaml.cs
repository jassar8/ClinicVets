using System.Windows.Controls;

namespace ClinicVets.Wpf.Views;

public partial class PlaceholderView : UserControl
{
    public PlaceholderView(string title, string body)
    {
        InitializeComponent();
        TitleBlock.Text = title;
        BodyBlock.Text = body;
    }
}
