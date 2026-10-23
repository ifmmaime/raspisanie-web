import { Component, inject, signal } from '@angular/core';
import { LessonsService } from '../../../core/lessons.service';
import { errorMessage } from '../../../shared/lesson';

@Component({
  selector: 'app-import-page', templateUrl: './import-page.component.html',
  styleUrl: './import-page.component.css'
})
export class ImportPageComponent {
  private readonly service = inject(LessonsService);
  readonly error = signal('');
  readonly message = signal('');
  readonly busy = signal(false);
  readonly file = signal<File | null>(null);

  choose(event: Event): void {
    this.file.set((event.target as HTMLInputElement).files?.[0] ?? null);
    this.error.set(''); this.message.set('');
  }

  async import(): Promise<void> {
    const file = this.file();
    if (!file || this.busy()) return;
    this.error.set(''); this.message.set('');
    if (!/\.html?$/i.test(file.name) || file.size > 1_048_576) {
      this.error.set('Выберите HTML-файл размером не больше 1 МБ.'); return;
    }
    this.busy.set(true);
    try {
      const count = await this.service.importHtml(await file.text());
      this.message.set(`Добавлено занятий: ${count}. Они доступны в разделе «Занятия».`);
      this.file.set(null);
    } catch (error) { this.error.set(errorMessage(error)); }
    finally { this.busy.set(false); }
  }
}
