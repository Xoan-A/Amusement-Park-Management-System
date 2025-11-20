import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  RewardRequest,
  RewardResponse,
  AllRewardsResponse,
  CreateRewardResponse,
  MessageResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class RewardService {
  private apiUrl = `${environment.apiUrl}/rewards`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<RewardResponse[]> {
    return this.http.get<RewardResponse[]>(this.apiUrl);
  }

  getById(id: string): Observable<RewardResponse> {
    return this.http.get<RewardResponse>(`${this.apiUrl}/${id}`);
  }

  getAvailable(): Observable<RewardResponse[]> {
    return this.http.get<RewardResponse[]>(`${this.apiUrl}/available`);
  }

  create(reward: RewardRequest): Observable<CreateRewardResponse> {
    return this.http.post<CreateRewardResponse>(this.apiUrl, reward);
  }

  update(id: string, reward: RewardRequest): Observable<RewardResponse> {
    return this.http.put<RewardResponse>(`${this.apiUrl}/${id}`, reward);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
