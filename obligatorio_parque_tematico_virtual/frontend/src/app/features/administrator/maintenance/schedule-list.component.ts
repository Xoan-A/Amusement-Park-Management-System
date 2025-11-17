import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { AttractionService } from '../../../core/services/attraction.service';
import {
  MaintenanceScheduleResponse,
  AttractionResponse,
  AllAttractionsResponse,
} from '../../../core/models/responses';
import { MaintenanceStatus } from '../../../core/models/enums';
import { ConfirmationModalComponent } from '../../../shared/components/confirmation-modal/confirmation-modal.component';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-schedule-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ConfirmationModalComponent],
  template: `
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Maintenance Schedules</h2>
        <button class="btn btn-primary" (click)="navigateToCreateSchedule()">
          <i class="bi bi-plus-circle"></i> Schedule Maintenance
        </button>
      </div>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row">
            <div class="col-md-3">
              <label for="attractionFilter" class="form-label"
                >Attraction</label
              >
              <select
                id="attractionFilter"
                class="form-select"
                [(ngModel)]="selectedAttractionId"
                (change)="loadSchedules()"
              >
                <option value="">All Attractions</option>
                @for (attraction of attractions; track attraction.id) {
                <option [value]="attraction.id">{{ attraction.name }}</option>
                }
              </select>
            </div>
            <div class="col-md-3">
              <label for="statusFilter" class="form-label">Status</label>
              <select
                id="statusFilter"
                class="form-select"
                [(ngModel)]="selectedStatus"
                (change)="loadSchedules()"
              >
                <option value="">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="InProgress">In Progress</option>
                <option value="Completed">Completed</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>
            <div class="col-md-3">
              <label for="dateFrom" class="form-label">From Date</label>
              <input
                type="date"
                id="dateFrom"
                class="form-control"
                [(ngModel)]="dateFrom"
                (change)="loadSchedules()"
              />
            </div>
            <div class="col-md-3">
              <label for="dateTo" class="form-label">To Date</label>
              <input
                type="date"
                id="dateTo"
                class="form-control"
                [(ngModel)]="dateTo"
                (change)="loadSchedules()"
              />
            </div>
          </div>
          <div class="row mt-3">
            <div class="col-md-12">
              <button
                class="btn btn-secondary btn-sm me-2"
                (click)="showOverdueOnly()"
              >
                <i class="bi bi-exclamation-triangle"></i> Show Overdue
              </button>
              <button
                class="btn btn-secondary btn-sm me-2"
                (click)="showUpcomingOnly()"
              >
                <i class="bi bi-calendar-event"></i> Show Upcoming (7 days)
              </button>
              <button
                class="btn btn-outline-secondary btn-sm"
                (click)="clearFilters()"
              >
                <i class="bi bi-x-circle"></i> Clear Filters
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Alerts -->
      @if (errorMessage) {
      <div class="alert alert-danger alert-dismissible fade show" role="alert">
        {{ errorMessage }}
        <button
          type="button"
          class="btn-close"
          (click)="errorMessage = null"
        ></button>
      </div>
      }

      <!-- Loading -->
      @if (loading) {
      <div class="text-center my-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>
      }

      <!-- Schedules Table -->
      @if (!loading && schedules.length > 0) {
      <div class="card">
        <div class="card-body">
          <div class="table-responsive">
            <table class="table table-hover">
              <thead>
                <tr>
                  <th>Attraction</th>
                  <th>Scheduled Date</th>
                  <th>Duration</th>
                  <th>Description</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (schedule of schedules; track schedule.id) {
                <tr [class.table-danger]="isOverdue(schedule)">
                  <td>{{ schedule.attractionName }}</td>
                  <td>
                    {{ schedule.scheduledDate | date : 'd/M/yyyy HH:mm' }}
                    @if (isOverdue(schedule)) {
                    <span class="badge bg-danger ms-2">OVERDUE</span>
                    }
                  </td>
                  <td>{{ schedule.estimatedDuration }}h</td>
                  <td>{{ schedule.description }}</td>
                  <td>
                    <span [class]="getStatusBadgeClass(schedule.status)">
                      {{ schedule.status }}
                    </span>
                  </td>
                  <td>
                    @if (schedule.status === 'Pending') {
                    <button
                      class="btn btn-sm btn-success me-1"
                      (click)="updateStatus(schedule.id, 'InProgress')"
                      title="Start"
                    >
                      <i class="bi bi-play-circle"></i>
                    </button>
                    <button
                      class="btn btn-sm btn-warning me-1"
                      (click)="updateStatus(schedule.id, 'Cancelled')"
                      title="Cancel"
                    >
                      <i class="bi bi-x-circle"></i>
                    </button>
                    } @if (schedule.status === 'InProgress') {
                    <button
                      class="btn btn-sm btn-primary me-1"
                      (click)="completeMaintenance(schedule.id)"
                      title="Complete"
                    >
                      <i class="bi bi-check-circle"></i>
                    </button>
                    } @if (schedule.status === 'Pending' || schedule.status ===
                    'Cancelled') {
                    <button
                      class="btn btn-sm btn-danger"
                      (click)="deleteSchedule(schedule.id)"
                      title="Delete"
                    >
                      <i class="bi bi-trash"></i>
                    </button>
                    }
                  </td>
                </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      </div>
      }

      <!-- No Results -->
      @if (!loading && schedules.length === 0) {
      <div class="alert alert-info">
        <i class="bi bi-info-circle"></i> No maintenance schedules found. Click
        "Schedule Maintenance" to create one.
      </div>
      }
    </div>

    <app-confirmation-modal
      [show]="showDeleteModal"
      title="Delete Schedule"
      message="Are you sure you want to delete this schedule?"
      (confirmed)="confirmDeleteSchedule()"
      (cancelled)="cancelDeleteSchedule()">
    </app-confirmation-modal>

    <app-confirmation-modal
      [show]="showCompleteModal"
      title="Complete Maintenance"
      message="Mark this maintenance schedule as completed?"
      (confirmed)="confirmCompleteMaintenance()"
      (cancelled)="cancelCompleteMaintenance()">
    </app-confirmation-modal>
  `,
  styles: [
    `
      .table-danger {
        background-color: #f8d7da !important;
      }
    `,
  ],
})
export class ScheduleListComponent implements OnInit {
  private maintenanceService = inject(MaintenanceService);
  private attractionService = inject(AttractionService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  schedules: MaintenanceScheduleResponse[] = [];
  attractions: AttractionResponse[] = [];
  loading = false;
  errorMessage: string | null = null;

  selectedAttractionId = '';
  selectedStatus = '';
  dateFrom = '';
  dateTo = '';

  showDeleteModal = false;
  showCompleteModal = false;
  scheduleToDelete: string | null = null;
  scheduleToComplete: string | null = null;

  ngOnInit() {
    this.loadAttractions();
    this.loadSchedules();
  }

  loadAttractions() {
    this.attractionService.getAll().subscribe({
      next: (response: AllAttractionsResponse) => {
        this.attractions = response.attractions;
      },
      error: () => {
        this.errorMessage = 'Failed to load attractions.';
      },
    });
  }

  loadSchedules() {
    this.loading = true;
    this.errorMessage = null;

    const params: any = {};
    if (this.selectedAttractionId)
      params.attractionId = this.selectedAttractionId;
    if (this.selectedStatus) params.status = this.selectedStatus;
    if (this.dateFrom) params.dateFrom = this.dateFrom;
    if (this.dateTo) params.dateTo = this.dateTo;

    this.maintenanceService.getAllSchedules(params).subscribe({
      next: (schedules) => {
        this.schedules = schedules;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load maintenance schedules.';
        this.loading = false;
      },
    });
  }

  showOverdueOnly() {
    this.loading = true;
    this.clearFilterValues();
    this.maintenanceService.getOverdueSchedules().subscribe({
      next: (schedules) => {
        this.schedules = schedules;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load overdue schedules.';
        this.loading = false;
      },
    });
  }

  showUpcomingOnly() {
    this.loading = true;
    this.clearFilterValues();
    this.maintenanceService.getUpcomingSchedules(7).subscribe({
      next: (schedules) => {
        this.schedules = schedules;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load upcoming schedules.';
        this.loading = false;
      },
    });
  }

  clearFilters() {
    this.clearFilterValues();
    this.loadSchedules();
  }

  private clearFilterValues() {
    this.selectedAttractionId = '';
    this.selectedStatus = '';
    this.dateFrom = '';
    this.dateTo = '';
  }

  updateStatus(scheduleId: string, status: string) {
    this.maintenanceService
      .updateScheduleStatus(scheduleId, { status })
      .subscribe({
        next: (response) => {
          this.toastService.showSuccess(response.message || 'Schedule status updated successfully');
          this.loadSchedules();
        },
        error: () => {
          this.errorMessage = 'Failed to update schedule status.';
        },
      });
  }

  deleteSchedule(scheduleId: string) {
    this.scheduleToDelete = scheduleId;
    this.showDeleteModal = true;
  }

  confirmDeleteSchedule() {
    if (this.scheduleToDelete) {
      this.maintenanceService.deleteSchedule(this.scheduleToDelete).subscribe({
        next: (response) => {
          this.toastService.showSuccess(response.message || 'Schedule deleted successfully');
          this.loadSchedules();
          this.scheduleToDelete = null;
          this.showDeleteModal = false;
        },
        error: () => {
          this.errorMessage = 'Failed to delete schedule.';
          this.scheduleToDelete = null;
          this.showDeleteModal = false;
        },
      });
    }
  }

  cancelDeleteSchedule() {
    this.scheduleToDelete = null;
    this.showDeleteModal = false;
  }

  navigateToCreateSchedule() {
    this.router.navigate(['/admin/maintenance/schedules/create']);
  }

  completeMaintenance(scheduleId: string) {
    this.scheduleToComplete = scheduleId;
    this.showCompleteModal = true;
  }

  confirmCompleteMaintenance() {
    if (this.scheduleToComplete) {
      this.maintenanceService.completeSchedule(this.scheduleToComplete).subscribe({
        next: (response) => {
          this.toastService.showSuccess(response.message || 'Maintenance completed successfully!');
          this.loadSchedules();
          this.scheduleToComplete = null;
          this.showCompleteModal = false;
        },
        error: () => {
          this.errorMessage = 'Failed to complete maintenance.';
          this.scheduleToComplete = null;
          this.showCompleteModal = false;
        },
      });
    }
  }

  cancelCompleteMaintenance() {
    this.scheduleToComplete = null;
    this.showCompleteModal = false;
  }

  isOverdue(schedule: MaintenanceScheduleResponse): boolean {
    if (schedule.status !== 'Pending') return false;
    const scheduledDate = new Date(schedule.scheduledDate);
    const now = new Date();
    return scheduledDate < now;
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Pending':
        return 'badge bg-warning';
      case 'InProgress':
        return 'badge bg-info';
      case 'Completed':
        return 'badge bg-success';
      case 'Cancelled':
        return 'badge bg-secondary';
      default:
        return 'badge bg-secondary';
    }
  }
}
