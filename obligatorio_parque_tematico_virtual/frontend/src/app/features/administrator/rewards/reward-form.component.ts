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
  templateUrl: './reward-form.component.html'
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
