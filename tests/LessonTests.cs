using Raspisanie.Api.Application;
using Raspisanie.Api.Domain;
using Raspisanie.Api.Infrastructure;

namespace Raspisanie.Tests;

public sealed class LessonTests
{
    private static LessonDetails Details(string subject = "Математика") =>
        new(new(2026, 10, 23), new(9, 0), new(10, 30), subject, "Преподаватель", "101");

    [Fact]
    public void ChangingDetailsPreservesAttendanceAndIdentity()
    {
        var original = new Lesson(7, Details(), true);
        var changed = original.ChangeDetails(Details("Физика"));
        Assert.Equal(7, changed.Id);
        Assert.True(changed.IsAttended);
        Assert.Equal("Физика", changed.Details.Subject);
        Assert.Equal("Математика", original.Details.Subject);
    }

    [Fact]
    public void InvalidTimeAndBlankSubjectAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new LessonDetails(new(2026, 10, 23), new(10, 0), new(9, 0), "Предмет", "Преподаватель", "1"));
        Assert.Throws<ArgumentException>(() => Details("   "));
    }

    [Fact]
    public void UnmarkedLessonsDoNotReducePercentage()
    {
        var repository = new InMemoryLessonRepository();
        var service = new LessonService(repository, new ScheduleParser());
        var stats = service.Statistics();
        Assert.Equal(50, stats.Percentage);
        Assert.Equal(2, stats.Unmarked);
        foreach (var lesson in repository.GetAll()) service.Mark(lesson.Id, null);
        Assert.Equal(0, service.Statistics().Percentage);
    }

    [Fact]
    public void EmptyStatisticsDoNotDivideByZero()
    {
        var repository = new InMemoryLessonRepository();
        var service = new LessonService(repository, new ScheduleParser());
        foreach (var lesson in repository.GetAll()) service.Delete(lesson.Id);
        Assert.Equal(0, service.Statistics().Percentage);
        Assert.Empty(service.Statistics().Subjects);
    }

    [Fact]
    public void LargePageDoesNotOverflow()
    {
        var service = new LessonService(new InMemoryLessonRepository(), new ScheduleParser());
        Assert.Empty(service.List(null, null, null, int.MaxValue, 100).Items);
    }

    [Fact]
    public void ImportDecodesEntitiesAndRemovesRoomPrefix()
    {
        var parser = new ScheduleParser();
        var lesson = Assert.Single(parser.Parse(Html("Математика &amp; логика")));
        Assert.Equal("Математика & логика", lesson.Subject);
        Assert.Equal("101", lesson.Room);
    }

    [Fact]
    public void InvalidImportDoesNotAddAnyLessons()
    {
        var repository = new InMemoryLessonRepository();
        var service = new LessonService(repository, new ScheduleParser());
        var before = repository.GetAll().Count;
        Assert.Throws<ArgumentException>(() => service.Import(Html("Верный предмет") + Html("")));
        Assert.Equal(before, repository.GetAll().Count);
    }

    internal static string Html(string subject) => $"""
        <div class="day"><div class="date"><p>25.10.2026</p></div>
          <div class="day_lesson"><div class="num_time"><p>1</p><p>09:00 - 10:30</p></div>
            <div class="lessen_info"><p class="name">{subject}</p><p>Преподаватель</p></div>
            <div class="lessen_info"><p>Аудитория: 101</p></div>
          </div>
        </div>
        """;
}
