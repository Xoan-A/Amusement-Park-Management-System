import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { RewardService } from '../../../core/services/reward.service';
import { RedemptionService } from '../../../core/services/redemption.service';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { RewardResponse, MembershipLevel } from '../../../core/models';

@Component({
  selector: 'app-browse-rewards',
  standalone: true,
  imports: [CommonModule, RouterLink, ],
  template: `
    <app-navbar></app-navbar>
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1>Available Rewards</h1>
        <div>
          <span class="badge bg-primary fs-5 me-2">Your Points: {{ userPoints }}</span>
          <a routerLink="/visitor/my-redemptions" class="btn btn-outline-primary">
            My Redemption History
          </a>
        </div>
      </div>

      @if (errorMessage) {
        <div class="alert alert-danger alert-dismissible">
          {{ errorMessage }}
          <button type="button" class="btn-close" (click)="errorMessage=''"></button>
        </div>
      }

      @if (successMessage) {
        <div class="alert alert-success alert-dismissible">
          {{ successMessage }}
          <button type="button" class="btn-close" (click)="successMessage=''"></button>
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
            No rewards available at the moment. Check back later!
          </div>
        } @else {
          <div class="row">
            @for (reward of rewards; track reward.id) {
              <div class="col-md-6 col-lg-4 mb-4">
                <div class="card h-100 shadow-sm" [class.border-success]="canRedeem(reward)">
                  <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                      <h5 class="card-title">{{ reward.name }}</h5>
                      @if (reward.isAvailable) {
                        <span class="badge bg-success">Available</span>
                      } @else {
                        <span class="badge bg-secondary">Out of Stock</span>
                      }
                    </div>

                    <p class="card-text text-muted">{{ reward.description }}</p>

                    <div class="mb-3">
                      <div class="d-flex justify-content-between mb-2">
                        <span><strong>Cost:</strong></span>
                        <span class="badge bg-info">{{ reward.pointsCost }} points</span>
                      </div>

                      <div class="d-flex justify-content-between mb-2">
                        <span><strong>In Stock:</strong></span>
                        <span>{{ reward.availableQuantity }}</span>
                      </div>

                      @if (reward.requiredMembershipLevel) {
                        <div class="d-flex justify-content-between">
                          <span><strong>Requires:</strong></span>
                          <span class="badge" [ngClass]="getMembershipBadgeClass(reward.requiredMembershipLevel)">
                            {{ reward.requiredMembershipLevel }}
                          </span>
                        </div>
                      }
                    </div>

                    <div class="d-grid">
                      @if (!reward.isAvailable) {
                        <button class="btn btn-secondary" disabled>
                          Out of Stock
                        </button>
                      } @else if (!hasEnoughPoints(reward)) {
                        <button class="btn btn-outline-danger" disabled>
                          Insufficient Points
                        </button>
                      } @else if (!meetsMembershipRequirement(reward)) {
                        <button class="btn btn-outline-warning" disabled>
                          Membership Required
                        </button>
                      } @else {
                        <button
                          class="btn btn-success"
                          (click)="redeemReward(reward)"
                          [disabled]="redeeming === reward.id"
                        >
                          {{ redeeming === reward.id ? 'Redeeming...' : 'Redeem Now' }}
                        </button>
                      }
                    </div>
                  </div>
                </div>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
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
  successMessage = '';
  redeeming: string | null = null;

  constructor(
    private rewardService: RewardService,
    private redemptionService: RedemptionService,
    private userService: UserService,
    private authService: AuthService
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
        this.userMembershipLevel = user.membershipLevel || null;
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
    if (!confirm(`Redeem "${reward.name}" for ${reward.pointsCost} points?`)) {
      return;
    }

    this.redeeming = reward.id;
    this.errorMessage = '';
    this.successMessage = '';

    this.redemptionService.redeemReward({ rewardId: reward.id }).subscribe({
      next: (redemption) => {
        this.successMessage = `Successfully redeemed "${reward.name}"! ${redemption.pointsSpent} points spent.`;
        this.redeeming = null;
        this.userPoints -= redemption.pointsSpent;
        this.loadRewards();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to redeem reward';
        this.redeeming = null;
      }
    });
  }

  hasEnoughPoints(reward: RewardResponse): boolean {
    return this.userPoints >= reward.pointsCost;
  }

  meetsMembershipRequirement(reward: RewardResponse): boolean {
    if (reward.requiredMembershipLevel === undefined || reward.requiredMembershipLevel === null) return true;
    if (this.userMembershipLevel === null) return false;
    return this.userMembershipLevel >= reward.requiredMembershipLevel;
  }

  canRedeem(reward: RewardResponse): boolean {
    return reward.isAvailable &&
           this.hasEnoughPoints(reward) &&
           this.meetsMembershipRequirement(reward);
  }

  getMembershipBadgeClass(level: MembershipLevel): string {
    switch (level) {
      case MembershipLevel.VIP:
        return 'bg-warning text-dark';
      case MembershipLevel.Premium:
        return 'bg-primary';
      case MembershipLevel.Standard:
        return 'bg-secondary';
      default:
        return 'bg-secondary';
    }
  }
}
