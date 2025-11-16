import { Component, OnInit } from '@angular/core';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { RewardService } from '../../../core/services/reward.service';
import { RewardResponse, MembershipLevel } from '../../../core/models';

@Component({
  selector: 'app-rewards-list',
  standalone: true,
  imports: [ConfirmationModalComponent, CommonModule, RouterLink, ],
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
      @if (successMessage) {
        <div class="alert alert-success alert-dismissible">
          {{ successMessage }}
          <button type="button" class="btn-close" (click)="successMessage=''"></button>
      @if (loading) {
        <div class="text-center my-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
      } @else {
        @if (rewards.length === 0) {
          <div class="alert alert-info">
            No rewards found. Create your first reward to get started!
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
                      @if (reward.requiredMembershipLevel) {
                        <span class="badge" [ngClass]="getMembershipBadgeClass(reward.requiredMembershipLevel)">
                          {{ reward.requiredMembershipLevel }}
                        </span>
                      } @else {
                        <span class="text-muted">None</span>
                      }
                      @if (reward.isAvailable) {
                        <span class="badge bg-success">Available</span>
                        <span class="badge bg-secondary">Out of Stock</span>
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
                  </tr>
                }
              </tbody>
            </table>
        }
    </div>
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
  showDeleteModal = false;
  itemToDelete: any = null;
  errorMessage = '';
  successMessage = '';
  deleting: string | null = null;
  constructor(private rewardService: RewardService) {}
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
    });
  deleteReward(reward: RewardResponse): void {
    if (!confirm(`Are you sure you want to delete "${reward.name}"?`)) {
      return;
    this.deleting = reward.id;
    this.successMessage = '';
    this.rewardService.delete(reward.id).subscribe({
      next: () => {
        this.successMessage = `Reward "${reward.name}" deleted successfully!`;
        this.deleting = null;
        this.loadRewards();
        this.errorMessage = error.error?.message || 'Failed to delete reward';
  getMembershipBadgeClass(level: MembershipLevel): string {
    switch (level) {
      case MembershipLevel.VIP:
        return 'bg-warning text-dark';
      case MembershipLevel.Premium:
        return 'bg-primary';
      case MembershipLevel.Standard:
        return 'bg-secondary';
      default:
}
