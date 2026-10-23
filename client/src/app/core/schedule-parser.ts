import { LessonDetails } from '../shared/lesson';

export function parseSchedule(html: string): LessonDetails[] {
  const document = new DOMParser().parseFromString(html, 'text/html');
  const result: LessonDetails[] = [];
  for (const day of document.querySelectorAll('.day')) {
    const date = day.querySelector('.date p')?.textContent?.trim() ?? '';
    const [d, m, y] = date.split('.');
    for (const row of day.querySelectorAll('.day_lesson')) {
      const info = row.querySelectorAll('.lessen_info');
      const time = row.querySelector('.num_time p:nth-child(2)')?.textContent ?? '';
      const [startTime, endTime] = time.trim().split(/\s*[-–—]\s*/);
      result.push({
        date: `${y}-${m}-${d}`, startTime, endTime,
        subject: info[0]?.querySelector('.name')?.textContent?.trim() ?? '',
        teacher: info[0]?.querySelector('p:not([class])')?.textContent?.trim() ?? '',
        room: (info[1]?.querySelector('p:last-child')?.textContent ?? '').replace(/^[Аа]удитория[:\s]*/, '').trim()
      });
    }
  }
  if (!result.length) throw new Error('В HTML не найдены занятия. Используйте файл в формате примера.');
  return result;
}
