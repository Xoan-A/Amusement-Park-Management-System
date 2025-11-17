import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScoreHistoryService } from '../../../core/services/score-history.service';
import { ScoreHistoryResponse } from '../../../core/models/responses';
import { ScoreOrigin } from '../../../core/models/enums';

@Component({
  selector: 'app-all-score-history',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container mt-4">
      <h2 class="mb-4">All Score History</h2>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row">
            <div class="col-md-4">
              <label for="searchVisitor" class="form-label"
                >Search Visitor</label
              >
              <input
                type="text"
                id="searchVisitor"
                class="form-control"
                [(ngModel)]="searchTerm"
                placeholder="Search by name or ID..."
                (input)="filterRecords()"
              />
            </div>
            <div class="col-md-4">
              <label for="originFilter" class="form-label">Origin</label>
              <select
                id="originFilter"
                class="form-select"
                [(ngModel)]="selectedOrigin"
                (change)="filterRecords()"
              >
                <option value="">All Origins</option>
                <option value="AttractionVisit">Attraction Visit</option>
                <option value="EventParticipation">Event Participation</option>
                <option value="SpecialMission">Special Mission</option>
                <option value="Redemption">Redemption</option>
                <option value="AdminAdjustment">Admin Adjustment</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div class="col-md-4">
              <label for="strategyFilter" class="form-label">Strategy</label>
              <input
                type="text"
                id="strategyFilter"
                class="form-control"
                [(ngModel)]="selectedStrategy"
                placeholder="Filter by strategy..."
                (input)="filterRecords()"
              />
            </div>
          </div>
          <div class="row mt-2">
            <div class="col-md-12">
              <button
                class="btn btn-outline-secondary btn-sm"
                (click)="clearFilters()"
              >
                <i class="bi bi-x-circle"></i> Clear Filters
              </button>
              <button
                class="btn btn-primary btn-sm ms-2"
                (click)="exportToCSV()"
              >
                <i class="bi bi-download"></i> Export to CSV
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Error Message -->
      @if (errorMessage) {
      <div class="alert alert-danger alert-dismissible fade show" role="alert">
        {{ errorMessage }}
        <button
          type="button"
          class="btn-close"
          (click)="errorMessage = null"
        ></button>
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

      <!-- Statistics -->
      @if (!loading && allHistory.length > 0) {
      <div class="row mb-4">
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Total Records</h5>
              <p class="card-text display-6">{{ filteredHistory.length }}</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Total Points Awarded</h5>
              <p class="card-text display-6">{{ getTotalPoints() }}</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Points Redeemed</h5>
              <p class="card-text display-6 text-danger">
                {{ getRedeemedPoints() }}
              </p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Unique Visitors</h5>
              <p class="card-text display-6">{{ getUniqueVisitors() }}</p>
            </div>
          </div>
        </div>
      </div>
      }

      <!-- History Table -->
      @if (!loading && filteredHistory.length > 0) {
        <div class="card">
          <div class="card-body">
            <div class="table-responsive">
              <table class="table table-hover">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Visitor</th>
                    <th>Points</th>
                    <th>Origin</th>
                    <th>Related Entity</th>
                    <th>Strategy</th>
                  </tr>
                </thead>
                <tbody>
                  @for (record of filteredHistory; track record.id) {
                    <tr>
                      <td>{{ record.createdAt | date:'d/M/yyyy HH:mm' }}</td>
                      <td>{{ record.visitorName || 'Unknown' }}</td>
                      <td>
                        <span [class]="getPointsClass(record.points)">
                          {{ record.points > 0 ? '+' + record.points : record.points }}
                        </span>
                      </td>
                      <td>
                        <span class="badge" [class]="getOriginBadgeClass(record.origin)">
                          {{ formatOrigin(record.origin) }}
                        </span>
                      </td>
                      <td>
                        @if (record.relatedEntityName) {
                          <small>{{ record.relatedEntityName }}</small>
                        } @else {
                          <small class="text-muted">—</small>
                        }
                      </td>
                      <td>{{ record.strategyName }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        </div>
      }

      <!-- No Results -->
      @if (!loading && filteredHistory.length === 0 && allHistory.length > 0) {
      <div class="alert alert-warning">
        <i class="bi bi-search"></i> No records match your filters.
      </div>
      } @if (!loading && allHistory.length === 0) {
      <div class="alert alert-info">
        <i class="bi bi-info-circle"></i> No score history records found.
      </div>
      }
    </div>
  `,
})
export class AllScoreHistoryComponent implements OnInit {
  private scoreHistoryService = inject(ScoreHistoryService);

  allHistory: ScoreHistoryResponse[] = [];
  filteredHistory: ScoreHistoryResponse[] = [];
  loading = false;
  errorMessage: string | null = null;

  searchTerm = '';
  selectedOrigin = '';
  selectedStrategy = '';

  ngOnInit() {
    this.loadHistory();
  }

  loadHistory() {
    this.loading = true;
    this.errorMessage = null;

    this.scoreHistoryService.getAllScoreHistory().subscribe({
      next: (history) => {
        this.allHistory = history;
        this.filteredHistory = history;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load score history.';
        this.loading = false;
      },
    });
  }

  filterRecords() {
    this.filteredHistory = this.allHistory.filter((record) => {
      const matchesSearch =
        !this.searchTerm ||
        record.visitorName
          ?.toLowerCase()
          .includes(this.searchTerm.toLowerCase()) ||
        record.visitorId?.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesOrigin =
        !this.selectedOrigin || record.origin === this.selectedOrigin;

      const matchesStrategy =
        !this.selectedStrategy ||
        record.strategyName
          .toLowerCase()
          .includes(this.selectedStrategy.toLowerCase());

      return matchesSearch && matchesOrigin && matchesStrategy;
    });
  }

  clearFilters() {
    this.searchTerm = '';
    this.selectedOrigin = '';
    this.selectedStrategy = '';
    this.filteredHistory = this.allHistory;
  }

  getTotalPoints(): number {
    return this.filteredHistory
      .filter((r) => r.points > 0)
      .reduce((sum, r) => sum + r.points, 0);
  }

  getRedeemedPoints(): number {
    return Math.abs(
      this.filteredHistory
        .filter((r) => r.points < 0)
        .reduce((sum, r) => sum + r.points, 0)
    );
  }

  getUniqueVisitors(): number {
    const uniqueIds = new Set(
      this.filteredHistory.map((r) => r.visitorId).filter((id) => id)
    );
    return uniqueIds.size;
  }

  getPointsClass(points: number): string {
    return points > 0 ? 'text-success fw-bold' : 'text-danger fw-bold';
  }

  getOriginBadgeClass(origin: string): string {
    switch (origin) {
      case 'AttractionVisit':
        return 'bg-primary';
      case 'EventParticipation':
        return 'bg-info';
      case 'SpecialMission':
        return 'bg-warning';
      case 'Redemption':
        return 'bg-danger';
      case 'AdminAdjustment':
        return 'bg-secondary';
      default:
        return 'bg-secondary';
    }
  }

  formatOrigin(origin: string): string {
    return origin.replace(/([A-Z])/g, ' $1').trim();
  }

  exportToCSV() {
    const headers = ['Date', 'Visitor', 'Points', 'Origin', 'Strategy'];
    const rows = this.filteredHistory.map((r) => [
      new Date(r.createdAt).toLocaleString(),
      r.visitorName || 'Unknown',
      r.points.toString(),
      this.formatOrigin(r.origin),
      r.strategyName,
    ]);

    const csvContent = [
      headers.join(','),
      ...rows.map((row) => row.map((cell) => `"${cell}"`).join(',')),
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `score-history-${
      new Date().toISOString().split('T')[0]
    }.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
