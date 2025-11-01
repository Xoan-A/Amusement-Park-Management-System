import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { AttractionService } from '../../../core/services/attraction.service';
import { MaintenanceScheduleResponse, AttractionResponse } from '../../../core/models/responses';
import { MaintenanceStatus } from '../../../core/models/enums';

@Component({
  selector: 'app-schedule-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
              <label for="attractionFilter" class="form-label">Attraction</label>
              <select id="attractionFilter" class="form-select" [(ngModel)]="selectedAttractionId" (change)="loadSchedules()">
                <option value="">All Attractions</option>
                @for (attraction of attractions; track attraction.id) {
                  <option [value]="attraction.id">{{ attraction.name }}</option>
                }
              </select>
            </div>
            <div class="col-md-3">
              <label for="statusFilter" class="form-label">Status</label>
              <select id="statusFilter" class="form-select" [(ngModel)]="selectedStatus" (change)="loadSchedules()">
                <option value="">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="InProgress">In Progress</option>
                <option value="Completed">Completed</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>
            <div class="col-md-3">
              <label for="dateFrom" class="form-label">From Date</label>
              <input type="date" id="dateFrom" class="form-control" [(ngModel)]="dateFrom" (change)="loadSchedules()">
            </div>
            <div class="col-md-3">
              <label for="dateTo" class="form-label">To Date</label>
              <input type="date" id="dateTo" class="form-control" [(ngModel)]="dateTo" (change)="loadSchedules()">
            </div>
          </div>
          <div class="row mt-3">
            <div class="col-md-12">
              <button class="btn btn-secondary btn-sm me-2" (click)="showOverdueOnly()">
                <i class="bi bi-exclamation-triangle"></i> Show Overdue
              </button>
              <button class="btn btn-secondary btn-sm me-2" (click)="showUpcomingOnly()">
                <i class="bi bi-calendar-event"></i> Show Upcoming (7 days)
              </button>
              <button class="btn btn-outline-secondary btn-sm" (click)="clearFilters()">
                <i class="bi bi-x-circle"></i> Clear Filters
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Alerts -->
      @if (successMessage) {
        <div class="alert alert-success alert-dismissible fade show" role="alert">
          {{ successMessage }}
          <button type="button" class="btn-close" (click)="successMessage = null"></button>
        </div>
      }
      @if (errorMessage) {
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
          {{ errorMessage }}
          <button type="button" class="btn-close" (click)="errorMessage = null"></button>
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
                    <th>Type</th>
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
                        {{ schedule.scheduledDate | date:'short' }}
                        @if (isOverdue(schedule)) {
                          <span class="badge bg-danger ms-2">OVERDUE</span>
                        }
                      </td>
                      <td>{{ schedule.maintenanceType }}</td>
                      <td>{{ schedule.description }}</td>
                      <td>
                        <span [class]="getStatusBadgeClass(schedule.status)">
                          {{ schedule.status }}
                        </span>
                      </td>
                      <td>
                        @if (schedule.status === 'Pending') {
                          <button class="btn btn-sm btn-success me-1" (click)="updateStatus(schedule.id, 'InProgress')" title="Start">
                            <i class="bi bi-play-circle"></i>
                          </button>
                          <button class="btn btn-sm btn-warning me-1" (click)="updateStatus(schedule.id, 'Cancelled')" title="Cancel">
                            <i class="bi bi-x-circle"></i>
                          </button>
                        }
                        @if (schedule.status === 'InProgress') {
                          <button class="btn btn-sm btn-primary me-1" (click)="navigateToComplete(schedule.id)" title="Complete">
                            <i class="bi bi-check-circle"></i>
                          </button>
                        }
                        @if (schedule.status === 'Pending' || schedule.status === 'Cancelled') {
                          <button class="btn btn-sm btn-danger" (click)="deleteSchedule(schedule.id)" title="Delete">
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
          <i class="bi bi-info-circle"></i> No maintenance schedules found. Click "Schedule Maintenance" to create one.
        </div>
      }
    </div>
  `,
  styles: [`
    .table-danger {
      background-color: #f8d7da !important;
    }
  `]
})
export class ScheduleListComponent implements OnInit {
  private maintenanceService = inject(MaintenanceService);
  private attractionService = inject(AttractionService);
  private router = inject(Router);

  schedules: MaintenanceScheduleResponse[] = [];
  attractions: AttractionResponse[] = [];
  loading = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  // Filters
  selectedAttractionId = '';
  selectedStatus = '';
  dateFrom = '';
  dateTo = '';

  ngOnInit() {
    this.loadAttractions();
    this.loadSchedules();
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

  loadSchedules() {
    this.loading = true;
    this.errorMessage = null;

    const params: any = {};
    if (this.selectedAttractionId) params.attractionId = this.selectedAttractionId;
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
      }
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
      }
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
      }
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
    this.maintenanceService.updateScheduleStatus(scheduleId, { status }).subscribe({
      next: (response) => {
        this.successMessage = response.message;
        this.loadSchedules();
      },
      error: () => {
        this.errorMessage = 'Failed to update schedule status.';
      }
    });
  }

  deleteSchedule(scheduleId: string) {
    if (!confirm('Are you sure you want to delete this schedule?')) return;

    this.maintenanceService.deleteSchedule(scheduleId).subscribe({
      next: (response) => {
        this.successMessage = response.message;
        this.loadSchedules();
      },
      error: () => {
        this.errorMessage = 'Failed to delete schedule.';
      }
    });
  }

  navigateToCreateSchedule() {
    this.router.navigate(['/admin/maintenance/schedules/create']);
  }

  navigateToComplete(scheduleId: string) {
    this.router.navigate(['/admin/maintenance/schedules', scheduleId, 'complete']);
  }

  isOverdue(schedule: MaintenanceScheduleResponse): boolean {
    if (schedule.status !== 'Pending') return false;
    const scheduledDate = new Date(schedule.scheduledDate);
    const now = new Date();
    return scheduledDate < now;
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Pending': return 'badge bg-warning';
      case 'InProgress': return 'badge bg-info';
      case 'Completed': return 'badge bg-success';
      case 'Cancelled': return 'badge bg-secondary';
      default: return 'badge bg-secondary';
    }
  }
}
