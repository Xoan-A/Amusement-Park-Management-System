import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  EventRequest,
  EventResponse,
  CreateEventResponse,
  MessageResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private apiUrl = `${environment.apiUrl}/events`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<EventResponse[]> {
    return this.http.get<EventResponse[]>(this.apiUrl);
  }

  getById(id: string): Observable<EventResponse> {
    return this.http.get<EventResponse>(`${this.apiUrl}/${id}`);
  }

  create(event: EventRequest): Observable<CreateEventResponse> {
    return this.http.post<CreateEventResponse>(this.apiUrl, event);
  }

  delete(id: string): Observable<MessageResponse> {
    return this.http.delete<MessageResponse>(`${this.apiUrl}/${id}`);
  }
}
