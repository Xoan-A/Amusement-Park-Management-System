import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../../core/services/user.service';
import { MembershipLevel } from '../../../../core/models/enums';

@Component({
  selector: 'app-create-user',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-user.component.html'
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

    if (this.userForm.value.role === 'Visitor') {
      userData.membershipLevel = MembershipLevel.Standard;
    }

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
