import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { QRCodeComponent } from 'angularx-qrcode';
import { NavbarComponent } from '../../../../shared/components/navbar/navbar.component';
import { TicketService } from '../../../../core/services/ticket.service';
import { AuthService } from '../../../../core/services/auth.service';
import { TicketResponse } from '../../../../core/models';

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule, RouterLink, QRCodeComponent, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
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
                    <p class="mb-1"><strong>Purchase Date:</strong> {{ ticket.purchaseDate | date:'short' }}</p>
                    <p class="mb-1"><strong>Visit Date:</strong> {{ ticket.visitDate | date:'short' }}</p>
                    <p class="mb-1">
                      <strong>Type:</strong>
                      <span class="badge" [class.bg-primary]="ticket.type === 0" [class.bg-success]="ticket.type === 1">
                        {{ ticket.type === 0 ? 'General' : 'Event Special' }}
                      </span>
                    </p>
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
  loading = true;

  constructor(
    private ticketService: TicketService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const userId = this.authService.getUserId();
    if (userId) {
      this.loadTickets(userId);
    } else {
      this.loading = false;
    }
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
