import { Routes } from '@angular/router';
import { LessonsPageComponent } from '../features/lessons/lessons-page/lessons-page.component';
import { LessonFormComponent } from '../features/lessons/lesson-form/lesson-form.component';
import { StatisticsPageComponent } from '../features/statistics/statistics-page/statistics-page.component';
import { ImportPageComponent } from '../features/import/import-page/import-page.component';

export const routes: Routes = [
  { path: '', redirectTo: 'lessons', pathMatch: 'full' },
  { path: 'lessons', component: LessonsPageComponent },
  { path: 'lessons/new', component: LessonFormComponent },
  { path: 'lessons/:id/edit', component: LessonFormComponent },
  { path: 'statistics', component: StatisticsPageComponent },
  { path: 'import', component: ImportPageComponent },
  { path: '**', redirectTo: 'lessons' }
];
