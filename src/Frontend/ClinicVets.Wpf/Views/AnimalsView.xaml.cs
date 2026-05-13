using System.Windows.Controls;
using ClinicVets.Application.Services;

namespace ClinicVets.Wpf.Views;

public sealed class AnimalRow
{
    public string Customer { get; init; } = "";
    public string Animal { get; init; } = "";
    public string Species { get; init; } = "";
}

public partial class AnimalsView : UserControl
{
    public AnimalsView(CustomerDirectoryService customers)
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            var rows = new List<AnimalRow>();
            foreach (var c in await customers.ListCustomersAsync())
            {
                foreach (var a in await customers.GetAnimalsForCustomerAsync(c.Id))
                    rows.Add(new AnimalRow { Customer = c.FullName, Animal = a.Name, Species = a.Species });
            }

            Grid.ItemsSource = rows;
        };
    }
}
