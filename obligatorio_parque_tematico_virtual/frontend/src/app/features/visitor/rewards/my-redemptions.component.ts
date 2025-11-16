import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { RedemptionService } from '../../../core/services/redemption.service';
import { RedemptionHistoryResponse } from '../../../core/models';

@Component({
  selector: 'app-my-redemptions',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ],
  template: `
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1>My Redemption History</h1>
        <a routerLink="/visitor/rewards" class="btn btn-primary">
          Browse Rewards
        </a>
      </div>

      <div class="card mb-4">
        <div class="card-body">
          <h5 class="card-title mb-3">Filter by Date Range</h5>
          <div class="row g-3">
            <div class="col-md-4">
              <label class="form-label">From Date</label>
              <input
                type="date"
                class="form-control"
                [(ngModel)]="dateFrom"
              />
            </div>
            <div class="col-md-4">
              <label class="form-label">To Date</label>
              <input
                type="date"
                class="form-control"
                [(ngModel)]="dateTo"
              />
            </div>
            <div class="col-md-4 d-flex align-items-end">
              <button class="btn btn-primary me-2" (click)="applyFilter()">
                Apply Filter
              </button>
              <button class="btn btn-outline-secondary" (click)="clearFilter()">
                Clear
              </button>
            </div>
          </div>
        </div>
      </div>

      @if (errorMessage) {
        <div class="alert alert-danger alert-dismissible">
          {{ errorMessage }}
          <button type="button" class="btn-close" (click)="errorMessage=''"></button>
        </div>
      }

      @if (loading) {
        <div class="text-center my-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>
      } @else {
        @if (redemptions.length === 0) {
          <div class="alert alert-info">
            @if (dateFrom || dateTo) {
              No redemptions found for the selected date range.
            } @else {
              You haven't redeemed any rewards yet. Start browsing available rewards!
            }
          </div>
        } @else {
          <div class="card">
            <div class="card-body">
              <h5 class="card-title mb-3">
                Total Redemptions: {{ redemptions.length }}
                <span class="text-muted fs-6">
                  ({{ getTotalPointsSpent() }} points spent)
                </span>
              </h5>

              <div class="table-responsive">
                <table class="table table-hover">
                  <thead class="table-light">
                    <tr>
                      <th>Date</th>
                      <th>Reward</th>
                      <th>Points Spent</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (redemption of redemptions; track redemption.id) {
                      <tr>
                        <td>{{ formatDate(redemption.redeemedAt) }}</td>
                        <td><strong>{{ redemption.rewardName || 'Unknown Reward' }}</strong></td>
                        <td>
                          <span class="badge bg-info">{{ redemption.pointsSpent }} pts</span>
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .table-responsive {
      box-shadow: 0 0 10px rgba(0,0,0,0.1);
      border-radius: 8px;
      overflow: hidden;
    }
  `]
})
export class MyRedemptionsComponent implements OnInit {
  redemptions: RedemptionHistoryResponse[] = [];
  loading = false;
  errorMessage = '';
  dateFrom = '';
  dateTo = '';

  constructor(private redemptionService: RedemptionService) {}

  ngOnInit(): void {
    this.loadRedemptions();
  }

  loadRedemptions(): void {
    this.loading = true;
    this.errorMessage = '';

    const fromDate = this.dateFrom ? new Date(this.dateFrom).toISOString() : undefined;
    const toDate = this.dateTo ? new Date(this.dateTo).toISOString() : undefined;

    this.redemptionService.getMyHistory(fromDate, toDate).subscribe({
      next: (redemptions) => {
        this.redemptions = redemptions;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load redemption history';
        this.loading = false;
      }
    });
  }

  applyFilter(): void {
    if (this.dateFrom && this.dateTo && this.dateFrom > this.dateTo) {
      this.errorMessage = '"From Date" must be before or equal to "To Date"';
      return;
    }
    this.loadRedemptions();
  }

  clearFilter(): void {
    this.dateFrom = '';
    this.dateTo = '';
    this.loadRedemptions();
  }

  getTotalPointsSpent(): number {
    return this.redemptions.reduce((total, r) => total + r.pointsSpent, 0);
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
