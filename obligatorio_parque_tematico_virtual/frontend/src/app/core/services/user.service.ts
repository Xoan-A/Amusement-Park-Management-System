import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  CreateUserRequest,
  ModifyUserRequest,
  AddRolesRequest,
  ChangeMembershipLevelRequest,
  UserResponse,
  MessageResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getById(userId: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.apiUrl}/${userId}`);
  }

  create(user: CreateUserRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(this.apiUrl, user);
  }

  update(userId: string, user: ModifyUserRequest): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.apiUrl}/${userId}`, user);
  }

  addRole(userId: string, role: AddRolesRequest): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.apiUrl}/${userId}/roles`, role);
  }

  changeMembershipLevel(userId: string, request: ChangeMembershipLevelRequest): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.apiUrl}/${userId}/membership`, request);
  }

  getAll(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(this.apiUrl);
  }
}
