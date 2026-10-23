import { provideZonelessChangeDetection } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { AppComponent } from './app/layout/app/app.component';
import { routes } from './app/core/app.routes';

bootstrapApplication(AppComponent, {
  providers: [provideZonelessChangeDetection(), provideRouter(routes), provideHttpClient()]
}).catch(console.error);
