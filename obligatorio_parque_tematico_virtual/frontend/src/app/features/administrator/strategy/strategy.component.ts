import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { StrategyService } from '../../../core/services/strategy.service';
import { TopVisitor } from '../../../core/models';

@Component({
  selector: 'app-strategy',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    <div class="container mt-4">
      <h1 class="mb-4">Strategy Management</h1>

      <div class="row">
        <div class="col-md-6 mb-4">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Current Strategy</h5>
            </div>
            <div class="card-body">
              <p><strong>Active Strategy:</strong> {{ currentStrategy || 'Loading...' }}</p>
              <form [formGroup]="strategyForm" (ngSubmit)="updateStrategy()">
                <div class="mb-3">
                  <label class="form-label">Set New Strategy</label>
                  <input type="text" class="form-control" formControlName="strategyName" placeholder="Enter strategy name">
                </div>
                <button type="submit" class="btn btn-primary" [disabled]="loading">
                  {{ loading ? 'Updating...' : 'Update Strategy' }}
                </button>
              </form>
              @if (successMessage) {
                <div class="alert alert-success mt-3">{{ successMessage }}</div>
              }
            </div>
          </div>
        </div>

        <div class="col-md-6 mb-4">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Top 10 Visitors</h5>
            </div>
            <div class="card-body">
              @if (loadingTopTen) {
                <div class="text-center">
                  <div class="spinner-border text-primary"></div>
                </div>
              } @else if (topVisitors.length > 0) {
                <table class="table table-sm">
                  <thead>
                    <tr>
                      <th>#</th>
                      <th>Name</th>
                      <th>Score</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (visitor of topVisitors; track visitor.visitorId; let idx = $index) {
                      <tr>
                        <td>{{ idx + 1 }}</td>
                        <td>{{ visitor.name }} {{ visitor.lastName }}</td>
                        <td>{{ visitor.score }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              } @else {
                <p class="text-muted">No data available</p>
              }
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class StrategyComponent implements OnInit {
  strategyForm: FormGroup;
  currentStrategy = '';
  topVisitors: TopVisitor[] = [];
  loading = false;
  loadingTopTen = false;
  successMessage = '';

  constructor(private fb: FormBuilder, private strategyService: StrategyService) {
    this.strategyForm = this.fb.group({
      strategyName: ['']
    });
  }

  ngOnInit(): void {
    this.loadCurrentStrategy();
    this.loadTopTen();
  }

  loadCurrentStrategy(): void {
    this.strategyService.getCurrent().subscribe({
      next: (strategy) => {
        this.currentStrategy = strategy.name;
      },
      error: (error) => console.error('Error loading strategy', error)
    });
  }

  loadTopTen(): void {
    this.loadingTopTen = true;
    this.strategyService.getTopTen().subscribe({
      next: (response) => {
        this.topVisitors = response.visitors || [];
        this.loadingTopTen = false;
      },
      error: (error) => {
        console.error('Error loading top visitors', error);
        this.loadingTopTen = false;
      }
    });
  }

  updateStrategy(): void {
    const strategyName = this.strategyForm.value.strategyName;
    if (!strategyName) return;

    this.loading = true;
    this.successMessage = '';

    this.strategyService.setStrategy({ strategyName }).subscribe({
      next: () => {
        this.successMessage = 'Strategy updated successfully!';
        this.currentStrategy = strategyName;
        this.strategyForm.reset();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error updating strategy', error);
        this.loading = false;
      }
    });
  }
}
