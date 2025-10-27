import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment.development';
import { LoginRequest, LoginResponse, RegisterVisitorRequest, RegisterResponse, Roles } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<LoginResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

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
      } catch (error) {
        this.logout();
      }
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials).pipe(
      tap(response => {
        if (response.token) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify(response));
          this.currentUserSubject.next(response);
        }
      })
    );
  }

  register(registerData: RegisterVisitorRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/auth/register`, registerData);
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
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
    return this.hasRole(Roles.ADMINISTRATOR);
  }

  isOperator(): boolean {
    return this.hasRole(Roles.OPERATOR);
  }

  isVisitor(): boolean {
    return this.hasRole(Roles.VISITOR);
  }

  getCurrentUser(): LoginResponse | null {
    return this.currentUserSubject.value;
  }

  getUserId(): string | null {
    const user = this.currentUserSubject.value;
    return user?.id || null;
  }

  getDashboardRoute(): string {
    if (this.isAdministrator()) {
      return '/admin/dashboard';
    } else if (this.isOperator()) {
      return '/operator/dashboard';
    } else if (this.isVisitor()) {
      return '/visitor/dashboard';
    }
    return '/login';
  }
}
