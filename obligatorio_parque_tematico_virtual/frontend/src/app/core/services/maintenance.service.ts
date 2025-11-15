import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MaintenanceScheduleRequest, UpdateStatusRequest } from '../models/requests';
import { MaintenanceScheduleResponse, MessageResponse } from '../models/responses';

@Injectable({
  providedIn: 'root'
})
export class MaintenanceService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/maintenance`;

  // Schedule endpoints
  createSchedule(request: MaintenanceScheduleRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/schedules`, request);
  }

  getAllSchedules(params?: { attractionId?: string; status?: string; dateFrom?: string; dateTo?: string }): Observable<MaintenanceScheduleResponse[]> {
    let httpParams = new HttpParams();
    if (params?.attractionId) httpParams = httpParams.set('attractionId', params.attractionId);
    if (params?.status) httpParams = httpParams.set('status', params.status);
    if (params?.dateFrom) httpParams = httpParams.set('dateFrom', params.dateFrom);
    if (params?.dateTo) httpParams = httpParams.set('dateTo', params.dateTo);

    return this.http.get<MaintenanceScheduleResponse[]>(`${this.apiUrl}/schedules`, { params: httpParams });
  }

  getScheduleById(id: string): Observable<MaintenanceScheduleResponse> {
    return this.http.get<MaintenanceScheduleResponse>(`${this.apiUrl}/schedules/${id}`);
  }

  getSchedulesByAttraction(attractionId: string): Observable<MaintenanceScheduleResponse[]> {
    return this.http.get<MaintenanceScheduleResponse[]>(`${this.apiUrl}/schedules/attraction/${attractionId}`);
  }

  getOverdueSchedules(): Observable<MaintenanceScheduleResponse[]> {
    return this.http.get<MaintenanceScheduleResponse[]>(`${this.apiUrl}/schedules/overdue`);
  }

  getUpcomingSchedules(days: number = 7): Observable<MaintenanceScheduleResponse[]> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<MaintenanceScheduleResponse[]>(`${this.apiUrl}/schedules/upcoming`, { params });
  }

  updateScheduleStatus(id: string, request: UpdateStatusRequest): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.apiUrl}/schedules/${id}/status`, request);
  }

  deleteSchedule(id: string): Observable<MessageResponse> {
    return this.http.delete<MessageResponse>(`${this.apiUrl}/schedules/${id}`);
  }
}
