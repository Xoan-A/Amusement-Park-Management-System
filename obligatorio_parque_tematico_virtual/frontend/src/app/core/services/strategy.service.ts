import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  StrategyRequest,
  StrategyResponse,
  TopTenResponse,
  MessageResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class StrategyService {
  private apiUrl = `${environment.apiUrl}/strategy`;

  constructor(private http: HttpClient) {}

  getCurrent(): Observable<StrategyResponse> {
    return this.http.get<StrategyResponse>(this.apiUrl);
  }

  setStrategy(strategy: StrategyRequest): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(this.apiUrl, strategy);
  }

  getTopTen(): Observable<TopTenResponse> {
    return this.http.get<TopTenResponse>(`${this.apiUrl}/topTen`);
  }
}
