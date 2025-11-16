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

  // Check if user has the required role in their available roles
  const userRoles = authService.getUserRoles();
  const hasRequiredRole = requiredRoles.some(role => userRoles.includes(role));

  if (!hasRequiredRole) {
    // User doesn't have any of the required roles at all
    router.navigate(['/unauthorized']);
    return false;
  }

  // Check if the active role matches one of the required roles
  const activeRole = authService.getActiveRole();
  const activeRoleMatches = activeRole && requiredRoles.includes(activeRole);

  if (activeRoleMatches) {
    return true;
  }

  // User has the role but it's not currently active - redirect to their dashboard
  router.navigate([authService.getDashboardRoute()]);
  return false;
};
