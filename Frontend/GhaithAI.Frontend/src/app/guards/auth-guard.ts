import { inject }
from '@angular/core';

import {
CanActivateFn
}
from '@angular/router';

import {
TokenService
}
from '../services/token';

export const authGuard:
CanActivateFn = () => {

  const tokenService =
    inject(TokenService);

  return tokenService
    .isLoggedIn();
};