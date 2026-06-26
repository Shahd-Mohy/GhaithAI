import {
  HttpInterceptorFn
}
from '@angular/common/http';

export const authInterceptor:
HttpInterceptorFn =
(req,next)=>{

  const token =
    localStorage.getItem(
      'token'
    );

  req =
    req.clone({
      setHeaders:{
        'ngrok-skip-browser-warning': 'true',
        ...(token ? {
          Authorization:
          `Bearer ${token}`
        } : {})
      }
    });

  return next(req);
};
