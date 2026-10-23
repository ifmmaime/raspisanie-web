using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Raspisanie.Api.Domain;

namespace Raspisanie.Api.Controllers;

public sealed class LessonRequest
{
    [Required] public DateOnly? Date { get; init; }
    [Required] public TimeOnly? StartTime { get; init; }
    [Required] public TimeOnly? EndTime { get; init; }
    [Required, StringLength(100)] public string Subject { get; init; } = "";
    [Required, StringLength(100)] public string Teacher { get; init; } = "";
    [Required, StringLength(30)] public string Room { get; init; } = "";

    public LessonDetails ToDetails() => new(
        Date ?? throw new ArgumentException("Укажите дату."),
        StartTime ?? throw new ArgumentException("Укажите начало занятия."),
        EndTime ?? throw new ArgumentException("Укажите окончание занятия."), Subject, Teacher, Room);
}

public sealed class AttendanceRequest
{
    // Поле обязательно; null внутри него означает сброс отметки.
    [JsonRequired] public bool? IsAttended { get; init; }
}

public sealed class ImportRequest
{
    [Required, StringLength(1_048_576)] public string Html { get; init; } = "";
}
