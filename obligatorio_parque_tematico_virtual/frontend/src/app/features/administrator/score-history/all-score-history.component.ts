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
  templateUrl: './all-score-history.component.html',
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
