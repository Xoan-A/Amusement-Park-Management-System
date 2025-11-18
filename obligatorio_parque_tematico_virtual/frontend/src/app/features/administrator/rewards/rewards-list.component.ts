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
  templateUrl: './rewards-list.component.html',
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
