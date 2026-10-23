import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LessonsService } from '../../../core/lessons.service';
import { attendanceValue, errorMessage, Lesson, LessonQuery, Page } from '../../../shared/lesson';

@Component({
  selector: 'app-lessons-page',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './lessons-page.component.html',
  styleUrl: './lessons-page.component.css'
})
export class LessonsPageComponent {
  private readonly service = inject(LessonsService);
  private request = 0;
  readonly result = signal<Page<Lesson>>({ items: [], total: 0, page: 1, pageSize: 10 });
  readonly loading = signal(false);
  readonly error = signal('');
  readonly pendingId = signal<number | null>(null);
  readonly deleteId = signal<number | null>(null);
  query: LessonQuery = { date: '', search: '', attendance: '', page: 1 };

  constructor() { void this.load(); }

  async load(page = 1): Promise<void> {
    this.query.page = page;
    const request = ++this.request;
    this.loading.set(true);
    this.error.set('');
    try {
      const result = await this.service.list({ ...this.query });
      // Более старый ответ не должен перезаписать новый результат поиска.
      if (request === this.request) this.result.set(result);
    } catch (error) {
      if (request === this.request) this.error.set(errorMessage(error));
    } finally {
      if (request === this.request) this.loading.set(false);
    }
  }

  status(lesson: Lesson): string {
    return lesson.isAttended === true ? 'present' : lesson.isAttended === false ? 'absent' : 'unmarked';
  }

  async mark(lesson: Lesson, value: string): Promise<void> {
    this.pendingId.set(lesson.id);
    try {
      await this.service.mark(lesson.id, attendanceValue(value));
      await this.load(this.query.page);
    } catch (error) {
      this.error.set(errorMessage(error));
      this.result.update(r => ({ ...r, items: r.items.map(l => ({ ...l })) }));
    } finally { this.pendingId.set(null); }
  }

  async remove(id: number): Promise<void> {
    this.pendingId.set(id);
    try {
      await this.service.delete(id);
      this.deleteId.set(null);
      const page = this.result().items.length === 1 ? Math.max(1, this.query.page - 1) : this.query.page;
      await this.load(page);
    } catch (error) { this.error.set(errorMessage(error)); }
    finally { this.pendingId.set(null); }
  }
}
