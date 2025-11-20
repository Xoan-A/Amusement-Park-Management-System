import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ScoreHistoryResponse } from '../models/responses';

@Injectable({
  providedIn: 'root',
})
export class ScoreHistoryService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/score-history`;

  getMyScoreHistory(): Observable<ScoreHistoryResponse[]> {
    return this.http.get<ScoreHistoryResponse[]>(`${this.apiUrl}/my-history`);
  }

  getVisitorScoreHistory(
    visitorId: string,
    dateFrom?: string,
    dateTo?: string
  ): Observable<ScoreHistoryResponse[]> {
    let params = new HttpParams();
    if (dateFrom) params = params.set('dateFrom', dateFrom);
    if (dateTo) params = params.set('dateTo', dateTo);

    return this.http.get<ScoreHistoryResponse[]>(
      `${this.apiUrl}/visitor/${visitorId}`,
      { params }
    );
  }

  getAllScoreHistory(): Observable<ScoreHistoryResponse[]> {
    return this.http.get<ScoreHistoryResponse[]>(this.apiUrl);
  }
}
