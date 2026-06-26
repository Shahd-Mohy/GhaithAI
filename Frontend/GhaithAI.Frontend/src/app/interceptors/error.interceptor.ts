import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { NotificationService } from '../services/NotificationExceptionService';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error) => {
      if (error) {
        switch (error.status) {
          case 400:
          case 404:
          case 409:
            console.log('Interceptor triggered for 404');
            if (error.error?.message) {
              notificationService.show(error.error.message, 'error');
            }
            break;
          case 500:
            router.navigateByUrl('/server-error');
            break;
          case 401:
          case 403:
            notificationService.show('Unauthorised', 'error');
            break;
          default:
            notificationService.show('Something went wrong', 'error');
            break;
        }
      }
      return throwError(() => error);
    })
  );
};
