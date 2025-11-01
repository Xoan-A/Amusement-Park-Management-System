import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { AttractionService } from '../../../core/services/attraction.service';
import { AuthService } from '../../../core/services/auth.service';
import { MaintenanceScheduleResponse, AttractionResponse } from '../../../core/models/responses';

@Component({
  selector: 'app-operator-maintenance',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="container mt-4">
      <h2 class="mb-4">Record Maintenance Work</h2>

      <!-- Pending Schedules -->
      <div class="card mb-4">
        <div class="card-header">
          <h5 class="mb-0">
            <i class="bi bi-calendar-check"></i> Pending Maintenance Schedules
            <span class="badge bg-warning ms-2">{{ pendingSchedules.length }}</span>
          </h5>
        </div>
        <div class="card-body">
          @if (loadingSchedules) {
            <div class="text-center">
              <div class="spinner-border spinner-border-sm" role="status"></div>
              <span class="ms-2">Loading...</span>
            </div>
          }

          @if (!loadingSchedules && pendingSchedules.length > 0) {
            <div class="list-group">
              @for (schedule of pendingSchedules; track schedule.id) {
                <a href="javascript:void(0)"
                   class="list-group-item list-group-item-action"
                   (click)="selectSchedule(schedule)">
                  <div class="d-flex w-100 justify-content-between">
                    <h6 class="mb-1">{{ schedule.attractionName }}</h6>
                    <small>{{ schedule.scheduledDate | date:'short' }}</small>
                  </div>
                  <p class="mb-1">{{ schedule.description }}</p>
                  <small class="text-muted">Type: {{ schedule.maintenanceType }}</small>
                </a>
              }
            </div>
          }

          @if (!loadingSchedules && pendingSchedules.length === 0) {
            <p class="text-muted mb-0">No pending maintenance schedules.</p>
          }
        </div>
      </div>

      <!-- Record Maintenance Form -->
      <div class="card">
        <div class="card-header">
          <h5 class="mb-0">
            @if (selectedSchedule) {
              Complete Scheduled Maintenance
            } @else {
              Record Unscheduled Maintenance
            }
          </h5>
        </div>
        <div class="card-body">
          @if (selectedSchedule) {
            <div class="alert alert-info mb-3">
              <strong>Scheduled Maintenance:</strong> {{ selected Schedule.attractionName }} - {{ selectedSchedule.description }}
              <button type="button" class="btn-close float-end" (click)="clearSelection()"></button>
            </div>
          }

          @if (errorMessage) {
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
              {{ errorMessage }}
              <button type="button" class="btn-close" (click)="errorMessage = null"></button>
            </div>
          }

          @if (successMessage) {
            <div class="alert alert-success alert-dismissible fade show" role="alert">
              {{ successMessage }}
              <button type="button" class="btn-close" (click)="successMessage = null"></button>
            </div>
          }

          <form [formGroup]="maintenanceForm" (ngSubmit)="onSubmit()">
            <!-- Attraction Selection (only for unscheduled) -->
            @if (!selectedSchedule) {
              <div class="mb-3">
                <label for="attractionId" class="form-label">Attraction *</label>
                <select
                  id="attractionId"
                  class="form-select"
                  formControlName="attractionId"
                  [class.is-invalid]="maintenanceForm.get('attractionId')?.invalid && maintenanceForm.get('attractionId')?.touched">
                  <option value="">Select an attraction</option>
                  @for (attraction of attractions; track attraction.id) {
                    <option [value]="attraction.id">{{ attraction.name }}</option>
                  }
                </select>
                @if (maintenanceForm.get('attractionId')?.invalid && maintenanceForm.get('attractionId')?.touched) {
                  <div class="invalid-feedback">Attraction is required.</div>
                }
              </div>

              <div class="mb-3">
                <label for="maintenanceType" class="form-label">Maintenance Type *</label>
                <select
                  id="maintenanceType"
                  class="form-select"
                  formControlName="maintenanceType"
                  [class.is-invalid]="maintenanceForm.get('maintenanceType')?.invalid && maintenanceForm.get('maintenanceType')?.touched">
                  <option value="">Select type</option>
                  <option value="Inspection">Inspection</option>
                  <option value="Cleaning">Cleaning</option>
                  <option value="Repair">Repair</option>
                  <option value="SafetyCheck">Safety Check</option>
                </select>
                @if (maintenanceForm.get('maintenanceType')?.invalid && maintenanceForm.get('maintenanceType')?.touched) {
                  <div class="invalid-feedback">Maintenance type is required.</div>
                }
              </div>
            }

            <!-- Performed Date -->
            <div class="mb-3">
              <label for="performedDate" class="form-label">Performed Date & Time *</label>
              <input
                type="datetime-local"
                id="performedDate"
                class="form-control"
                formControlName="performedDate"
                [class.is-invalid]="maintenanceForm.get('performedDate')?.invalid && maintenanceForm.get('performedDate')?.touched">
              @if (maintenanceForm.get('performedDate')?.invalid && maintenanceForm.get('performedDate')?.touched) {
                <div class="invalid-feedback">Performed date is required.</div>
              }
            </div>

            <!-- Duration -->
            <div class="mb-3">
              <label for="durationMinutes" class="form-label">Duration (minutes) *</label>
              <input
                type="number"
                id="durationMinutes"
                class="form-control"
                formControlName="durationMinutes"
                min="1"
                placeholder="e.g., 30"
                [class.is-invalid]="maintenanceForm.get('durationMinutes')?.invalid && maintenanceForm.get('durationMinutes')?.touched">
              @if (maintenanceForm.get('durationMinutes')?.invalid && maintenanceForm.get('durationMinutes')?.touched) {
                <div class="invalid-feedback">
                  Duration is required and must be at least 1 minute.
                </div>
              }
            </div>

            <!-- Description -->
            <div class="mb-3">
              <label for="description" class="form-label">Work Description *</label>
              <textarea
                id="description"
                class="form-control"
                rows="3"
                formControlName="description"
                placeholder="Describe the maintenance work performed..."
                [class.is-invalid]="maintenanceForm.get('description')?.invalid && maintenanceForm.get('description')?.touched"></textarea>
              @if (maintenanceForm.get('description')?.invalid && maintenanceForm.get('description')?.touched) {
                <div class="invalid-feedback">
                  Description is required (minimum 10 characters).
                </div>
              }
            </div>

            <!-- Notes -->
            <div class="mb-3">
              <label for="notes" class="form-label">Additional Notes</label>
              <textarea
                id="notes"
                class="form-control"
                rows="2"
                formControlName="notes"
                placeholder="Any additional observations or comments..."></textarea>
            </div>

            <!-- Buttons -->
            <div class="d-flex justify-content-between">
              @if (selectedSchedule) {
                <button type="button" class="btn btn-secondary" (click)="clearSelection()" [disabled]="loading">
                  Cancel
                </button>
              } @else {
                <div></div>
              }
              <button type="submit" class="btn btn-primary" [disabled]="maintenanceForm.invalid || loading">
                @if (loading) {
                  <span class="spinner-border spinner-border-sm me-2"></span>
                  Recording...
                } @else {
                  <i class="bi bi-check-circle"></i> Record Maintenance
                }
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class OperatorMaintenanceComponent implements OnInit {
  private fb = inject(FormBuilder);
  private maintenanceService = inject(MaintenanceService);
  private attractionService = inject(AttractionService);
  private authService = inject(AuthService);

  maintenanceForm!: FormGroup;
  pendingSchedules: MaintenanceScheduleResponse[] = [];
  attractions: AttractionResponse[] = [];
  selectedSchedule: MaintenanceScheduleResponse | null = null;

  loading = false;
  loadingSchedules = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  ngOnInit() {
    this.initForm();
    this.loadAttractions();
    this.loadPendingSchedules();
    this.setDefaultDateTime();
  }

  initForm() {
    this.maintenanceForm = this.fb.group({
      attractionId: ['', Validators.required],
      maintenanceType: ['', Validators.required],
      performedDate: ['', Validators.required],
      durationMinutes: ['', [Validators.required, Validators.min(1)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      notes: ['']
    });
  }

  setDefaultDateTime() {
    const now = new Date();
    const localDateTime = new Date(now.getTime() - now.getTimezoneOffset() * 60000)
      .toISOString()
      .slice(0, 16);
    this.maintenanceForm.patchValue({ performedDate: localDateTime });
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

  loadPendingSchedules() {
    this.loadingSchedules = true;
    this.maintenanceService.getAllSchedules({ status: 'Pending' }).subscribe({
      next: (schedules) => {
        this.pendingSchedules = schedules;
        this.loadingSchedules = false;
      },
      error: () => {
        this.loadingSchedules = false;
      }
    });
  }

  selectSchedule(schedule: MaintenanceScheduleResponse) {
    this.selectedSchedule = schedule;
    this.maintenanceForm.patchValue({
      attractionId: schedule.attractionId,
      maintenanceType: schedule.maintenanceType,
      description: `Completed: ${schedule.description}`
    });
    this.maintenanceForm.get('attractionId')?.disable();
    this.maintenanceForm.get('maintenanceType')?.disable();
  }

  clearSelection() {
    this.selectedSchedule = null;
    this.maintenanceForm.reset();
    this.setDefaultDateTime();
    this.maintenanceForm.get('attractionId')?.enable();
    this.maintenanceForm.get('maintenanceType')?.enable();
  }

  onSubmit() {
    if (this.maintenanceForm.invalid) {
      this.maintenanceForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const formValue = this.maintenanceForm.getRawValue();
    const request = {
      attractionId: formValue.attractionId,
      maintenanceScheduleId: this.selectedSchedule?.id,
      performedDate: formValue.performedDate,
      maintenanceType: formValue.maintenanceType,
      description: formValue.description,
      durationMinutes: formValue.durationMinutes,
      notes: formValue.notes || undefined
    };

    const observable = this.selectedSchedule
      ? this.maintenanceService.completeMaintenance(this.selectedSchedule.id, request)
      : this.maintenanceService.recordMaintenance(request);

    observable.subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = response.message || 'Maintenance recorded successfully!';
        this.maintenanceForm.reset();
        this.setDefaultDateTime();
        this.clearSelection();
        this.loadPendingSchedules();
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Failed to record maintenance.';
      }
    });
  }
}
