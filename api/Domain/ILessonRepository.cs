namespace Raspisanie.Api.Domain;

public interface ILessonRepository
{
    IReadOnlyList<Lesson> GetAll();
    Lesson? Find(int id);
    IReadOnlyList<Lesson> AddRange(IReadOnlyList<LessonDetails> details);
    bool Update(int id, Func<Lesson, Lesson> change);
    bool Delete(int id);
}
