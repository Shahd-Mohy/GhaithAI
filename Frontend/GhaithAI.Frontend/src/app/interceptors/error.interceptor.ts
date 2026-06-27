import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { NotificationService } from '../services/NotificationExceptionService';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'Something went wrong, please try again later.';

      if (error.error && typeof error.error.message === 'string') {
        errorMessage = error.error.message;
      }

      switch (error.status) {
        case 400: // Bad Request
          notificationService.show(errorMessage, 'error');
          break;

        case 401: // Unauthorised
          notificationService.show('Session expired, please login again.', 'error');
          router.navigateByUrl('/login');
          break;

        case 403: // Forbidden
          notificationService.show('You do not have permission to perform this action.', 'error');
          break;

        case 404: // Not Found
          notificationService.show(errorMessage, 'error');
          break;

        case 409: // Conflict
          notificationService.show(errorMessage, 'warning');
          break;

        case 428: // Precondition Required
          notificationService.show('Please accept the AI consent form to continue.', 'info');
          router.navigateByUrl('/consent');
          break;

        case 502: // Bad Gateway (External Service Error)
          notificationService.show('Service is temporarily unavailable, please try again later.', 'error');
          break;

        case 500: // Internal Server Error
          router.navigateByUrl('/server-error');
          break;

        default:
          notificationService.show(errorMessage, 'error');
          break;
      }

      return throwError(() => error);
    })
  );
};
