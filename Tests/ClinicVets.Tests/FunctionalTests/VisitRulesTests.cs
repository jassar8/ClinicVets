using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Tests;

public class VisitRulesTests
{
    [Fact]
    public void NewVisit_DefaultsToScheduledArrivalStatus()
    {
        var visit = new Visit();

        Assert.Equal("Scheduled", visit.ArrivalStatus);
    }

    [Theory]
    [InlineData("Arrived")]
    [InlineData("NoShow")]
    public void Visit_WithFinalArrivalStatus_IsClosedForEditing(string arrivalStatus)
    {
        var visit = new Visit { ArrivalStatus = arrivalStatus };

        Assert.True(IsVisitClosed(visit));
    }

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
