import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { UserService } from '../../../../core/services/user.service';
import { MembershipService } from '../../../../core/services/membership.service';
import { UserResponse, MembershipLevel, Roles } from '../../../../core/models';
import { TopTenComponent } from '../top-ten/top-ten.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, TopTenComponent, FormsModule],
  templateUrl: './users-list.component.html'
})
export class UsersListComponent implements OnInit {
  usersList: UserResponse[] = [];
  loading = false;
  errorMessage = '';

  showAddRoleModal = false;
  showMembershipModal = false;
  selectedUser: UserResponse | null = null;
  addingRole = false;
  changingMembership = false;

  newRole = '';
  newMembershipLevel: number = 0;

  availableRoles = [Roles.ADMINISTRATOR, Roles.OPERATOR, Roles.VISITOR];

  constructor(
    private userService: UserService,
    private membershipService: MembershipService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getAll().subscribe({
      next: (response) => {
        this.usersList = response || [];
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = error.error?.message || 'Failed to load users';
        this.loading = false;
      }
    });
  }

  goToCreateUser(): void {
    this.router.navigate(['/admin/users/create']);
  }

  openAddRoleModal(user: UserResponse): void {
    this.selectedUser = user;
    this.newRole = this.availableRoles.find(r => !user.userRoles.includes(r)) || '';
    this.showAddRoleModal = true;
  }

  openMembershipModal(user: UserResponse): void {
    this.selectedUser = user;
    this.newMembershipLevel = user.membershipLevel ?? 0;
    this.showMembershipModal = true;
  }

  closeModals(): void {
    this.showAddRoleModal = false;
    this.showMembershipModal = false;
    this.selectedUser = null;
    this.newRole = '';
    this.newMembershipLevel = 0;
  }

  addRole(): void {
    if (!this.selectedUser || !this.newRole) return;

    this.addingRole = true;
    this.userService.addRole(this.selectedUser.id, { role: this.newRole }).subscribe({
      next: () => {
        this.loadUsers();
        this.closeModals();
        this.addingRole = false;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = error.error?.message || 'Failed to add role';
        this.addingRole = false;
      }
    });
  }

  changeMembership(): void {
    if (!this.selectedUser) return;

    this.changingMembership = true;
    this.userService.changeMembershipLevel(this.selectedUser.id, { membershipLevel: this.newMembershipLevel }).subscribe({
      next: () => {
        this.loadUsers();
        this.closeModals();
        this.changingMembership = false;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = error.error?.message || 'Failed to change membership';
        this.changingMembership = false;
      }
    });
  }

  getMembershipLevelName(level?: number | null): string {
    if (level === null || level === undefined) {
      return this.membershipService.getLevelName(MembershipLevel.Standard);
    }
    return this.membershipService.getLevelName(level as MembershipLevel);
  }
}
