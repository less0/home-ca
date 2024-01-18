import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
import { firstValueFrom } from 'rxjs';

export const authGuard: CanActivateFn = async (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const isAuthenticated = await firstValueFrom(authService.isAuthenticated$);

  if(isAuthenticated)
  {
    return true;
  }

  return router.parseUrl("/");
};
