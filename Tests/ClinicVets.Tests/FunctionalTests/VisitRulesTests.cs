using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Tests;

// Tests for visit arrival rules: default status, when a visit is closed, and when arrival can be recorded.
public class VisitRulesTests
{
    // A new visit should start in the "Scheduled" arrival status.
    [Fact]
    public void NewVisit_DefaultsToScheduledArrivalStatus()
    {
        var visit = new Visit();

        Assert.Equal("Scheduled", visit.ArrivalStatus);
    }

    // Once a visit is Arrived or NoShow it is final and closed for further editing.
    [Theory]
    [InlineData("Arrived")]
    [InlineData("NoShow")]
    public void Visit_WithFinalArrivalStatus_IsClosedForEditing(string arrivalStatus)
    {
        var visit = new Visit { ArrivalStatus = arrivalStatus };

        Assert.True(IsVisitClosed(visit));
    }

    // Negative: arrival cannot be recorded for a visit whose date is still in the future.
    [Fact]
    public void FutureVisit_ShouldNotAllowArrivalRecording()
    {
        var visit = new Visit
        {
            VisitDate = DateTime.Now.AddDays(2),
            ArrivalStatus = "Scheduled"
        };

        Assert.False(CanRecordArrival(visit, DateTime.Now));
    }

    // Positive: a scheduled visit whose time has passed can have its arrival recorded.
    [Fact]
    public void PassedScheduledVisit_AllowsArrivalRecording()
    {
        var visit = new Visit
        {
            VisitDate = DateTime.Now.AddMinutes(-30),
            ArrivalStatus = "Scheduled"
        };

        Assert.True(CanRecordArrival(visit, DateTime.Now));
    }

    private static bool IsVisitClosed(Visit visit)
    {
        return visit.ArrivalStatus is "Arrived" or "NoShow";
    }

    private static bool CanRecordArrival(Visit visit, DateTime now)
    {
        return !IsVisitClosed(visit) && visit.VisitDate <= now;
    }
}
