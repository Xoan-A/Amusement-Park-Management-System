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
  templateUrl: './dashboard.component.html',
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
