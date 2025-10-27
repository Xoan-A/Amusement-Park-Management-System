import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { MembershipLevel } from '../../../core/models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    <div class="container mt-4">
      <h1 class="mb-4">My Profile</h1>

      <div class="row">
        <div class="col-lg-8">
          <div class="card">
            <div class="card-body">
              <h5 class="card-title mb-4">Edit Profile</h5>

              @if (successMessage) {
                <div class="alert alert-success">{{ successMessage }}</div>
              }
              @if (errorMessage) {
                <div class="alert alert-danger">{{ errorMessage }}</div>
              }

              <form [formGroup]="profileForm" (ngSubmit)="updateProfile()">
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">First Name</label>
                    <input type="text" class="form-control" formControlName="name">
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Last Name</label>
                    <input type="text" class="form-control" formControlName="lastName">
                  </div>
                </div>

                <div class="mb-3">
                  <label class="form-label">Email</label>
                  <input type="email" class="form-control" formControlName="email">
                </div>

                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Birth Date</label>
                    <input type="date" class="form-control" formControlName="birthDate">
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Membership Level</label>
                    <select class="form-select" formControlName="membershipLevel">
                      <option value="Standard">Standard</option>
                      <option value="Premium">Premium</option>
                      <option value="VIP">VIP</option>
                    </select>
                  </div>
                </div>

                <button type="submit" class="btn btn-primary" [disabled]="loading">
                  {{ loading ? 'Updating...' : 'Update Profile' }}
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class ProfileComponent implements OnInit {
  profileForm: FormGroup;
  loading = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private authService: AuthService
  ) {
    this.profileForm = this.fb.group({
      name: [''],
      lastName: [''],
      email: [''],
      birthDate: [''],
      membershipLevel: [MembershipLevel.Standard]
    });
  }

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.profileForm.patchValue({
        name: user.name,
        email: user.email
      });
    }
  }

  updateProfile(): void {
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const userId = this.authService.getUserId();
    if (!userId) {
      this.errorMessage = 'User not authenticated';
      this.loading = false;
      return;
    }

    this.userService.update(userId, this.profileForm.value).subscribe({
      next: () => {
        this.successMessage = 'Profile updated successfully!';
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to update profile';
        this.loading = false;
      }
    });
  }
}
