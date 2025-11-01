import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  RedeemRewardRequest,
  RedemptionHistoryResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class RedemptionService {
  private apiUrl = `${environment.apiUrl}/redemptions`;

  constructor(private http: HttpClient) {}

  redeemReward(request: RedeemRewardRequest): Observable<RedemptionHistoryResponse> {
    return this.http.post<RedemptionHistoryResponse>(`${this.apiUrl}/redeem`, request);
  }

  getMyHistory(dateFrom?: string, dateTo?: string): Observable<RedemptionHistoryResponse[]> {
    let params = new HttpParams();
    if (dateFrom) {
      params = params.set('dateFrom', dateFrom);
    }
    if (dateTo) {
      params = params.set('dateTo', dateTo);
    }
    return this.http.get<RedemptionHistoryResponse[]>(`${this.apiUrl}/my-history`, { params });
  }

  getVisitorHistory(visitorId: string, dateFrom?: string, dateTo?: string): Observable<RedemptionHistoryResponse[]> {
    let params = new HttpParams();
    if (dateFrom) {
      params = params.set('dateFrom', dateFrom);
    }
    if (dateTo) {
      params = params.set('dateTo', dateTo);
    }
    return this.http.get<RedemptionHistoryResponse[]>(`${this.apiUrl}/visitor/${visitorId}/history`, { params });
  }
}
