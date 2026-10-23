namespace Raspisanie.Api.Domain;

public sealed class Lesson
{
    public int Id { get; }
    public LessonDetails Details { get; }
    public bool? IsAttended { get; }

    public Lesson(int id, LessonDetails details, bool? isAttended = null)
    {
        if (id <= 0) throw new ArgumentException("Номер занятия должен быть положительным.");
        ArgumentNullException.ThrowIfNull(details);
        Id = id;
        Details = details;
        IsAttended = isAttended;
    }

    // Сведения меняются без потери идентификатора и отметки посещения.
    public Lesson ChangeDetails(LessonDetails details) => new(Id, details, IsAttended);
    public Lesson Mark(bool? isAttended) => new(Id, Details, isAttended);
}
