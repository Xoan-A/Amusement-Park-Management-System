import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { MembershipService } from '../../../core/services/membership.service';
import { MembershipLevel } from '../../../core/models';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ],
  templateUrl: './profile.component.html',
  styles: []
})
export class ProfileComponent implements OnInit {
  profileForm: FormGroup;
  loading = false;
  errorMessage = '';
  membershipLevel: MembershipLevel = MembershipLevel.Standard;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private authService: AuthService,
    private membershipService: MembershipService,
    private toastService: ToastService
  ) {
    this.profileForm = this.fb.group({
      name: [''],
      lastName: [''],
      email: [''],
      birthDate: ['']
    });
  }

  ngOnInit(): void {
    const userId = this.authService.getUserId();
    if (!userId) {
      this.errorMessage = 'User not authenticated';
      return;
    }

    this.loading = true;
    this.userService.getById(userId).subscribe({
      next: (user) => {
        this.profileForm.patchValue({
          name: user.name,
          lastName: user.lastName,
          email: user.email,
          birthDate: this.formatDateForInput(user.birthDate)
        });
        this.membershipLevel = user.membershipLevel || MembershipLevel.Standard;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load profile';
        this.loading = false;
      }
    });
  }

  updateProfile(): void {
    this.loading = true;
    this.errorMessage = '';

    const userId = this.authService.getUserId();
    if (!userId) {
      this.errorMessage = 'User not authenticated';
      this.loading = false;
      return;
    }

    this.userService.update(userId, this.profileForm.value).subscribe({
      next: () => {
        this.toastService.showSuccess('Profile updated successfully!');
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to update profile';
        this.loading = false;
      }
    });
  }

  getMembershipLevelName(level: MembershipLevel): string {
    return this.membershipService.getLevelName(level);
  }

  getMembershipBadgeClass(level: MembershipLevel): string {
    return this.membershipService.getBadgeClass(level);
  }

  formatDateForInput(date: string | Date | undefined): string {
    if (!date) return '';

    let dateObj: Date;

    if (typeof date === 'string') {
      dateObj = new Date(date);
    } else {
      dateObj = new Date(date);
    }

    if (isNaN(dateObj.getTime())) {
      return '';
    }

    const year = dateObj.getFullYear();
    const month = String(dateObj.getMonth() + 1).padStart(2, '0');
    const day = String(dateObj.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
