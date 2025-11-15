import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { DateTimeResponse, MessageResponse } from '../models/responses';
import { SetDateTimeRequest } from '../models/requests';

@Injectable({
  providedIn: 'root'
})
export class DateTimeService {
  private apiUrl = `${environment.apiUrl}/datetime`;

  constructor(private http: HttpClient) {}

  getCurrentDateTime(): Observable<DateTimeResponse> {
    return this.http.get<DateTimeResponse>(this.apiUrl);
  }

  setDateTime(dateTime: string): Observable<void> {
    const request: SetDateTimeRequest = { dateTime };
    return this.http.put<void>(this.apiUrl, request);
  }
}
