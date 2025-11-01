import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScoreHistoryService } from '../../../core/services/score-history.service';
import { ScoreHistoryResponse } from '../../../core/models/responses';
import { ScoreOrigin } from '../../../core/models/enums';

@Component({
  selector: 'app-score-history',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container mt-4">
      <h2 class="mb-4">My Score History</h2>

      <!-- Error Message -->
      @if (errorMessage) {
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
          {{ errorMessage }}
          <button type="button" class="btn-close" (click)="errorMessage = null"></button>
        </div>
      }

      <!-- Loading -->
      @if (loading) {
        <div class="text-center my-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>
      }

      <!-- Statistics Cards -->
      @if (!loading && history.length > 0) {
        <div class="row mb-4">
          <div class="col-md-4">
            <div class="card text-center border-primary">
              <div class="card-body">
                <h5 class="card-title text-primary">Total Score</h5>
                <p class="card-text display-4 fw-bold">{{ getTotalScore() }}</p>
              </div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="card text-center border-success">
              <div class="card-body">
                <h5 class="card-title text-success">Points Earned</h5>
                <p class="card-text display-5">{{ getPointsEarned() }}</p>
              </div>
            </div>
          </div>
          <div class="col-md-4">
            <div class="card text-center border-danger">
              <div class="card-body">
                <h5 class="card-title text-danger">Points Spent</h5>
                <p class="card-text display-5">{{ getPointsSpent() }}</p>
              </div>
            </div>
          </div>
        </div>
      }

      <!-- Filters -->
      @if (!loading && history.length > 0) {
        <div class="card mb-4">
          <div class="card-body">
            <div class="row">
              <div class="col-md-4">
                <label for="dateFrom" class="form-label">From Date</label>
                <input type="date" id="dateFrom" class="form-control" [(ngModel)]="dateFrom" (change)="filterRecords()">
              </div>
              <div class="col-md-4">
                <label for="dateTo" class="form-label">To Date</label>
                <input type="date" id="dateTo" class="form-control" [(ngModel)]="dateTo" (change)="filterRecords()">
              </div>
              <div class="col-md-4">
                <label for="originFilter" class="form-label">Origin Type</label>
                <select id="originFilter" class="form-select" [(ngModel)]="selectedOrigin" (change)="filterRecords()">
                  <option value="">All Types</option>
                  <option value="AttractionVisit">Attraction Visit</option>
                  <option value="EventParticipation">Event Participation</option>
                  <option value="SpecialMission">Special Mission</option>
                  <option value="Redemption">Redemption</option>
                  <option value="AdminAdjustment">Admin Adjustment</option>
                  <option value="Other">Other</option>
                </select>
              </div>
            </div>
            <div class="row mt-2">
              <div class="col-md-12">
                <button class="btn btn-outline-secondary btn-sm" (click)="clearFilters()">
                  <i class="bi bi-x-circle"></i> Clear Filters
                </button>
              </div>
            </div>
          </div>
        </div>
      }

      <!-- Origin Summary -->
      @if (!loading && filteredHistory.length > 0) {
        <div class="card mb-4">
          <div class="card-header">
            <h5 class="mb-0">Points by Category</h5>
          </div>
          <div class="card-body">
            <div class="row">
              @for (summary of getOriginSummary(); track summary.origin) {
                <div class="col-md-3 mb-2">
                  <div class="d-flex justify-content-between align-items-center">
                    <span class="badge" [class]="getOriginBadgeClass(summary.origin)">
                      {{ formatOrigin(summary.origin) }}
                    </span>
                    <span class="fw-bold" [class]="getPointsClass(summary.total)">
                      {{ summary.total > 0 ? '+' + summary.total : summary.total }}
                    </span>
                  </div>
                </div>
              }
            </div>
          </div>
        </div>
      }

      <!-- History Timeline -->
      @if (!loading && filteredHistory.length > 0) {
        <div class="card">
          <div class="card-header">
            <h5 class="mb-0">
              <i class="bi bi-clock-history"></i> Activity Timeline
              <span class="badge bg-secondary ms-2">{{ filteredHistory.length }} records</span>
            </h5>
          </div>
          <div class="card-body">
            <div class="table-responsive">
              <table class="table table-hover">
                <thead>
                  <tr>
                    <th>Date & Time</th>
                    <th>Points</th>
                    <th>Category</th>
                    <th>Strategy</th>
                    <th>Description</th>
                  </tr>
                </thead>
                <tbody>
                  @for (record of filteredHistory; track record.id) {
                    <tr>
                      <td>{{ record.createdAt | date:'medium' }}</td>
                      <td>
                        <span class="badge" [class]="getPointsBadgeClass(record.points)">
                          {{ record.points > 0 ? '+' + record.points : record.points }}
                        </span>
                      </td>
                      <td>
                        <span class="badge" [class]="getOriginBadgeClass(record.origin)">
                          {{ formatOrigin(record.origin) }}
                        </span>
                      </td>
                      <td>
                        <small class="text-muted">{{ record.strategyName }}</small>
                      </td>
                      <td>{{ record.description }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        </div>
      }

      <!-- No Results -->
      @if (!loading && filteredHistory.length === 0 && history.length > 0) {
        <div class="alert alert-warning">
          <i class="bi bi-search"></i> No records match your filters.
        </div>
      }

      @if (!loading && history.length === 0) {
        <div class="alert alert-info">
          <i class="bi bi-info-circle"></i> You don't have any score history yet. Start visiting attractions and participating in events to earn points!
        </div>
      }
    </div>
  `
})
export class ScoreHistoryComponent implements OnInit {
  private scoreHistoryService = inject(ScoreHistoryService);

  history: ScoreHistoryResponse[] = [];
  filteredHistory: ScoreHistoryResponse[] = [];
  loading = false;
  errorMessage: string | null = null;

  // Filters
  dateFrom = '';
  dateTo = '';
  selectedOrigin = '';

  ngOnInit() {
    this.loadHistory();
  }

  loadHistory() {
    this.loading = true;
    this.errorMessage = null;

    this.scoreHistoryService.getMyScoreHistory().subscribe({
      next: (history) => {
        // Sort by date descending (most recent first)
        this.history = history.sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.filteredHistory = this.history;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load your score history.';
        this.loading = false;
      }
    });
  }

  filterRecords() {
    this.filteredHistory = this.history.filter(record => {
      const recordDate = new Date(record.createdAt);

      const matchesDateFrom = !this.dateFrom ||
        recordDate >= new Date(this.dateFrom);

      const matchesDateTo = !this.dateTo ||
        recordDate <= new Date(this.dateTo + 'T23:59:59');

      const matchesOrigin = !this.selectedOrigin ||
        record.origin === this.selectedOrigin;

      return matchesDateFrom && matchesDateTo && matchesOrigin;
    });
  }

  clearFilters() {
    this.dateFrom = '';
    this.dateTo = '';
    this.selectedOrigin = '';
    this.filteredHistory = this.history;
  }

  getTotalScore(): number {
    return this.filteredHistory.reduce((sum, r) => sum + r.points, 0);
  }

  getPointsEarned(): number {
    return this.filteredHistory
      .filter(r => r.points > 0)
      .reduce((sum, r) => sum + r.points, 0);
  }

  getPointsSpent(): number {
    return Math.abs(this.filteredHistory
      .filter(r => r.points < 0)
      .reduce((sum, r) => sum + r.points, 0));
  }

  getOriginSummary(): Array<{ origin: string; total: number }> {
    const summary = new Map<string, number>();

    this.filteredHistory.forEach(record => {
      const current = summary.get(record.origin) || 0;
      summary.set(record.origin, current + record.points);
    });

    return Array.from(summary.entries())
      .map(([origin, total]) => ({ origin, total }))
      .sort((a, b) => b.total - a.total);
  }

  getPointsClass(points: number): string {
    return points > 0 ? 'text-success' : 'text-danger';
  }

  getPointsBadgeClass(points: number): string {
    return points > 0 ? 'bg-success' : 'bg-danger';
  }

  getOriginBadgeClass(origin: string): string {
    switch (origin) {
      case 'AttractionVisit': return 'bg-primary';
      case 'EventParticipation': return 'bg-info';
      case 'SpecialMission': return 'bg-warning text-dark';
      case 'Redemption': return 'bg-danger';
      case 'AdminAdjustment': return 'bg-secondary';
      default: return 'bg-secondary';
    }
  }

  formatOrigin(origin: string): string {
    return origin.replace(/([A-Z])/g, ' $1').trim();
  }
}
