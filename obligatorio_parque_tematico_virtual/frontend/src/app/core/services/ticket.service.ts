import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  PurchaseTicketRequest,
  TicketResponse,
  MessageResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private apiUrl = `${environment.apiUrl}/tickets`;

  constructor(private http: HttpClient) {}

  purchase(ticketData: PurchaseTicketRequest): Observable<TicketResponse> {
    return this.http.post<TicketResponse>(this.apiUrl, ticketData);
  }

  getById(id: string): Observable<TicketResponse> {
    return this.http.get<TicketResponse>(`${this.apiUrl}/${id}`);
  }

  getByQrCode(qrCode: string): Observable<TicketResponse> {
    return this.http.get<TicketResponse>(`${this.apiUrl}/qr/${qrCode}`);
  }

  getByVisitorId(visitorId: string): Observable<TicketResponse[]> {
    return this.http.get<TicketResponse[]>(`${this.apiUrl}/visitor/${visitorId}`);
  }
}
