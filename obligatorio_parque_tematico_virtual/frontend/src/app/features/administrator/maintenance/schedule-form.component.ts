import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { AttractionService } from '../../../core/services/attraction.service';
import { AttractionResponse } from '../../../core/models/responses';
import { MaintenanceType } from '../../../core/models/enums';

@Component({
  selector: 'app-schedule-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
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

                <!-- Maintenance Type -->
                <div class="mb-3">
                  <label for="maintenanceType" class="form-label">Maintenance Type *</label>
                  <select
                    id="maintenanceType"
                    class="form-select"
                    formControlName="maintenanceType"
                    [class.is-invalid]="scheduleForm.get('maintenanceType')?.invalid && scheduleForm.get('maintenanceType')?.touched">
                    <option value="">Select maintenance type</option>
                    <option value="Inspection">Inspection</option>
                    <option value="Cleaning">Cleaning</option>
                    <option value="Repair">Repair</option>
                    <option value="SafetyCheck">Safety Check</option>
                  </select>
                  @if (scheduleForm.get('maintenanceType')?.invalid && scheduleForm.get('maintenanceType')?.touched) {
                    <div class="invalid-feedback">
                      Maintenance type is required.
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
      maintenanceType: ['', Validators.required],
      description: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  loadAttractions() {
    this.attractionService.getAllAttractions().subscribe({
      next: (response) => {
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
      maintenanceType: formValue.maintenanceType,
      description: formValue.description
    };

    this.maintenanceService.createSchedule(request).subscribe({
      next: (response) => {
        this.loading = false;
        alert(response.message || 'Maintenance scheduled successfully!');
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
