import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StrategyService } from '../../../../core/services/strategy.service';
import { UserResponseData } from '../../../../core/models';

@Component({
  selector: 'app-top-ten',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './top-ten.component.html'
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
