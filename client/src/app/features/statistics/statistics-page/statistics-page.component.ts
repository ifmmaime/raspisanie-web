import { Component, inject, signal } from '@angular/core';
import { LessonsService } from '../../../core/lessons.service';
import { errorMessage, Statistics } from '../../../shared/lesson';

@Component({
  selector: 'app-statistics-page', templateUrl: './statistics-page.component.html',
  styleUrl: './statistics-page.component.css'
})
export class StatisticsPageComponent {
  private readonly service = inject(LessonsService);
  readonly data = signal<Statistics | null>(null);
  readonly error = signal('');
  constructor() { void this.load(); }

  private async load(): Promise<void> {
    try { this.data.set(await this.service.statistics()); }
    catch (error) { this.error.set(errorMessage(error)); }
  }
}
