import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PluginResponse } from '../models/responses';

@Injectable({
  providedIn: 'root'
})
export class PluginService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/plugins`;

  // Get all available plugins
  getAvailablePlugins(): Observable<PluginResponse[]> {
    return this.http.get<PluginResponse[]>(this.apiUrl);
  }

  // Get specific plugin by name
  getPluginByName(name: string): Observable<PluginResponse> {
    return this.http.get<PluginResponse>(`${this.apiUrl}/${name}`);
  }
}
