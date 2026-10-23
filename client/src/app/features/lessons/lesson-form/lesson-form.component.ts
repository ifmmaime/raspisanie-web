import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LessonsService } from '../../../core/lessons.service';
import { errorMessage, LessonDetails } from '../../../shared/lesson';

@Component({
  selector: 'app-lesson-form', imports: [FormsModule, RouterLink],
  templateUrl: './lesson-form.component.html', styleUrl: './lesson-form.component.css'
})
export class LessonFormComponent {
  private readonly service = inject(LessonsService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly id = this.route.snapshot.paramMap.get('id');
  readonly error = signal('');
  readonly busy = signal(false);
  readonly ready = signal(!this.id);
  details: LessonDetails = { date: '', startTime: '09:00', endTime: '10:30', subject: '', teacher: '', room: '' };

  constructor() { if (this.id) void this.load(); }

  private async load(): Promise<void> {
    try {
      const { date, startTime, endTime, subject, teacher, room } = await this.service.get(Number(this.id));
      this.details = { date, startTime, endTime, subject, teacher, room };
      this.ready.set(true);
    } catch (error) { this.error.set(errorMessage(error)); }
  }

  async save(): Promise<void> {
    if (this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    try {
      await this.service.save(this.details, this.id ? Number(this.id) : undefined);
      await this.router.navigate(['/lessons']);
    } catch (error) { this.error.set(errorMessage(error)); }
    finally { this.busy.set(false); }
  }
}
