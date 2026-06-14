using System;
using System.Collections.Generic;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Tests;

// Tests for deriving the latest veterinarian note for an animal from its visits.
public class AnimalNoteServiceTests
{
    private const string Chip = "3761234";

    [Fact]
    public void GetLatestVetNote_ReturnsNoteFromMostRecentVisitWithNote()
    {
        var visits = new List<Visit>
        {
            new() { AnimalChipNumber = Chip, VisitDate = DateTime.Today.AddDays(-5), ArrivalNote = "הערה ישנה" },
            new() { AnimalChipNumber = Chip, VisitDate = DateTime.Today.AddDays(-1), ArrivalNote = "הערה חדשה" },
            new() { AnimalChipNumber = Chip, VisitDate = DateTime.Today.AddDays(-10), ArrivalNote = "הערה הכי ישנה" }
        };

        string note = AnimalNoteService.GetLatestVetNote(Chip, visits);

        Assert.Equal("הערה חדשה", note);
    }

    [Fact]
    public void GetLatestVetNote_SkipsVisitsWithoutNote()
    {
        var visits = new List<Visit>
        {
            new() { AnimalChipNumber = Chip, VisitDate = DateTime.Today.AddDays(-2), ArrivalNote = "הערה תקפה" },
            new() { AnimalChipNumber = Chip, VisitDate = DateTime.Today.AddDays(-1), ArrivalNote = "   " }
        };

        string note = AnimalNoteService.GetLatestVetNote(Chip, visits);

        Assert.Equal("הערה תקפה", note);
    }

    [Fact]
    public void GetLatestVetNote_IgnoresOtherAnimals()
    {
        var visits = new List<Visit>
        {
            new() { AnimalChipNumber = "3769999", VisitDate = DateTime.Today, ArrivalNote = "של חיה אחרת" }
        };

        string note = AnimalNoteService.GetLatestVetNote(Chip, visits);

        Assert.Equal("", note);
    }

    [Fact]
    public void GetLatestVetNoteOrPlaceholder_ReturnsPlaceholderWhenNoNotes()
    {
        var visits = new List<Visit>
        {
            new() { AnimalChipNumber = Chip, VisitDate = DateTime.Today, ArrivalNote = "" }
        };

        string note = AnimalNoteService.GetLatestVetNoteOrPlaceholder(Chip, visits);

        Assert.Equal(AnimalNoteService.NoNoteText, note);
    }
}
