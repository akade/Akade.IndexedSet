using Akade.IndexedSet.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akade.IndexedSet.Tests.Samples.Appointments;

[TestClass]
public class AppointmentSample
{
    private readonly DateTime _todayDateTime;
    private readonly DateOnly _today;

    private static TimeSpan Duration(Appointment appointment)
    {
        return appointment.End - appointment.Start;
    }

    private readonly IndexedSet<int, Appointment> _appointments = IndexedSetBuilder<Appointment>.Create(a => a.Id)
        .WithIndex(x => x.Owner)
        .WithRangeIndex(x => x.Start)
        .WithRangeIndex(x => x.End)
        .WithRangeIndex(Duration) // calculated property
        .Build();

    public AppointmentSample()
    {
        _todayDateTime = DateTime.Today;
        _today = DateOnly.FromDateTime(_todayDateTime);

        DateOnly[] days = Enumerable.Range(0, 5).Select(offset => _today.AddDays(offset)).ToArray();
        int id = 1;

        foreach (DateOnly day in days)
        {
            _appointments.Add(new(id++, "Daily", "Lancelot", day.WithDayTime(09, 00), day.WithDayTime(09, 15)));
        }
        _appointments.Add(new(id++, "Weekly", "Cinderella", days[0].WithDayTime(10, 00), days[0].WithDayTime(11, 15)));

        _appointments.Add(new(id++, "Iteration Planning", "Esmeralda", days[2].WithDayTime(09, 15), days[2].WithDayTime(16, 15)));

        _appointments.Add(new(id++, "Discuss Technical Debt #42", "Lancelot", days[3].WithDayTime(11, 00), days[3].WithDayTime(11, 45)));


        _appointments.Add(new(id++, "Discuss Issue #1234", "Esmeralda", days[4].WithDayTime(09, 15), days[4].WithDayTime(10, 00)));

    }


    [TestMethod]
    public void get_all_meetings_for_owner()
    {
        // Multivalue access. Note that we use the same expression that was used during setting the set up
        Assert.AreEqual(6, _appointments.Where(x => x.Owner, "Lancelot").Count());
        Assert.AreEqual(2, _appointments.Where(x => x.Owner, "Esmeralda").Count());
        Assert.AreEqual(1, _appointments.Where(x => x.Owner, "Cinderella").Count());
    }


    [TestMethod]
    public void get_all_meetings_today_or_tomorrow()
    {
        // Range query with exlusive end
        string[] meetings = _appointments.Range(x => x.Start, _todayDateTime, _todayDateTime.AddDays(2))
                                         .OrderBy(x => x.Start) // This call is not necessary at the moment but the order is not part of the spec and might change depending on the index implementation
                                         .Select(x => x.Subject)
                                         .ToArray();

        CollectionAssert.AreEqual(new[] { "Daily", "Weekly", "Daily" }, meetings);
    }

    [TestMethod]
    public void check_for_conflicting_meetings()
    {
        DateTime plannedStart = _today.AddDays(2).WithDayTime(09, 00);
        DateTime plannedEnd = _today.AddDays(2).WithDayTime(09, 30);

        // Note that you can specify if the start and end of ranges are inclusive or exclusive. Here we do not want adjacent meetings and
        // for the sample data, it does in fact not matter...
        IEnumerable<Appointment> viaStart = _appointments.Range(x => x.Start, plannedStart, plannedEnd, inclusiveStart: false, inclusiveEnd: false);
        IEnumerable<Appointment> viaEnd = _appointments.Range(x => x.End, plannedStart, plannedEnd, inclusiveStart: false, inclusiveEnd: false);

        string[] meetings = viaStart.Union(viaEnd)
                                    .OrderBy(x => x.Start)
                                    .Select(x => x.Subject)
                                    .ToArray();

        CollectionAssert.AreEqual(new[] { "Daily", "Iteration Planning" }, meetings);
    }

    [TestMethod]
    [ExpectedException(typeof(IndexNotFoundException))]
    public void key_expression_does_matter()
    {
        // the index was created with x => x.Owner
        // Hence, even though the indexed key is the same, the name for the index is not
        _ = _appointments.Single(t => t.Owner, "Test");
    }

    [TestMethod]
    public void using_a_calculated_index_property_to_find_all_longer_appointments()
    {
        // Note that you can use complex key extractors, make sure 
        string[] longerAppointments = _appointments.Range(Duration, TimeSpan.FromMinutes(30), TimeSpan.MaxValue)
                                                   .OrderByDescending(Duration)
                                                   .Select(a => $"{a.Subject} - {Duration(a).TotalMinutes:00}")
                                                   .ToArray();

        longerAppointments.ForEach(Console.WriteLine);

        CollectionAssert.AreEqual(new[] { "Iteration Planning - 420", "Weekly - 75", "Discuss Issue #1234 - 45", "Discuss Technical Debt #42 - 45" }, longerAppointments);
    }
}
