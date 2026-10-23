using Raspisanie.Api.Domain;

namespace Raspisanie.Api.Application;

public sealed class LessonService(ILessonRepository repository, ScheduleParser parser)
{
    private readonly ILessonRepository repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ScheduleParser parser = parser ?? throw new ArgumentNullException(nameof(parser));

    public Page<LessonDto> List(DateOnly? date, string? search, string? attendance, int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100) throw new ArgumentException("Неверные параметры страницы.");
        if (date == default(DateOnly)) throw new ArgumentException("Неверная дата.");
        if (search?.Length > 100) throw new ArgumentException("Поиск: не больше 100 символов.");
        if (attendance is not (null or "" or "present" or "absent" or "unmarked")) throw new ArgumentException("Неверная отметка посещения.");
        var items = repository.GetAll().Where(l => (date is null || l.Details.Date == date)
            && l.Details.Subject.Contains(search?.Trim() ?? "", StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrEmpty(attendance) || attendance switch
            {
                "present" => l.IsAttended == true,
                "absent" => l.IsAttended == false,
                _ => l.IsAttended is null
            })).OrderBy(l => l.Details.Date).ThenBy(l => l.Details.StartTime).ThenBy(l => l.Id).ToArray();
        // long защищает смещение от переполнения при большом номере страницы.
        var offset = (long)(page - 1) * pageSize;
        var slice = offset >= items.Length ? [] : items.Skip((int)offset).Take(pageSize).Select(LessonDto.From).ToArray();
        return new(slice, items.Length, page, pageSize);
    }

    public LessonDto Get(int id) => LessonDto.From(Find(id));

    public LessonDto Create(LessonDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);
        return LessonDto.From(repository.AddRange([details])[0]);
    }

    public void Update(int id, LessonDetails details)
    {
        CheckId(id);
        ArgumentNullException.ThrowIfNull(details);
        if (!repository.Update(id, l => l.ChangeDetails(details))) throw NotFound();
    }

    public void Mark(int id, bool? isAttended)
    {
        CheckId(id);
        if (!repository.Update(id, l => l.Mark(isAttended))) throw NotFound();
    }

    public void Delete(int id)
    {
        CheckId(id);
        if (!repository.Delete(id)) throw NotFound();
    }

    public int Import(string html)
    {
        // Проверяем весь файл до первой записи: импорт не бывает частичным.
        var details = parser.Parse(html);
        return repository.AddRange(details).Count;
    }

    public Statistics Statistics()
    {
        var lessons = repository.GetAll();
        var attended = lessons.Count(l => l.IsAttended == true);
        var absent = lessons.Count(l => l.IsAttended == false);
        var subjects = lessons.GroupBy(l => l.Details.Subject).OrderBy(g => g.Key).Select(g =>
        {
            var present = g.Count(l => l.IsAttended == true);
            var missed = g.Count(l => l.IsAttended == false);
            return new SubjectStatistics(g.Key, g.Count(), present, missed, Percentage(present, missed));
        }).ToArray();
        return new(lessons.Count, attended, absent, lessons.Count - attended - absent,
            Percentage(attended, absent), subjects);
    }

    private Lesson Find(int id)
    {
        CheckId(id);
        return repository.Find(id) ?? throw NotFound();
    }

    private static void CheckId(int id)
    {
        if (id <= 0) throw new ArgumentException("Номер занятия должен быть положительным.");
    }

    // Занятия без отметки не входят в знаменатель.
    private static double Percentage(int attended, int absent) => attended + absent == 0
        ? 0 : Math.Round((double)attended / (attended + absent) * 100, 1, MidpointRounding.AwayFromZero);
    private static KeyNotFoundException NotFound() => new("Занятие не найдено.");
}
