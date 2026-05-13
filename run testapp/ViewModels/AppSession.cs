using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.ViewModels;

/// <summary>
/// Holds the signed-in employee for the current application session.
/// </summary>
public sealed class AppSession
{
    public Employee? CurrentEmployee { get; set; }
}
