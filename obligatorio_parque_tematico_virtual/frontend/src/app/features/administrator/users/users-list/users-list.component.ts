import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../../../core/services/user.service';
import { MembershipService } from '../../../../core/services/membership.service';
import { UserResponse, MembershipLevel, Roles } from '../../../../core/models';
import { TopTenComponent } from '../top-ten/top-ten.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, TopTenComponent, FormsModule],
  template: `
    <div class="container mt-4">
      <div class="row mb-4 align-items-center">
        <div class="col">
          <h1>Users Management</h1>
        </div>
        <div class="col-auto">
          <button class="btn btn-primary" (click)="goToCreateUser()">
            + Add New User
          </button>
        </div>
      </div>

      <div class="card mb-4">
        <div class="card-header">
          <h5 class="mb-0">Users List</h5>
        </div>
        <div class="card-body">
          @if (loading) {
            <p class="text-muted">Loading users...</p>
          } @else if (usersList && usersList.length > 0) {
            <div class="table-responsive">
              <table class="table table-striped">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Roles</th>
                    <th>Membership Level</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (user of usersList; track user.id) {
                    <tr>
                      <td>{{ user.name }} {{ user.lastName }}</td>
                      <td>{{ user.email }}</td>
                      <td>
                        @if (user.userRoles && user.userRoles.length > 0) {
                          <span class="badge bg-secondary me-1" *ngFor="let role of user.userRoles">
                            {{ role }}
                          </span>
                        } @else {
                          <span class="text-muted">No roles</span>
                        }
                      </td>
                      <td>
                        @if (user.userRoles?.includes('Visitor')) {
                          {{ getMembershipLevelName(user.membershipLevel) }}
                        } @else {
                          <span class="text-muted">-</span>
                        }
                      </td>
                      <td>
                        <button
                          class="btn btn-sm btn-warning me-2"
                          (click)="openAddRoleModal(user)"
                          title="Add Role">
                          Add Role
                        </button>
                        @if (user.userRoles?.includes('Visitor')) {
                          <button
                            class="btn btn-sm btn-info"
                            (click)="openMembershipModal(user)"
                            title="Change Membership">
                            Change Membership
                          </button>
                        }
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          } @else {
            <p class="text-muted">No users available.</p>
          }
        </div>
      </div>

      <!-- Modal Add Role -->
      @if (showAddRoleModal && selectedUser) {
        <div class="modal d-block" style="background-color: rgba(0,0,0,0.5);">
          <div class="modal-dialog">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Add Role to {{ selectedUser.name }}</h5>
                <button type="button" class="btn-close" (click)="closeModals()"></button>
              </div>
              <div class="modal-body">
                <div class="mb-3">
                  <label class="form-label">Select Role</label>
                  <select class="form-select" [(ngModel)]="newRole">
                    @for (role of availableRoles; track role) {
                      @if (!selectedUser.userRoles.includes(role)) {
                        <option [value]="role">{{ role }}</option>
                      }
                    }
                  </select>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" (click)="closeModals()">Cancel</button>
                <button type="button" class="btn btn-primary" (click)="addRole()" [disabled]="addingRole">
                  {{ addingRole ? 'Adding...' : 'Add Role' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      }

      <!-- Modal Change Membership -->
      @if (showMembershipModal && selectedUser) {
        <div class="modal d-block" style="background-color: rgba(0,0,0,0.5);">
          <div class="modal-dialog">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Change Membership - {{ selectedUser.name }}</h5>
                <button type="button" class="btn-close" (click)="closeModals()"></button>
              </div>
              <div class="modal-body">
                <div class="mb-3">
                  <label class="form-label">Membership Level</label>
                  <select class="form-select" [(ngModel)]="newMembershipLevel">
                    <option [value]="0">Standard</option>
                    <option [value]="1">Premium</option>
                    <option [value]="2">VIP</option>
                  </select>
                </div>
                <small class="text-muted">
                  Current: {{ getMembershipLevelName(selectedUser.membershipLevel) }}
                </small>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" (click)="closeModals()">Cancel</button>
                <button type="button" class="btn btn-primary" (click)="changeMembership()" [disabled]="changingMembership">
                  {{ changingMembership ? 'Updating...' : 'Update' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      }

      <app-top-ten></app-top-ten>
    </div>
  `
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
      error: (error: any) => {
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
      },
      error: (error: any) => {
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
      },
      error: (error: any) => {
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
