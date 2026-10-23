using Raspisanie.Api.Domain;

namespace Raspisanie.Api.Infrastructure;

// Заглушка только вместо БД; сервис и контроллер зависят от интерфейса.
public sealed class InMemoryLessonRepository : ILessonRepository
{
    private readonly object gate = new();
    private readonly Dictionary<int, Lesson> lessons = new();
    private int nextId = 1;

    public InMemoryLessonRepository()
    {
        Seed(new(2026, 10, 23), new(10, 0), new(11, 30), "Программная инженерия", "И. А. Зорин", "А-101", true);
        Seed(new(2026, 10, 23), new(12, 0), new(13, 30), "Базы данных", "О. В. Петрова", "Б-203", false);
        Seed(new(2026, 10, 24), new(9, 0), new(10, 30), "Английский язык", "Е. С. Смирнова", "В-305", null);
        Seed(new(2026, 10, 24), new(11, 0), new(12, 30), "Математика", "А. Н. Волков", "Г-404", null);
    }

    public IReadOnlyList<Lesson> GetAll()
    {
        lock (gate) return lessons.Values.ToArray();
    }

    public Lesson? Find(int id)
    {
        if (id <= 0) throw new ArgumentException("Неверный номер занятия.");
        lock (gate) return lessons.GetValueOrDefault(id);
    }

    public IReadOnlyList<Lesson> AddRange(IReadOnlyList<LessonDetails> details)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (details.Count == 0 || details.Any(d => d is null))
            throw new ArgumentException("Список занятий не должен быть пустым или содержать null.");
        lock (gate)
        {
            var added = details.Select(d => new Lesson(nextId++, d)).ToArray();
            foreach (var lesson in added) lessons.Add(lesson.Id, lesson);
            return added;
        }
    }

    public bool Update(int id, Func<Lesson, Lesson> change)
    {
        if (id <= 0) throw new ArgumentException("Неверный номер занятия.");
        ArgumentNullException.ThrowIfNull(change);
        lock (gate)
        {
            if (!lessons.TryGetValue(id, out var lesson)) return false;
            var updated = change(lesson);
            if (updated is null || updated.Id != id) throw new ArgumentException("Нельзя менять номер занятия.");
            lessons[id] = updated;
            return true;
        }
    }

    public bool Delete(int id)
    {
        if (id <= 0) throw new ArgumentException("Неверный номер занятия.");
        lock (gate) return lessons.Remove(id);
    }

    private void Seed(DateOnly date, TimeOnly start, TimeOnly end, string subject, string teacher, string room, bool? attended)
    {
        var lesson = new Lesson(nextId++, new(date, start, end, subject, teacher, room), attended);
        lessons.Add(lesson.Id, lesson);
    }
}
