import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { TicketService } from '../../../core/services/ticket.service';
import { AuthService } from '../../../core/services/auth.service';
import { TicketResponse } from '../../../core/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    <div class="container mt-4">
      <h1 class="mb-4">Visitor Dashboard</h1>

      <div class="row g-4 mb-4">
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">My Tickets</h5>
              <p class="display-4 text-primary">{{ tickets.length }}</p>
              <a routerLink="/visitor/tickets" class="btn btn-primary btn-sm">View All</a>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Browse</h5>
              <p class="my-3">Explore our attractions and events</p>
              <div class="d-flex gap-2 justify-content-center">
                <a routerLink="/visitor/attractions" class="btn btn-success btn-sm">Attractions</a>
                <a routerLink="/visitor/events" class="btn btn-info btn-sm">Events</a>
              </div>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Quick Purchase</h5>
              <p class="my-3">Buy tickets now</p>
              <a routerLink="/visitor/tickets/purchase" class="btn btn-warning btn-sm">Purchase Ticket</a>
            </div>
          </div>
        </div>
      </div>

      <div class="row">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">My Recent Tickets</h5>
            </div>
            <div class="card-body">
              @if (tickets.length === 0) {
                <p class="text-muted">You haven't purchased any tickets yet.</p>
                <a routerLink="/visitor/tickets/purchase" class="btn btn-primary">Purchase Your First Ticket</a>
              } @else {
                <div class="table-responsive">
                  <table class="table table-striped">
                    <thead>
                      <tr>
                        <th>Purchase Date</th>
                        <th>Visit Date</th>
                        <th>Type</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      @for (ticket of tickets.slice(0, 5); track ticket.id) {
                        <tr>
                          <td>{{ ticket.purchaseDate | date:'short' }}</td>
                          <td>{{ ticket.visitDate | date:'short' }}</td>
                          <td>
                            <span class="badge" [class.bg-primary]="ticket.type === 0" [class.bg-success]="ticket.type === 1">
                              {{ ticket.type === 0 ? 'General' : 'Event Special' }}
                            </span>
                          </td>
                          <td>
                            <a routerLink="/visitor/tickets" class="btn btn-sm btn-outline-primary">View QR</a>
                          </td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
              }
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class DashboardComponent implements OnInit {
  tickets: TicketResponse[] = [];

  constructor(
    private ticketService: TicketService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const userId = this.authService.getUserId();
    if (userId) {
      this.loadTickets(userId);
    }
  }

  loadTickets(visitorId: string): void {
    this.ticketService.getByVisitorId(visitorId).subscribe({
      next: (tickets) => {
        this.tickets = tickets || [];
      },
      error: (error) => {
        console.error('Error loading tickets', error);
      }
    });
  }
}
