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
  templateUrl: './score-history.component.html',
})
export class ScoreHistoryComponent implements OnInit {
  private scoreHistoryService = inject(ScoreHistoryService);

  history: ScoreHistoryResponse[] = [];
  filteredHistory: ScoreHistoryResponse[] = [];
  loading = false;
  errorMessage: string | null = null;

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
        this.history = history.sort(
          (a, b) =>
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.filteredHistory = this.history;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load your score history.';
        this.loading = false;
      },
    });
  }

  filterRecords() {
    this.filteredHistory = this.history.filter((record) => {
      const recordDate = new Date(record.createdAt);

      const matchesDateFrom =
        !this.dateFrom || recordDate >= new Date(this.dateFrom);

      const matchesDateTo =
        !this.dateTo || recordDate <= new Date(this.dateTo + 'T23:59:59');

      const matchesOrigin =
        !this.selectedOrigin || record.origin === this.selectedOrigin;

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
      .filter((r) => r.points > 0)
      .reduce((sum, r) => sum + r.points, 0);
  }

  getPointsSpent(): number {
    return Math.abs(
      this.filteredHistory
        .filter((r) => r.points < 0)
        .reduce((sum, r) => sum + r.points, 0)
    );
  }

  getOriginSummary(): Array<{ origin: string; total: number }> {
    const summary = new Map<string, number>();

    this.filteredHistory.forEach((record) => {
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
      case 'AttractionVisit':
        return 'bg-primary';
      case 'EventParticipation':
        return 'bg-info';
      case 'SpecialMission':
        return 'bg-warning text-dark';
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
}
