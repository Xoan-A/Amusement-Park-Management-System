import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { QRCodeComponent } from 'angularx-qrcode';
import { TicketService } from '../../../../core/services/ticket.service';
import { EventService } from '../../../../core/services/event.service';
import { AuthService } from '../../../../core/services/auth.service';
import { TicketResponse, TicketType, EventResponse } from '../../../../core/models';

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule, RouterLink, QRCodeComponent, ],
  template: `
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1>My Tickets</h1>
        <a routerLink="/visitor/tickets/purchase" class="btn btn-primary">Purchase New Ticket</a>
      </div>

      @if (loading) {
        <div class="text-center py-5">
          <div class="spinner-border text-primary"></div>
        </div>
      } @else if (tickets.length === 0) {
        <div class="alert alert-info">
          You don't have any tickets yet.
          <a routerLink="/visitor/tickets/purchase" class="alert-link">Purchase your first ticket!</a>
        </div>
      } @else {
        <div class="row">
          @for (ticket of tickets; track ticket.id) {
            <div class="col-md-6 col-lg-4 mb-4">
              <div class="card">
                <div class="card-body text-center">
                  <h5 class="card-title">Ticket #{{ ticket.id.substring(0, 8) }}</h5>

                  <div class="my-3">
                    <qrcode
                      [qrdata]="ticket.qrCode"
                      [width]="200"
                      [errorCorrectionLevel]="'M'"
                    ></qrcode>
                  </div>

                  <div class="text-start">
                    <p class="mb-1"><strong>Purchase Date:</strong> {{ ticket.purchaseDate | date:'d/M/yyyy HH:mm' }}</p>
                    <p class="mb-1"><strong>Visit Date:</strong> {{ ticket.visitDate | date:'d/M/yyyy' }}</p>
                    <p class="mb-1">
                      <strong>Type:</strong>
                      <span class="badge" [class.bg-primary]="ticket.type === ticketType.General" [class.bg-success]="ticket.type === ticketType.EventSpecial">
                        {{ ticket.type === ticketType.General ? 'General' : 'Event Special' }}
                      </span>
                    </p>
                    @if (ticket.type === ticketType.EventSpecial && ticket.eventId) {
                      <p class="mb-1"><strong>Event:</strong> {{ getEventName(ticket.eventId) }}</p>
                    }
                    <p class="mb-0"><strong>QR Code:</strong> {{ ticket.qrCode }}</p>
                  </div>
                </div>
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: []
})
export class MyTicketsComponent implements OnInit {
  tickets: TicketResponse[] = [];
  events: Map<string, EventResponse> = new Map();
  loading = true;
  ticketType = TicketType;

  constructor(
    private ticketService: TicketService,
    private eventService: EventService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const userId = this.authService.getUserId();
    if (userId) {
      this.loadEvents();
      this.loadTickets(userId);
    } else {
      this.loading = false;
    }
  }

  loadEvents(): void {
    this.eventService.getAll().subscribe({
      next: (events) => {
        events.forEach(event => {
          this.events.set(event.id, event);
        });
      },
      error: (error) => console.error('Error loading events', error)
    });
  }

  getEventName(eventId?: string): string {
    if (!eventId) return '';
    return this.events.get(eventId)?.name || 'Unknown Event';
  }

  loadTickets(visitorId: string): void {
    this.ticketService.getByVisitorId(visitorId).subscribe({
      next: (tickets) => {
        this.tickets = tickets || [];
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading tickets', error);
        this.loading = false;
      }
    });
  }
}
