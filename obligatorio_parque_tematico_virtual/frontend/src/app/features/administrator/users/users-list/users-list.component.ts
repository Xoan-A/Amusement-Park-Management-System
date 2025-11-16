import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../../../core/services/user.service';
import { Roles } from '../../../../core/models';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ],
  template: `
    <app-navbar></app-navbar>
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
    </div>
  `
})
export class UsersListComponent {
  userForm: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';

  constructor(private fb: FormBuilder, private userService: UserService) {
    this.userForm = this.fb.group({
      name: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['Administrator', Validators.required]
    });
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
}
