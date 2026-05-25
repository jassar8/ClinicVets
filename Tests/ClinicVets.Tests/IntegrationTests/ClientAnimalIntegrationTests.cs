using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Tests;

public class ClientAnimalIntegrationTests
{
    [Fact]
    public void Animal_WithOwnerIdNumber_AppearsUnderMatchingClient()
    {
        var client = new Client
        {
            FullName = "Fares",
            IdNumber = "123456789"
        };

        var animals = new List<Animal>
        {
            new()
            {
                Name = "ZAZA",
                ChipNumber = "3761234",
                OwnerIdNumber = client.IdNumber
            },
            new()
            {
                Name = "Lolo",
                ChipNumber = "3765678",
                OwnerIdNumber = "987654321"
            }
        };

        var clientAnimals = animals
            .Where(animal => animal.OwnerIdNumber == client.IdNumber)
            .ToList();

        Assert.Single(clientAnimals);
        Assert.Equal("ZAZA", clientAnimals[0].Name);
    }

    [Fact]
    public void Visit_WithAnimalChipNumber_AppearsInThatAnimalVisitHistory()
    {
        var animal = new Animal
        {
            Name = "ZAZA",
            ChipNumber = "3761234",
            OwnerIdNumber = "123456789"
        };

        var visits = new List<Visit>
        {
            new()
            {
                AnimalChipNumber = animal.ChipNumber,
                VisitDate = DateTime.Today.AddDays(1),
                Reason = "Vaccination"
            },
            new()
            {
                AnimalChipNumber = "3765678",
                VisitDate = DateTime.Today.AddDays(2),
                Reason = "Checkup"
            }
        };

        var animalVisits = visits
            .Where(visit => visit.AnimalChipNumber == animal.ChipNumber)
            .ToList();

        Assert.Single(animalVisits);
        Assert.Equal("Vaccination", animalVisits[0].Reason);
    }

    [Fact]
    public void UpcomingVisit_ForAnimal_IsTheNearestFutureVisit()
    {
        var animal = new Animal { Name = "ZAZA", ChipNumber = "3761234" };
        var visits = new List<Visit>
        {
            new() { AnimalChipNumber = animal.ChipNumber, VisitDate = DateTime.Today.AddDays(10) },
            new() { AnimalChipNumber = animal.ChipNumber, VisitDate = DateTime.Today.AddDays(2) },
            new() { AnimalChipNumber = animal.ChipNumber, VisitDate = DateTime.Today.AddDays(-1) }
        };

        var nextVisit = visits
            .Where(visit => visit.AnimalChipNumber == animal.ChipNumber &&
                            visit.VisitDate >= DateTime.Today &&
                            visit.ArrivalStatus == "Scheduled")
            .OrderBy(visit => visit.VisitDate)
            .FirstOrDefault();

        Assert.NotNull(nextVisit);
        Assert.Equal(DateTime.Today.AddDays(2), nextVisit.VisitDate);
    }
}
