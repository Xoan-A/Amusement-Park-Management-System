import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../../core/services/user.service';

@Component({
  selector: 'app-create-user',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container mt-4">
      <div class="row mb-4">
        <div class="col-12">
          <h1>Create New User</h1>
          <button class="btn btn-secondary" (click)="goBack()">
            ← Back to Users List
          </button>
        </div>
      </div>

      <div class="card">
        <div class="card-header">
          <h5 class="mb-0">User Details (Administrator/Operator)</h5>
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
                @if (userForm.get('name')?.invalid && userForm.get('name')?.touched) {
                  <small class="text-danger">First name is required</small>
                }
              </div>
              <div class="col-md-6 mb-3">
                <label class="form-label">Last Name *</label>
                <input type="text" class="form-control" formControlName="lastName">
                @if (userForm.get('lastName')?.invalid && userForm.get('lastName')?.touched) {
                  <small class="text-danger">Last name is required</small>
                }
              </div>
            </div>
            <div class="row">
              <div class="col-md-6 mb-3">
                <label class="form-label">Email *</label>
                <input type="email" class="form-control" formControlName="email">
                @if (userForm.get('email')?.invalid && userForm.get('email')?.touched) {
                  <small class="text-danger">
                    @if (userForm.get('email')?.errors?.['required']) {
                      Email is required
                    } @else if (userForm.get('email')?.errors?.['email']) {
                      Please enter a valid email
                    }
                  </small>
                }
              </div>
              <div class="col-md-6 mb-3">
                <label class="form-label">Password *</label>
                <input type="password" class="form-control" formControlName="password">
                @if (userForm.get('password')?.invalid && userForm.get('password')?.touched) {
                  <small class="text-danger">
                    @if (userForm.get('password')?.errors?.['required']) {
                      Password is required
                    } @else if (userForm.get('password')?.errors?.['minlength']) {
                      Password must be at least 6 characters
                    }
                  </small>
                }
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label">Role *</label>
              <select class="form-select" formControlName="role">
                <option value="Administrator">Administrator</option>
                <option value="Operator">Operator</option>
                <option value="Visitor">Visitor</option>
              </select>
              @if (userForm.get('role')?.invalid && userForm.get('role')?.touched) {
                <small class="text-danger">Role is required</small>
              }
            </div>
            <div class="d-flex gap-2">
              <button type="submit" class="btn btn-primary" [disabled]="loading || userForm.invalid">
                {{ loading ? 'Creating...' : 'Create User' }}
              </button>
              <button type="button" class="btn btn-outline-secondary" (click)="goBack()">
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class CreateUserComponent {
  userForm: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router
  ) {
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
        setTimeout(() => {
          this.router.navigate(['/admin/users']);
        }, 1500);
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to create user';
        this.loading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/users']);
  }
}
