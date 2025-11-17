import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StrategyService } from '../../../../core/services/strategy.service';
import { UserResponseData } from '../../../../core/models';

@Component({
  selector: 'app-top-ten',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="mb-0">Top Ten Daily Ranking</h5>
      </div>
      <div class="card-body">
        @if (loading) {
          <p class="text-muted">Loading top ten ranking...</p>
        } @else if (topTenData && topTenData.length > 0) {
          <div class="table-responsive">
            <table class="table table-striped">
              <thead>
                <tr>
                  <th>Rank</th>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Score</th>
                </tr>
              </thead>
              <tbody>
                @for (item of topTenData; let i = $index; track item.id) {
                  <tr>
                    <td>{{ i + 1 }}</td>
                    <td>{{ item.name }} {{ item.lastName }}</td>
                    <td>{{ item.email }}</td>
                    <td>{{ item.score }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        } @else {
          <p class="text-muted">No data available.</p>
        }
      </div>
    </div>
  `
})
export class TopTenComponent implements OnInit {
  topTenData: UserResponseData[] | null = null;
  loading = false;
  errorMessage = '';

  constructor(private strategyService: StrategyService) {}

  ngOnInit(): void {
    this.loadTopTen();
  }

  loadTopTen(): void {
    this.loading = true;
    this.strategyService.getTopTen().subscribe({
      next: (response) => {
        this.topTenData = response.topTenUsers;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load top ten';
        this.loading = false;
      }
    });
  }
}
