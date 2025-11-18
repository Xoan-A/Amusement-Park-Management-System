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
  templateUrl: './my-tickets.component.html',
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
