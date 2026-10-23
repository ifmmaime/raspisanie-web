using System.Globalization;
using HtmlAgilityPack;
using Raspisanie.Api.Domain;

namespace Raspisanie.Api.Application;

public sealed class ScheduleParser
{
    public IReadOnlyList<LessonDetails> Parse(string html)
    {
        if (string.IsNullOrWhiteSpace(html) || html.Length > 1_048_576)
            throw new ArgumentException("Передайте HTML размером не больше 1 МБ.");
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var result = new List<LessonDetails>();
        foreach (var day in document.DocumentNode.SelectNodes("//div[contains(concat(' ',normalize-space(@class),' '),' day ')]") ?? Enumerable.Empty<HtmlNode>())
        {
            var dateText = Text(day.SelectSingleNode(".//div[@class='date']/p[1]"));
            if (!DateOnly.TryParseExact(dateText, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw new ArgumentException("В расписании указана неверная дата.");
            foreach (var row in day.SelectNodes(".//div[@class='day_lesson']") ?? Enumerable.Empty<HtmlNode>())
            {
                var time = Text(row.SelectSingleNode(".//div[@class='num_time']/p[2]"));
                var parts = time.Split(['-', '–', '—'], StringSplitOptions.TrimEntries);
                if (parts.Length != 2 || !TryTime(parts[0], out var start) || !TryTime(parts[1], out var end))
                    throw new ArgumentException("В расписании указано неверное время.");
                var info = row.SelectNodes(".//div[@class='lessen_info']");
                if (info is null || info.Count < 2) throw new ArgumentException("В занятии не хватает сведений.");
                var room = Text(info[1].SelectSingleNode(".//p[last()]"));
                if (room.StartsWith("Аудитория:", StringComparison.OrdinalIgnoreCase)) room = room[10..].Trim();
                result.Add(new(date, start, end, Text(info[0].SelectSingleNode(".//p[@class='name']")),
                    Text(info[0].SelectSingleNode(".//p[not(@class)]")), room));
                if (result.Count > 500) throw new ArgumentException("В одном файле допускается до 500 занятий.");
            }
        }
        if (result.Count == 0) throw new ArgumentException("В HTML не найдены занятия. Используйте файл в формате примера.");
        return result;
    }

    private static string Text(HtmlNode? node) => HtmlEntity.DeEntitize(node?.InnerText ?? "").Trim();
    private static bool TryTime(string text, out TimeOnly time) =>
        TimeOnly.TryParseExact(text, ["HH:mm", "H:mm"], CultureInfo.InvariantCulture, DateTimeStyles.None, out time);
}
