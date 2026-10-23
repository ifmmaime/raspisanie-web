import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Lesson, LessonDetails, LessonQuery, Page, Statistics } from '../shared/lesson';

@Injectable({ providedIn: 'root' })
export class LessonsService {
  private readonly http = inject(HttpClient);

  list(query: LessonQuery): Promise<Page<Lesson>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', 10);
    if (query.date) params = params.set('date', query.date);
    if (query.search.trim()) params = params.set('search', query.search.trim());
    if (query.attendance) params = params.set('attendance', query.attendance);
    return this.request(firstValueFrom(this.http.get<Page<Lesson>>('/api/lessons', { params })));
  }

  get(id: number): Promise<Lesson> {
    return this.request(firstValueFrom(this.http.get<Lesson>(`/api/lessons/${id}`)));
  }

  async save(details: LessonDetails, id?: number): Promise<void> {
    // ISO-время API содержит секунды; поле формы хранит часы и минуты.
    const body = { ...details, startTime: `${details.startTime}:00`, endTime: `${details.endTime}:00` };
    if (id === undefined) await this.request(firstValueFrom(this.http.post('/api/lessons', body)));
    else await this.request(firstValueFrom(this.http.put(`/api/lessons/${id}`, body)));
  }

  async mark(id: number, isAttended: boolean | null): Promise<void> {
    await this.request(firstValueFrom(this.http.patch(`/api/lessons/${id}/attendance`, { isAttended })));
  }

  async delete(id: number): Promise<void> {
    await this.request(firstValueFrom(this.http.delete(`/api/lessons/${id}`)));
  }

  async importHtml(html: string): Promise<number> {
    const result = await this.request(firstValueFrom(this.http.post<{ count: number }>('/api/lessons/import', { html })));
    return result.count;
  }

  statistics(): Promise<Statistics> {
    return this.request(firstValueFrom(this.http.get<Statistics>('/api/statistics')));
  }

  private async request<T>(promise: Promise<T>): Promise<T> {
    try { return await promise; }
    catch (error) {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 0 || error.status === 502 || error.status === 504) throw new Error('API недоступен. Проверьте, что сервер запущен на порту 5080.');
        throw new Error(error.error?.title ?? 'Не удалось выполнить запрос. Повторите попытку.');
      }
      throw error;
    }
  }
}
