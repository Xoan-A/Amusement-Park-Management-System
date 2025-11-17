import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RewardService } from '../../../core/services/reward.service';
import { MembershipLevel } from '../../../core/models';
import { CreateRewardResponse, RewardResponse } from '../../../core/models/responses';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-reward-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ],
  template: `
    <div class="container mt-4">
      <div class="row justify-content-center">
        <div class="col-md-8">
          <div class="card">
            <div class="card-header">
              <h3 class="mb-0">{{ isEditMode ? 'Edit Reward' : 'Create New Reward' }}</h3>
            </div>
            <div class="card-body">
              @if (errorMessage) {
                <div class="alert alert-danger alert-dismissible">
                  {{ errorMessage }}
                  <button type="button" class="btn-close" (click)="errorMessage=''"></button>
                </div>
              }

              @if (loading && isEditMode) {
                <div class="text-center my-4">
                  <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                  </div>
                </div>
              } @else {
                <form [formGroup]="rewardForm" (ngSubmit)="onSubmit()">
                  <div class="mb-3">
                    <label class="form-label">Reward Name *</label>
                    <input
                      type="text"
                      class="form-control"
                      formControlName="name"
                      placeholder="e.g., VIP Access Pass"
                      [class.is-invalid]="isFieldInvalid('name')"
                    />
                    @if (isFieldInvalid('name')) {
                      <div class="invalid-feedback">Name is required (max 100 characters)</div>
                    }
                  </div>

                  <div class="mb-3">
                    <label class="form-label">Description *</label>
                    <textarea
                      class="form-control"
                      formControlName="description"
                      rows="3"
                      placeholder="Describe the reward and what it includes..."
                      [class.is-invalid]="isFieldInvalid('description')"
                    ></textarea>
                    @if (isFieldInvalid('description')) {
                      <div class="invalid-feedback">Description is required</div>
                    }
                  </div>

                  <div class="row">
                    <div class="col-md-6 mb-3">
                      <label class="form-label">Points Cost *</label>
                      <input
                        type="number"
                        class="form-control"
                        formControlName="pointsCost"
                        min="0"
                        placeholder="e.g., 500"
                        [class.is-invalid]="isFieldInvalid('pointsCost')"
                      />
                      @if (isFieldInvalid('pointsCost')) {
                        <div class="invalid-feedback">Points cost must be greater than 0</div>
                      }
                    </div>

                    <div class="col-md-6 mb-3">
                      <label class="form-label">Available Quantity *</label>
                      <input
                        type="number"
                        class="form-control"
                        formControlName="availableQuantity"
                        min="0"
                        placeholder="e.g., 10"
                        [class.is-invalid]="isFieldInvalid('availableQuantity')"
                      />
                      @if (isFieldInvalid('availableQuantity')) {
                        <div class="invalid-feedback">Quantity must be 0 or greater</div>
                      }
                    </div>
                  </div>

                  <div class="mb-3">
                    <label class="form-label">Required Membership Level *</label>
                    <select class="form-select" formControlName="requiredMembershipLevel" [class.is-invalid]="isFieldInvalid('requiredMembershipLevel')">
                      <option [ngValue]="membershipLevels.Standard">Standard</option>
                      <option [ngValue]="membershipLevels.Premium">Premium</option>
                      <option [ngValue]="membershipLevels.VIP">VIP</option>
                    </select>
                    <div class="form-text">Select the minimum membership level required to redeem this reward</div>
                    @if (isFieldInvalid('requiredMembershipLevel')) {
                      <div class="invalid-feedback">Membership level is required</div>
                    }
                  </div>

                  <div class="d-flex justify-content-between mt-4">
                    <button
                      type="button"
                      class="btn btn-secondary"
                      (click)="goBack()"
                      [disabled]="saving"
                    >
                      Cancel
                    </button>
                    <button
                      type="submit"
                      class="btn btn-primary"
                      [disabled]="rewardForm.invalid || saving"
                    >
                      {{ saving ? 'Saving...' : (isEditMode ? 'Update Reward' : 'Create Reward') }}
                    </button>
                  </div>
                </form>
              }
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class RewardFormComponent implements OnInit {
  rewardForm: FormGroup;
  loading = false;
  saving = false;
  errorMessage = '';
  isEditMode = false;
  rewardId: string | null = null;
  membershipLevels = MembershipLevel;

  constructor(
    private fb: FormBuilder,
    private rewardService: RewardService,
    private router: Router,
    private route: ActivatedRoute,
    private toastService: ToastService
  ) {
    this.rewardForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.required],
      pointsCost: [0, [Validators.required, Validators.min(1)]],
      availableQuantity: [0, [Validators.required, Validators.min(0)]],
      requiredMembershipLevel: [this.membershipLevels.Standard, Validators.required]
    });
  }

  ngOnInit(): void {
    this.rewardId = this.route.snapshot.paramMap.get('id');
    if (this.rewardId) {
      this.isEditMode = true;
      this.loadReward();
    }
  }

  loadReward(): void {
    if (!this.rewardId) return;

    this.loading = true;
    this.errorMessage = '';

    this.rewardService.getById(this.rewardId).subscribe({
      next: (reward) => {
        this.rewardForm.patchValue({
          name: reward.name,
          description: reward.description,
          pointsCost: reward.pointsCost,
          availableQuantity: reward.availableQuantity,
          requiredMembershipLevel: reward.requiredMembershipLevel ?? this.membershipLevels.Standard
        });
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load reward';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.rewardForm.invalid) return;

    this.saving = true;
    this.errorMessage = '';

    const rewardData = this.rewardForm.value;

    const operation: Observable<CreateRewardResponse | RewardResponse> = this.isEditMode && this.rewardId
      ? this.rewardService.update(this.rewardId, rewardData)
      : this.rewardService.create(rewardData);

    operation.subscribe({
      next: () => {
        const message = this.isEditMode ? 'Reward updated successfully!' : 'Reward created successfully!';
        this.toastService.showSuccess(message);
        setTimeout(() => this.router.navigate(['/admin/rewards']), 1500);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = error.error?.message || `Failed to ${this.isEditMode ? 'update' : 'create'} reward`;
        this.saving = false;
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.rewardForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  goBack(): void {
    this.router.navigate(['/admin/rewards']);
  }
}
