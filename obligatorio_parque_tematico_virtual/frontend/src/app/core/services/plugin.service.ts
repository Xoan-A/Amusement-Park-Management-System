import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PluginResponse } from '../models/responses';

@Injectable({
  providedIn: 'root',
})
export class PluginService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/plugins`;

  getAvailablePlugins(): Observable<PluginResponse[]> {
    return this.http.get<PluginResponse[]>(this.apiUrl);
  }

  getPluginByName(name: string): Observable<PluginResponse> {
    return this.http.get<PluginResponse>(`${this.apiUrl}/${name}`);
  }

  uploadPlugin(file: File): Observable<HttpResponse<object>> {
    const formData = new FormData();
    formData.append('dllFile', file, file.name);
    return this.http.post<object>(this.apiUrl, formData, {
      observe: 'response',
    });
  }
}
