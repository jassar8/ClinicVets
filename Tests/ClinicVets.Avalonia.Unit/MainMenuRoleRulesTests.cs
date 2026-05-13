using Xunit;

namespace ClinicVets.Avalonia.Unit;

/// <summary>
/// Documents expected menu enablement — keep in sync with <c>MainMenuView.ApplyEmployeeData</c>.
/// </summary>
public class MainMenuRoleRulesTests
{
    [Theory]
    [InlineData("Secretary", true, true, false, false)]
    [InlineData("Vet", false, true, true, true)]
    [InlineData("Unknown", false, false, false, false)]
    public void Role_matrix_matches_main_menu_view(
        string role,
        bool clients,
        bool animals,
        bool visits,
        bool medicines)
    {
        // Mirror MainMenuView.ApplyEmployeeData
        bool c, a, v, m;
        if (role == "Secretary")
        {
            c = true;
            a = true;
            v = false;
            m = false;
        }
        else if (role == "Vet")
        {
            c = false;
            a = true;
            v = true;
            m = true;
        }
        else
        {
            c = a = v = m = false;
        }

        Assert.Equal(clients, c);
        Assert.Equal(animals, a);
        Assert.Equal(visits, v);
        Assert.Equal(medicines, m);
    }
}
