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
  templateUrl: './my-redemptions.component.html',
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
    return date.toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
