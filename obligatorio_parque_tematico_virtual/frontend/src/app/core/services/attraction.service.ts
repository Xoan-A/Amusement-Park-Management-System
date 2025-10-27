import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  AttractionRequest,
  AttractionResponse,
  AllAttractionsResponse,
  CreateAttractionResponse,
  CapacityResponse,
  MessageResponse,
  RegisterEntryRequest,
  RegisterExitRequest,
  AttractionsVisitsRequest,
  AttractionsVisitResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class AttractionService {
  private apiUrl = `${environment.apiUrl}/attractions`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AllAttractionsResponse> {
    return this.http.get<AllAttractionsResponse>(this.apiUrl);
  }

  getById(id: string): Observable<AttractionResponse> {
    return this.http.get<AttractionResponse>(`${this.apiUrl}/${id}`);
  }

  create(attraction: AttractionRequest): Observable<CreateAttractionResponse> {
    return this.http.post<CreateAttractionResponse>(this.apiUrl, attraction);
  }

  update(id: string, attraction: AttractionRequest): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.apiUrl}/${id}`, attraction);
  }

  delete(id: string): Observable<MessageResponse> {
    return this.http.delete<MessageResponse>(`${this.apiUrl}/${id}`);
  }

  registerEntry(id: string, request: RegisterEntryRequest): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.apiUrl}/entry/${id}`, request);
  }

  registerExit(id: string, request: RegisterExitRequest): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.apiUrl}/exit/${id}`, request);
  }

  getCapacity(id: string): Observable<CapacityResponse> {
    return this.http.get<CapacityResponse>(`${this.apiUrl}/capacity/${id}`);
  }

  getVisitsReport(request: AttractionsVisitsRequest): Observable<AttractionsVisitResponse> {
    const params = new HttpParams()
      .set('startDate', request.startDate)
      .set('endDate', request.endDate);
    return this.http.get<AttractionsVisitResponse>(`${this.apiUrl}/visits`, { params });
  }
}
