import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment.development';
import {
  LoginRequest,
  LoginResponse,
  RegisterVisitorRequest,
  RegisterResponse,
  Roles,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<LoginResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private activeRoleSubject = new BehaviorSubject<string | null>(null);
  public activeRole$ = this.activeRoleSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage(): void {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');

    if (token && userStr) {
      try {
        const user = JSON.parse(userStr);
        this.currentUserSubject.next(user);

        const storedActiveRole = sessionStorage.getItem('activeRole');
        if (storedActiveRole && user.roles?.includes(storedActiveRole)) {
          this.activeRoleSubject.next(storedActiveRole);
        } else {
          this.initializeActiveRole(user.roles || []);
        }
      } catch (error) {
        this.logout();
      }
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap((response) => {
          if (response.token) {
            localStorage.setItem('token', response.token);
            localStorage.setItem('user', JSON.stringify(response));
            this.currentUserSubject.next(response);

            this.initializeActiveRole(response.roles || []);
          }
        })
      );
  }

  register(registerData: RegisterVisitorRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(
      `${this.apiUrl}/auth/register`,
      registerData
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    sessionStorage.removeItem('activeRole');
    this.currentUserSubject.next(null);
    this.activeRoleSubject.next(null);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  }

  getUserRoles(): string[] {
    const user = this.currentUserSubject.value;
    return user?.roles || [];
  }

  hasRole(role: string): boolean {
    return this.getUserRoles().includes(role);
  }

  isAdministrator(): boolean {
    return this.activeRoleSubject.value === Roles.ADMINISTRATOR;
  }

  isOperator(): boolean {
    return this.activeRoleSubject.value === Roles.OPERATOR;
  }

  isVisitor(): boolean {
    return this.activeRoleSubject.value === Roles.VISITOR;
  }

  getCurrentUser(): LoginResponse | null {
    return this.currentUserSubject.value;
  }

  getUserId(): string | null {
    const user = this.currentUserSubject.value;
    return user?.id || null;
  }

  getDashboardRoute(): string {
    const activeRole = this.activeRoleSubject.value;

    if (activeRole === Roles.ADMINISTRATOR) {
      return '/admin/dashboard';
    } else if (activeRole === Roles.OPERATOR) {
      return '/operator/dashboard';
    } else if (activeRole === Roles.VISITOR) {
      return '/visitor/dashboard';
    }
    return '/login';
  }

  initializeActiveRole(roles: string[]): void {
    if (!roles || roles.length === 0) {
      return;
    }

    let selectedRole: string;

    if (roles.includes(Roles.ADMINISTRATOR)) {
      selectedRole = Roles.ADMINISTRATOR;
    } else if (roles.includes(Roles.OPERATOR)) {
      selectedRole = Roles.OPERATOR;
    } else if (roles.includes(Roles.VISITOR)) {
      selectedRole = Roles.VISITOR;
    } else {
      selectedRole = roles[0];
    }

    this.setActiveRole(selectedRole);
  }

  setActiveRole(role: string): void {
    const userRoles = this.getUserRoles();

    if (!userRoles.includes(role)) {
      console.error(`User does not have role: ${role}`);
      return;
    }

    sessionStorage.setItem('activeRole', role);
    this.activeRoleSubject.next(role);
  }

  getActiveRole(): string | null {
    return this.activeRoleSubject.value;
  }

  getAvailableRoles(): string[] {
    return this.getUserRoles();
  }

  hasMultipleRoles(): boolean {
    return this.getUserRoles().length > 1;
  }

  switchRole(newRole: string): void {
    const currentRoute = this.router.url;
    const userRoles = this.getUserRoles();

    if (!userRoles.includes(newRole)) {
      console.error(
        `Cannot switch to role: ${newRole}. User does not have this role.`
      );
      return;
    }

    this.setActiveRole(newRole);

    const rolePrefix = this.getRolePrefixFromRoute(currentRoute);
    const newRolePrefix = this.getRolePrefixForRole(newRole);

    if (rolePrefix && rolePrefix !== newRolePrefix) {
      this.router.navigate([this.getDashboardRoute()]);
    }
  }

  private getRolePrefixFromRoute(route: string): string | null {
    if (route.startsWith('/admin/')) return 'admin';
    if (route.startsWith('/operator/')) return 'operator';
    if (route.startsWith('/visitor/')) return 'visitor';
    return null;
  }

  private getRolePrefixForRole(role: string): string {
    switch (role) {
      case Roles.ADMINISTRATOR:
        return 'admin';
      case Roles.OPERATOR:
        return 'operator';
      case Roles.VISITOR:
        return 'visitor';
      default:
        return '';
    }
  }
}
