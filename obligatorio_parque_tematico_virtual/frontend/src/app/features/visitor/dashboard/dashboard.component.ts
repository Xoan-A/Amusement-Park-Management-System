import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TicketService } from '../../../core/services/ticket.service';
import { AuthService } from '../../../core/services/auth.service';
import { ScoreHistoryService } from '../../../core/services/score-history.service';
import { TicketResponse, ScoreHistoryResponse, TicketType } from '../../../core/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, ],
  template: `
    <div class="container mt-4">
      <h1 class="mb-4">Visitor Dashboard</h1>

      <div class="row g-4 mb-4">
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">My Tickets</h5>
              <p class="display-4 text-primary">{{ tickets.length }}</p>
              <a routerLink="/visitor/tickets" class="btn btn-primary btn-sm">View All</a>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center border-success">
            <div class="card-body">
              <h5 class="card-title">My Total Score</h5>
              <p class="display-4 text-success">{{ totalScore }}</p>
              <a routerLink="/visitor/score-history" class="btn btn-success btn-sm">View History</a>
            </div>
          </div>
        </div>
        <div class="col-md-3">
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
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Quick Purchase</h5>
              <p class="my-3">Buy tickets now</p>
              <a routerLink="/visitor/tickets/purchase" class="btn btn-warning btn-sm">Purchase Ticket</a>
            </div>
          </div>
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Recent Score Activity</h5>
            </div>
            <div class="card-body">
              @if (recentScores.length === 0) {
                <p class="text-muted">No score activity yet. Start visiting attractions to earn points!</p>
                <a routerLink="/visitor/attractions" class="btn btn-primary">Browse Attractions</a>
              } @else {
                <div class="list-group">
                  @for (score of recentScores; track score.id) {
                    <div class="list-group-item">
                      <div class="d-flex w-100 justify-content-between align-items-start">
                        <div class="flex-grow-1">
                          <span class="badge" [class.bg-success]="score.points > 0" [class.bg-danger]="score.points < 0">
                            {{ score.points > 0 ? '+' + score.points : score.points }}
                          </span>
                          <small class="ms-2">{{ score.origin }} - {{ score.strategyName }}</small>
                          @if (score.relatedEntityName) {
                            <div class="mt-1">
                              <small class="text-muted d-block">{{ score.relatedEntityName }}</small>
                            </div>
                          }
                        </div>
                        <small class="text-muted ms-2" style="white-space: nowrap;">{{ score.createdAt | date:'short' }}</small>
                      </div>
                    </div>
                  }
                </div>
                <div class="mt-2">
                  <a routerLink="/visitor/score-history" class="btn btn-sm btn-outline-primary">View Full History</a>
                </div>
              }
            </div>
          </div>
        </div>

        <div class="col-md-6">
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
                  <table class="table table-sm">
                    <thead>
                      <tr>
                        <th>Visit Date</th>
                        <th>Type</th>
                      </tr>
                    </thead>
                    <tbody>
                      @for (ticket of tickets.slice(0, 5); track ticket.id) {
                        <tr>
                          <td>{{ ticket.visitDate | date:'short' }}</td>
                          <td>
                            <span class="badge" [class.bg-primary]="ticket.type === ticketType.General" [class.bg-success]="ticket.type === ticketType.EventSpecial">
                              {{ ticket.type === ticketType.General ? 'General' : 'Event Special' }}
                            </span>
                          </td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
                <div class="mt-2">
                  <a routerLink="/visitor/tickets" class="btn btn-sm btn-outline-primary">View All Tickets</a>
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
  recentScores: ScoreHistoryResponse[] = [];
  ticketType = TicketType;
  totalScore = 0;

  constructor(
    private ticketService: TicketService,
    private authService: AuthService,
    private scoreHistoryService: ScoreHistoryService
  ) {}

  ngOnInit(): void {
    const userId = this.authService.getUserId();
    if (userId) {
      this.loadTickets(userId);
    }
    this.loadScoreHistory();
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

  loadScoreHistory(): void {
    this.scoreHistoryService.getMyScoreHistory().subscribe({
      next: (history) => {
        this.recentScores = history
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
          .slice(0, 5);

        this.totalScore = history.reduce(
          (sum, record) => sum + record.points,
          0
        );
      },
      error: (error) => {
        console.error('Error loading score history', error);
      }
    });
  }
}
