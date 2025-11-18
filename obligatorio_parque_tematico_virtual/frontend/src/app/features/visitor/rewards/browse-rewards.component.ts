import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { RewardService } from '../../../core/services/reward.service';
import { RedemptionService } from '../../../core/services/redemption.service';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { MembershipService } from '../../../core/services/membership.service';
import { RewardResponse, MembershipLevel } from '../../../core/models';
import { ConfirmationModalComponent } from '../../../shared/components/confirmation-modal/confirmation-modal.component';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-browse-rewards',
  standalone: true,
  imports: [CommonModule, RouterLink, ConfirmationModalComponent],
  templateUrl: './browse-rewards.component.html',
  styles: [`
    .card {
      transition: transform 0.2s;
    }
    .card:hover {
      transform: translateY(-5px);
    }
    .card.border-success {
      border-width: 2px;
    }
  `]
})
export class BrowseRewardsComponent implements OnInit {
  rewards: RewardResponse[] = [];
  userPoints = 0;
  userMembershipLevel: MembershipLevel | null = null;
  userId: string = '';
  loading = false;
  errorMessage = '';
  redeeming: string | null = null;
  showRedeemModal = false;
  redeemMessage = '';
  rewardToRedeem: RewardResponse | null = null;

  constructor(
    private rewardService: RewardService,
    private redemptionService: RedemptionService,
    private userService: UserService,
    private authService: AuthService,
    private membershipService: MembershipService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.userId = this.authService.getCurrentUser()?.id || '';
    this.loadUserData();
    this.loadRewards();
  }

  loadUserData(): void {
    this.userService.getById(this.userId).subscribe({
      next: (user) => {
        this.userPoints = user.score;
        this.userMembershipLevel = user.membershipLevel ?? MembershipLevel.Standard;
      },
      error: (error) => {
        console.error('Error loading user data', error);
      }
    });
  }

  loadRewards(): void {
    this.loading = true;
    this.errorMessage = '';

    this.rewardService.getAvailable().subscribe({
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

  redeemReward(reward: RewardResponse): void {
    this.rewardToRedeem = reward;
    this.redeemMessage = `Redeem "${reward.name}" for ${reward.pointsCost} points?`;
    this.showRedeemModal = true;
  }

  confirmRedeem(): void {
    if (this.rewardToRedeem) {
      this.redeeming = this.rewardToRedeem.id;
      this.errorMessage = '';

      this.redemptionService.redeemReward({ rewardId: this.rewardToRedeem.id }).subscribe({
        next: (redemption) => {
          this.toastService.showSuccess(`Successfully redeemed "${this.rewardToRedeem!.name}"! ${redemption.pointsSpent} points spent.`);
          this.redeeming = null;
          this.userPoints -= redemption.pointsSpent;
          this.loadRewards();
          this.rewardToRedeem = null;
          this.showRedeemModal = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to redeem reward';
          this.redeeming = null;
          this.rewardToRedeem = null;
          this.showRedeemModal = false;
        }
      });
    }
  }

  cancelRedeem(): void {
    this.rewardToRedeem = null;
    this.showRedeemModal = false;
  }

  hasEnoughPoints(reward: RewardResponse): boolean {
    return this.userPoints >= reward.pointsCost;
  }

  meetsMembershipRequirement(reward: RewardResponse): boolean {
    if (reward.requiredMembershipLevel === undefined || reward.requiredMembershipLevel === null) return true;
    const userLevel = this.userMembershipLevel ?? MembershipLevel.Standard;
    return userLevel >= reward.requiredMembershipLevel;
  }

  canRedeem(reward: RewardResponse): boolean {
    return reward.isAvailable &&
           this.hasEnoughPoints(reward) &&
           this.meetsMembershipRequirement(reward);
  }

  getMembershipBadgeClass(level: MembershipLevel): string {
    return this.membershipService.getBadgeClass(level);
  }

  getMembershipLevelName(level: MembershipLevel): string {
    return this.membershipService.getLevelName(level);
  }
}
