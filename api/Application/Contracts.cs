using Raspisanie.Api.Domain;

namespace Raspisanie.Api.Application;

public sealed record LessonDto(int Id, string Date, string StartTime, string EndTime,
    string Subject, string Teacher, string Room, bool? IsAttended)
{
    public static LessonDto From(Lesson lesson)
    {
        ArgumentNullException.ThrowIfNull(lesson);
        return new(lesson.Id, lesson.Details.Date.ToString("yyyy-MM-dd"),
            lesson.Details.StartTime.ToString("HH:mm"), lesson.Details.EndTime.ToString("HH:mm"),
            lesson.Details.Subject, lesson.Details.Teacher, lesson.Details.Room, lesson.IsAttended);
    }
}

public sealed record Page<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize);
public sealed record SubjectStatistics(string Subject, int Total, int Attended, int Absent, double Percentage);
public sealed record Statistics(int Total, int Attended, int Absent, int Unmarked,
    double Percentage, IReadOnlyList<SubjectStatistics> Subjects);
