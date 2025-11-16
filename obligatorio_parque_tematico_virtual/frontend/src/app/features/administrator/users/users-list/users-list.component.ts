import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../../../core/services/user.service';
import { StrategyService } from '../../../../core/services/strategy.service';
import { Roles, UserResponseData } from '../../../../core/models';

  @Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ],
  template: `
    <div class="container mt-4">
      <h1 class="mb-4">User Management</h1>

      <div class="card mb-4">
        <div class="card-header">
          <h5 class="mb-0">Create New User (Administrator/Operator)</h5>
        </div>
        <div class="card-body">
          @if (errorMessage) {
            <div class="alert alert-danger alert-dismissible">
              {{ errorMessage }}
              <button type="button" class="btn-close" (click)="errorMessage=''"></button>
            </div>
          }
          @if (successMessage) {
            <div class="alert alert-success">{{ successMessage }}</div>
          }

          <form [formGroup]="userForm" (ngSubmit)="createUser()">
            <div class="row">
              <div class="col-md-6 mb-3">
                <label class="form-label">First Name *</label>
                <input type="text" class="form-control" formControlName="name">
              </div>
              <div class="col-md-6 mb-3">
                <label class="form-label">Last Name *</label>
                <input type="text" class="form-control" formControlName="lastName">
              </div>
            </div>
            <div class="row">
              <div class="col-md-6 mb-3">
                <label class="form-label">Email *</label>
                <input type="email" class="form-control" formControlName="email">
              </div>
              <div class="col-md-6 mb-3">
                <label class="form-label">Password *</label>
                <input type="password" class="form-control" formControlName="password">
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label">Role *</label>
              <select class="form-select" formControlName="role">
                <option value="Administrator">Administrator</option>
                <option value="Operator">Operator</option>
              </select>
            </div>
            <button type="submit" class="btn btn-primary" [disabled]="loading">
              {{ loading ? 'Creating...' : 'Create User' }}
            </button>
          </form>
        </div>
      </div>

      <div class="card mb-4">
        <div class="card-header">
          <h5 class="mb-0">Top Ten Daily Ranking</h5>
        </div>
        <div class="card-body">
          @if (loadingTopTen) {
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
    </div>
  `
})
export class UsersListComponent implements OnInit {
  userForm: FormGroup;
  loading = false;
  loadingTopTen = false;
  errorMessage = '';
  successMessage = '';
  topTenData: UserResponseData[] | null = null;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private strategyService: StrategyService
  ) {
    this.userForm = this.fb.group({
      name: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['Administrator', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadTopTen();
  }

  createUser(): void {
    if (this.userForm.invalid) return;

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const userData = {
      ...this.userForm.value,
      roles: [this.userForm.value.role]
    };

    this.userService.create(userData).subscribe({
      next: () => {
        this.successMessage = 'User created successfully!';
        this.userForm.reset({ role: 'Administrator' });
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to create user';
        this.loading = false;
      }
    });
  }

  loadTopTen(): void {
    this.loadingTopTen = true;
    this.strategyService.getTopTen().subscribe({
      next: (response) => {
        this.topTenData = response.topTenUsers;
        this.loadingTopTen = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load top ten';
        this.loadingTopTen = false;
      }
    });
  }
}
