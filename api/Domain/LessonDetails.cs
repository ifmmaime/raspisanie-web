namespace Raspisanie.Api.Domain;

// Значение без собственного Id: некорректные сведения создать нельзя.
public sealed record LessonDetails
{
    public DateOnly Date { get; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }
    public string Subject { get; }
    public string Teacher { get; }
    public string Room { get; }

    public LessonDetails(DateOnly date, TimeOnly startTime, TimeOnly endTime,
        string subject, string teacher, string room)
    {
        if (date == default) throw new ArgumentException("Укажите дату занятия.");
        if (endTime <= startTime) throw new ArgumentException("Время окончания должно быть позже начала.");
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Subject = RequiredText(subject, 100, "Предмет");
        Teacher = RequiredText(teacher, 100, "Преподаватель");
        Room = RequiredText(room, 30, "Аудитория");
    }

    private static string RequiredText(string? value, int maximum, string field)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximum)
            throw new ArgumentException($"{field}: от 1 до {maximum} символов.");
        return value.Trim();
    }
}
