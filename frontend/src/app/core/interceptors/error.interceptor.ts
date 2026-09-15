import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { NotificationService } from '../services/notification.service';

interface ProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const notification = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const body = error.error as ProblemDetails | undefined;
      const message = body?.detail || body?.title || 'Ocorreu um erro inesperado.';
      notification.error(message);

      if (error.status === 401) {
        authService.logout();
      }

      return throwError(() => error);
    })
  );
};
