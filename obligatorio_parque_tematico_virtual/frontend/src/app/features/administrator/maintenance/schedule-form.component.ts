import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { AttractionService } from '../../../core/services/attraction.service';
import { AttractionResponse, AllAttractionsResponse } from '../../../core/models/responses';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-schedule-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ],
  template: `
    <div class="container mt-4">
      <div class="row justify-content-center">
        <div class="col-md-8">
          <div class="card">
            <div class="card-header">
              <h3>Schedule Preventive Maintenance</h3>
            </div>
            <div class="card-body">
              @if (errorMessage) {
                <div class="alert alert-danger alert-dismissible fade show" role="alert">
                  {{ errorMessage }}
                  <button type="button" class="btn-close" (click)="errorMessage = null"></button>
                </div>
              }

              <form [formGroup]="scheduleForm" (ngSubmit)="onSubmit()">
                <!-- Attraction Selection -->
                <div class="mb-3">
                  <label for="attractionId" class="form-label">Attraction *</label>
                  <select
                    id="attractionId"
                    class="form-select"
                    formControlName="attractionId"
                    [class.is-invalid]="scheduleForm.get('attractionId')?.invalid && scheduleForm.get('attractionId')?.touched">
                    <option value="">Select an attraction</option>
                    @for (attraction of attractions; track attraction.id) {
                      <option [value]="attraction.id">{{ attraction.name }}</option>
                    }
                  </select>
                  @if (scheduleForm.get('attractionId')?.invalid && scheduleForm.get('attractionId')?.touched) {
                    <div class="invalid-feedback">
                      Attraction is required.
                    </div>
                  }
                </div>

                <!-- Scheduled Date -->
                <div class="mb-3">
                  <label for="scheduledDate" class="form-label">Scheduled Date & Time *</label>
                  <input
                    type="datetime-local"
                    id="scheduledDate"
                    class="form-control"
                    formControlName="scheduledDate"
                    [class.is-invalid]="scheduleForm.get('scheduledDate')?.invalid && scheduleForm.get('scheduledDate')?.touched">
                  @if (scheduleForm.get('scheduledDate')?.invalid && scheduleForm.get('scheduledDate')?.touched) {
                    <div class="invalid-feedback">
                      Scheduled date is required.
                    </div>
                  }
                </div>

                <!-- Estimated Duration -->
                <div class="mb-3">
                  <label for="estimatedDuration" class="form-label">Estimated Duration (hours) *</label>
                  <input
                    type="number"
                    id="estimatedDuration"
                    class="form-control"
                    formControlName="estimatedDuration"
                    min="1"
                    max="24"
                    placeholder="Enter duration in hours"
                    [class.is-invalid]="scheduleForm.get('estimatedDuration')?.invalid && scheduleForm.get('estimatedDuration')?.touched">
                  @if (scheduleForm.get('estimatedDuration')?.invalid && scheduleForm.get('estimatedDuration')?.touched) {
                    <div class="invalid-feedback">
                      @if (scheduleForm.get('estimatedDuration')?.errors?.['required']) {
                        Estimated duration is required.
                      }
                      @if (scheduleForm.get('estimatedDuration')?.errors?.['min']) {
                        Duration must be at least 1 hour.
                      }
                      @if (scheduleForm.get('estimatedDuration')?.errors?.['max']) {
                        Duration cannot exceed 24 hours.
                      }
                    </div>
                  }
                </div>

                <!-- Description -->
                <div class="mb-3">
                  <label for="description" class="form-label">Description *</label>
                  <textarea
                    id="description"
                    class="form-control"
                    rows="4"
                    formControlName="description"
                    placeholder="Describe the maintenance work to be performed..."
                    [class.is-invalid]="scheduleForm.get('description')?.invalid && scheduleForm.get('description')?.touched"></textarea>
                  @if (scheduleForm.get('description')?.invalid && scheduleForm.get('description')?.touched) {
                    <div class="invalid-feedback">
                      @if (scheduleForm.get('description')?.errors?.['required']) {
                        Description is required.
                      }
                      @if (scheduleForm.get('description')?.errors?.['minlength']) {
                        Description must be at least 10 characters long.
                      }
                    </div>
                  }
                </div>

                <!-- Buttons -->
                <div class="d-flex justify-content-between">
                  <button type="button" class="btn btn-secondary" (click)="cancel()" [disabled]="loading">
                    <i class="bi bi-x-circle"></i> Cancel
                  </button>
                  <button type="submit" class="btn btn-primary" [disabled]="scheduleForm.invalid || loading">
                    @if (loading) {
                      <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                      Scheduling...
                    } @else {
                      <i class="bi bi-calendar-check"></i> Schedule Maintenance
                    }
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ScheduleFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private maintenanceService = inject(MaintenanceService);
  private attractionService = inject(AttractionService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  scheduleForm!: FormGroup;
  attractions: AttractionResponse[] = [];
  loading = false;
  errorMessage: string | null = null;

  ngOnInit() {
    this.initForm();
    this.loadAttractions();
  }

  initForm() {
    this.scheduleForm = this.fb.group({
      attractionId: ['', Validators.required],
      scheduledDate: ['', Validators.required],
      estimatedDuration: ['', [Validators.required, Validators.min(1), Validators.max(24)]],
      description: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  loadAttractions() {
    this.attractionService.getAll().subscribe({
      next: (response: AllAttractionsResponse) => {
        this.attractions = response.attractions;
      },
      error: () => {
        this.errorMessage = 'Failed to load attractions.';
      }
    });
  }

  onSubmit() {
    if (this.scheduleForm.invalid) {
      this.scheduleForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    const formValue = this.scheduleForm.value;
    const request = {
      attractionId: formValue.attractionId,
      scheduledDate: formValue.scheduledDate,
      estimatedDuration: parseInt(formValue.estimatedDuration, 10),
      description: formValue.description
    };

    this.maintenanceService.createSchedule(request).subscribe({
      next: (response) => {
        this.loading = false;
        this.toastService.showSuccess(response.message || 'Maintenance scheduled successfully!');
        this.router.navigate(['/admin/maintenance/schedules']);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Failed to schedule maintenance. Please try again.';
      }
    });
  }

  cancel() {
    this.router.navigate(['/admin/maintenance/schedules']);
  }
}
