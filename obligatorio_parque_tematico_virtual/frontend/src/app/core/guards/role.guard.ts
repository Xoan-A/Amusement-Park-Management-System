import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const requiredRoles = route.data['roles'] as string[];

  if (!requiredRoles || requiredRoles.length === 0) {
    return true;
  }

  const userRoles = authService.getUserRoles();
  const hasRequiredRole = requiredRoles.some((role) =>
    userRoles.includes(role)
  );

  if (!hasRequiredRole) {
    router.navigate(['/unauthorized']);
    return false;
  }

  const activeRole = authService.getActiveRole();
  const activeRoleMatches = activeRole && requiredRoles.includes(activeRole);

  if (activeRoleMatches) {
    return true;
  }

  router.navigate([authService.getDashboardRoute()]);
  return false;
};
