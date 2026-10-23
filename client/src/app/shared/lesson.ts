export type Attendance = 'present' | 'absent' | 'unmarked';

export interface LessonDetails {
  date: string;
  startTime: string;
  endTime: string;
  subject: string;
  teacher: string;
  room: string;
}

export interface Lesson extends LessonDetails {
  id: number;
  isAttended: boolean | null;
}

export interface LessonQuery {
  date: string;
  search: string;
  attendance: string;
  page: number;
}

export interface Page<T> { items: T[]; total: number; page: number; pageSize: number; }
export interface SubjectStatistics { subject: string; total: number; attended: number; absent: number; percentage: number; }
export interface Statistics { total: number; attended: number; absent: number; unmarked: number; percentage: number; subjects: SubjectStatistics[]; }

// null означает отсутствие отметки, а не пропуск занятия.
export function attendanceValue(value: string): boolean | null {
  return value === 'present' ? true : value === 'absent' ? false : null;
}

export function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Не удалось выполнить действие. Повторите попытку.';
}
