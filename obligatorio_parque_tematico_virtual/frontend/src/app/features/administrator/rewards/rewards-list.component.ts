import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { RewardService } from '../../../core/services/reward.service';
import { MembershipService } from '../../../core/services/membership.service';
import { RewardResponse, MembershipLevel } from '../../../core/models';
import { ConfirmationModalComponent } from '../../../shared/components/confirmation-modal/confirmation-modal.component';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-rewards-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ConfirmationModalComponent],
  template: `
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1>Rewards Management</h1>
        <a routerLink="/admin/rewards/create" class="btn btn-primary">
          Create New Reward
        </a>
      </div>

      @if (errorMessage) {
        <div class="alert alert-danger alert-dismissible">
          {{ errorMessage }}
          <button type="button" class="btn-close" (click)="errorMessage=''"></button>
        </div>
      }

      @if (loading) {
        <div class="text-center my-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>
      } @else {
        @if (rewards.length === 0) {
          <div class="alert alert-info">
            No rewards found. Create your first reward to get started!
          </div>
        } @else {
          <div class="table-responsive">
            <table class="table table-hover">
              <thead class="table-light">
                <tr>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Points Cost</th>
                  <th>Available Quantity</th>
                  <th>Membership Required</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (reward of rewards; track reward.id) {
                  <tr>
                    <td><strong>{{ reward.name }}</strong></td>
                    <td>{{ reward.description }}</td>
                    <td>
                      <span class="badge bg-info">{{ reward.pointsCost }} pts</span>
                    </td>
                    <td>{{ reward.availableQuantity }}</td>
                    <td>
                      @if (reward.requiredMembershipLevel !== null && reward.requiredMembershipLevel !== undefined) {
                        <span class="badge" [ngClass]="getMembershipBadgeClass(reward.requiredMembershipLevel)">
                          {{ getMembershipLevelName(reward.requiredMembershipLevel) }}
                        </span>
                      } @else {
                        <span class="text-muted">None</span>
                      }
                    </td>
                    <td>
                      @if (reward.isAvailable) {
                        <span class="badge bg-success">Available</span>
                      } @else {
                        <span class="badge bg-secondary">Out of Stock</span>
                      }
                    </td>
                    <td>
                      <div class="btn-group" role="group">
                        <a [routerLink]="['/admin/rewards/edit', reward.id]"
                           class="btn btn-sm btn-outline-primary">
                          Edit
                        </a>
                        <button (click)="deleteReward(reward)"
                                class="btn btn-sm btn-outline-danger"
                                [disabled]="deleting === reward.id">
                          {{ deleting === reward.id ? 'Deleting...' : 'Delete' }}
                        </button>
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      }
    </div>

    <app-confirmation-modal
      [show]="showDeleteModal"
      title="Delete Reward"
      [message]="deleteMessage"
      (confirmed)="confirmDelete()"
      (cancelled)="cancelDelete()">
    </app-confirmation-modal>
  `,
  styles: [`
    .table-responsive {
      box-shadow: 0 0 10px rgba(0,0,0,0.1);
      border-radius: 8px;
      overflow: hidden;
    }
  `]
})
export class RewardsListComponent implements OnInit {
  rewards: RewardResponse[] = [];
  loading = false;
  errorMessage = '';
  deleting: string | null = null;
  showDeleteModal = false;
  deleteMessage = '';
  rewardToDelete: RewardResponse | null = null;

  constructor(
    private rewardService: RewardService,
    private membershipService: MembershipService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadRewards();
  }

  loadRewards(): void {
    this.loading = true;
    this.errorMessage = '';

    this.rewardService.getAll().subscribe({
      next: (rewards) => {
        this.rewards = rewards;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load rewards';
        this.loading = false;
      }
    });
  }

  deleteReward(reward: RewardResponse): void {
    this.rewardToDelete = reward;
    this.deleteMessage = `Are you sure you want to delete "${reward.name}"?`;
    this.showDeleteModal = true;
  }

  confirmDelete(): void {
    if (this.rewardToDelete) {
      this.deleting = this.rewardToDelete.id;
      this.errorMessage = '';

      this.rewardService.delete(this.rewardToDelete.id).subscribe({
        next: () => {
          this.toastService.showSuccess(`Reward "${this.rewardToDelete!.name}" deleted successfully!`);
          this.deleting = null;
          this.loadRewards();
          this.rewardToDelete = null;
          this.showDeleteModal = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to delete reward';
          this.deleting = null;
          this.rewardToDelete = null;
          this.showDeleteModal = false;
        }
      });
    }
  }

  cancelDelete(): void {
    this.rewardToDelete = null;
    this.showDeleteModal = false;
  }

  getMembershipBadgeClass(level: MembershipLevel): string {
    return this.membershipService.getBadgeClass(level);
  }

  getMembershipLevelName(level: MembershipLevel): string {
    return this.membershipService.getLevelName(level);
  }
}
