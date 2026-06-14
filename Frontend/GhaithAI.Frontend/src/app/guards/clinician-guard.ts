import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token';

export const clinicianGuard: CanActivateFn = () => {
  const token = inject(TokenService);
  const router = inject(Router);

  if (!token.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  if (!token.isClinician()) {
    // Logged in but not a clinician — redirect to appropriate dashboard
    router.navigate(['/dashboard']);
    return false;
  }

  return true;
};
