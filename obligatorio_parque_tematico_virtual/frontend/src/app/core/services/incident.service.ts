import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  IncidentRequest,
  MessageResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class IncidentService {
  private apiUrl = `${environment.apiUrl}/incidents`;

  constructor(private http: HttpClient) {}

  getByAttractionId(attractionId: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/${attractionId}`);
  }

  addIncident(attractionId: string, incident: IncidentRequest): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.apiUrl}/${attractionId}`, incident);
  }

  removeIncident(attractionId: string, incident: IncidentRequest): Observable<MessageResponse> {
    return this.http.delete<MessageResponse>(
      `${this.apiUrl}/${attractionId}?incident=${encodeURIComponent(incident.incident)}`
    );
  }
}
