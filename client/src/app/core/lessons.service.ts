import { Injectable } from '@angular/core';
import { attendanceValue, Lesson, LessonDetails, LessonQuery, Page, Statistics } from '../shared/lesson';
import { parseSchedule } from './schedule-parser';

@Injectable({ providedIn: 'root' })
export class LessonsService {
  // Во второй лабе данные находятся внутри обычного сервиса.
  // Компоненты не знают, откуда он их получает.
  private lessons: Lesson[] = [
    { id: 1, date: '2026-10-23', startTime: '10:00', endTime: '11:30', subject: 'Программная инженерия', teacher: 'И. А. Зорин', room: 'А-101', isAttended: true },
    { id: 2, date: '2026-10-23', startTime: '12:00', endTime: '13:30', subject: 'Базы данных', teacher: 'О. В. Петрова', room: 'Б-203', isAttended: false },
    { id: 3, date: '2026-10-24', startTime: '09:00', endTime: '10:30', subject: 'Английский язык', teacher: 'Е. С. Смирнова', room: 'В-305', isAttended: null },
    { id: 4, date: '2026-10-24', startTime: '11:00', endTime: '12:30', subject: 'Математика', teacher: 'А. Н. Волков', room: 'Г-404', isAttended: null }
  ];
  private nextId = 5;

  async list(query: LessonQuery): Promise<Page<Lesson>> {
    const items = this.lessons.filter(l => (!query.date || l.date === query.date)
      && l.subject.toLocaleLowerCase('ru').includes(query.search.trim().toLocaleLowerCase('ru'))
      && (!query.attendance || l.isAttended === attendanceValue(query.attendance)))
      .sort((a, b) => `${a.date} ${a.startTime}`.localeCompare(`${b.date} ${b.startTime}`) || a.id - b.id);
    return { items: structuredClone(items.slice((query.page - 1) * 10, query.page * 10)), total: items.length, page: query.page, pageSize: 10 };
  }

  async get(id: number): Promise<Lesson> {
    const lesson = this.lessons.find(l => l.id === id);
    if (!lesson) throw new Error('Занятие не найдено.');
    return structuredClone(lesson);
  }

  async save(details: LessonDetails, id?: number): Promise<void> {
    this.validate(details);
    if (id !== undefined) {
      const lesson = await this.get(id);
      this.lessons = this.lessons.map(l => l.id === id ? { ...lesson, ...details } : l);
    } else {
      this.lessons.push({ ...details, id: this.nextId++, isAttended: null });
    }
  }

  async mark(id: number, isAttended: boolean | null): Promise<void> {
    await this.get(id);
    this.lessons = this.lessons.map(l => l.id === id ? { ...l, isAttended } : l);
  }

  async delete(id: number): Promise<void> {
    await this.get(id);
    this.lessons = this.lessons.filter(l => l.id !== id);
  }

  async importHtml(html: string): Promise<number> {
    const lessons = parseSchedule(html);
    // Сначала проверяем весь файл, чтобы ошибка не оставила частичный импорт.
    lessons.forEach(l => this.validate(l));
    for (const details of lessons) await this.save(details);
    return lessons.length;
  }

  async statistics(): Promise<Statistics> {
    const subjects = [...new Set(this.lessons.map(l => l.subject))].sort().map(subject => {
      const items = this.lessons.filter(l => l.subject === subject);
      const attended = items.filter(l => l.isAttended === true).length;
      const absent = items.filter(l => l.isAttended === false).length;
      return { subject, total: items.length, attended, absent, percentage: this.percentage(attended, absent) };
    });
    const attended = this.lessons.filter(l => l.isAttended === true).length;
    const absent = this.lessons.filter(l => l.isAttended === false).length;
    return { total: this.lessons.length, attended, absent, unmarked: this.lessons.length - attended - absent, percentage: this.percentage(attended, absent), subjects };
  }

  private percentage(attended: number, absent: number): number {
    return attended + absent === 0 ? 0 : Math.round(attended / (attended + absent) * 1000) / 10;
  }

  private validate(details: LessonDetails): void {
    const date = new Date(`${details.date}T00:00:00Z`);
    if (!/^\d{4}-\d{2}-\d{2}$/.test(details.date) || !Number.isFinite(date.getTime()) || date.toISOString().slice(0, 10) !== details.date) throw new Error('Укажите существующую дату.');
    if (![details.startTime, details.endTime].every(t => /^([01]\d|2[0-3]):[0-5]\d$/.test(t)) || details.startTime >= details.endTime) throw new Error('Время окончания должно быть позже начала.');
    if (!details.subject.trim() || details.subject.length > 100 || !details.teacher.trim() || details.teacher.length > 100 || !details.room.trim() || details.room.length > 30) throw new Error('Заполните предмет, преподавателя и аудиторию в пределах допустимой длины.');
  }
}
