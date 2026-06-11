import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token';

export const adminGuard: CanActivateFn = () => {
  const token = inject(TokenService);
  const router = inject(Router);

  if (!token.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  if (!token.isAdmin()) {
    router.navigate(['/dashboard']);
    return false;
  }

  return true;
};
