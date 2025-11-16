import { Component, OnInit, inject } from '@angular/core';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { CommonModule } from '@angular/common';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { MaintenanceScheduleResponse } from '../../../core/models/responses';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';

@Component({
  selector: 'app-operator-maintenance',
  standalone: true,
  imports: [ConfirmationModalComponent, CommonModule, ],
  template: `
    <div class="container mt-4">
      <h2 class="mb-4">Maintenance Schedules</h2>

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

      <!-- Filter Buttons -->
      <div class="mb-3">
        <button class="btn btn-outline-danger me-2" (click)="toggleOverdue()">
          <i class="bi bi-exclamation-triangle"></i>
          {{ showOverdue ? 'Hide' : 'Show' }} Overdue
          @if (overdueSchedules.length > 0) {
            <span class="badge bg-danger ms-1">{{ overdueSchedules.length }}</span>
          }
        </button>
        <button class="btn btn-outline-info" (click)="toggleUpcoming()">
          <i class="bi bi-calendar-event"></i>
          {{ showUpcoming ? 'Hide' : 'Show' }} Upcoming (7 days)
          @if (upcomingSchedules.length > 0) {
            <span class="badge bg-info ms-1">{{ upcomingSchedules.length }}</span>
          }
        </button>
      </div>

      <!-- Overdue Schedules -->
      @if (showOverdue) {
        <div class="card mb-4 border-danger">
          <div class="card-header bg-danger text-white">
            <h5 class="mb-0">
              <i class="bi bi-exclamation-triangle"></i> Overdue Maintenance
              <span class="badge bg-white text-danger ms-2">{{ overdueSchedules.length }}</span>
            </h5>
          </div>
          <div class="card-body">
            @if (loadingOverdue) {
              <div class="text-center py-3">
                <div class="spinner-border text-danger" role="status"></div>
                <p class="mt-2">Loading overdue schedules...</p>
              </div>
            }

            @if (!loadingOverdue && overdueSchedules.length > 0) {
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
                    @for (schedule of overdueSchedules; track schedule.id) {
                      <tr class="table-danger">
                        <td>{{ schedule.attractionName }}</td>
                        <td>
                          {{ schedule.scheduledDate | date:'short' }}
                          <span class="badge bg-danger ms-2">OVERDUE</span>
                        </td>
                        <td>{{ schedule.estimatedDuration }}h</td>
                        <td>{{ schedule.description }}</td>
                        <td>
                          <span class="badge bg-warning">{{ schedule.status }}</span>
                        </td>
                        <td>
                          <button
                            class="btn btn-sm btn-success"
                            (click)="completeMaintenance(schedule.id)"
                            [disabled]="loading"
                            title="Mark as completed">
                            <i class="bi bi-check-circle"></i> Complete
                          </button>
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }

            @if (!loadingOverdue && overdueSchedules.length === 0) {
              <p class="text-success mb-0">
                <i class="bi bi-check-circle"></i> No overdue maintenance schedules!
              </p>
            }
          </div>
        </div>
      }

      <!-- Upcoming Schedules -->
      @if (showUpcoming) {
        <div class="card mb-4 border-info">
          <div class="card-header bg-info text-white">
            <h5 class="mb-0">
              <i class="bi bi-calendar-event"></i> Upcoming Maintenance (Next 7 days)
              <span class="badge bg-white text-info ms-2">{{ upcomingSchedules.length }}</span>
            </h5>
          </div>
          <div class="card-body">
            @if (loadingUpcoming) {
              <div class="text-center py-3">
                <div class="spinner-border text-info" role="status"></div>
                <p class="mt-2">Loading upcoming schedules...</p>
              </div>
            }

            @if (!loadingUpcoming && upcomingSchedules.length > 0) {
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
                    @for (schedule of upcomingSchedules; track schedule.id) {
                      <tr>
                        <td>{{ schedule.attractionName }}</td>
                        <td>{{ schedule.scheduledDate | date:'short' }}</td>
                        <td>{{ schedule.estimatedDuration }}h</td>
                        <td>{{ schedule.description }}</td>
                        <td>
                          <span class="badge bg-warning">{{ schedule.status }}</span>
                        </td>
                        <td>
                          <button
                            class="btn btn-sm btn-success"
                            (click)="completeMaintenance(schedule.id)"
                            [disabled]="loading"
                            title="Mark as completed">
                            <i class="bi bi-check-circle"></i> Complete
                          </button>
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }

            @if (!loadingUpcoming && upcomingSchedules.length === 0) {
              <p class="text-muted mb-0">No upcoming maintenance scheduled for the next 7 days.</p>
            }
          </div>
        </div>
      }

      <!-- Active Schedules -->
      <div class="card mb-4">
        <div class="card-header">
          <h5 class="mb-0">
            <i class="bi bi-gear-fill"></i> Active Maintenance (In Progress)
            <span class="badge bg-primary ms-2">{{ activeSchedules.length }}</span>
          </h5>
        </div>
        <div class="card-body">
          @if (loadingSchedules) {
            <div class="text-center py-3">
              <div class="spinner-border" role="status"></div>
              <p class="mt-2">Loading schedules...</p>
            </div>
          }

          @if (!loadingSchedules && activeSchedules.length > 0) {
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
                  @for (schedule of activeSchedules; track schedule.id) {
                    <tr [class.table-danger]="schedule.isOverdue">
                      <td>{{ schedule.attractionName }}</td>
                      <td>
                        {{ schedule.scheduledDate | date:'short' }}
                        @if (schedule.isOverdue) {
                          <span class="badge bg-danger ms-2">OVERDUE</span>
                        }
                      </td>
                      <td>{{ schedule.estimatedDuration }}h</td>
                      <td>{{ schedule.description }}</td>
                      <td>
                        <span class="badge bg-primary">{{ schedule.status }}</span>
                      </td>
                      <td>
                        <button
                          class="btn btn-sm btn-success"
                          (click)="completeMaintenance(schedule.id)"
                          [disabled]="loading"
                          title="Mark as completed">
                          <i class="bi bi-check-circle"></i> Complete
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }

          @if (!loadingSchedules && activeSchedules.length === 0) {
            <p class="text-muted mb-0">No active maintenance in progress.</p>
          }
        </div>
      </div>

      <!-- Completed Schedules -->
      <div class="card">
        <div class="card-header">
          <h5 class="mb-0">
            <i class="bi bi-check-circle"></i> Recently Completed Maintenance
          </h5>
        </div>
        <div class="card-body">
          @if (loadingCompleted) {
            <div class="text-center py-3">
              <div class="spinner-border spinner-border-sm" role="status"></div>
            </div>
          }

          @if (!loadingCompleted && completedSchedules.length > 0) {
            <div class="table-responsive">
              <table class="table">
                <thead>
                  <tr>
                    <th>Attraction</th>
                    <th>Scheduled Date</th>
                    <th>Duration</th>
                    <th>Description</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  @for (schedule of completedSchedules; track schedule.id) {
                    <tr>
                      <td>{{ schedule.attractionName }}</td>
                      <td>{{ schedule.scheduledDate | date:'short' }}</td>
                      <td>{{ schedule.estimatedDuration }}h</td>
                      <td>{{ schedule.description }}</td>
                      <td>
                        <span class="badge bg-success">{{ schedule.status }}</span>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }

          @if (!loadingCompleted && completedSchedules.length === 0) {
            <p class="text-muted mb-0">No completed maintenance schedules.</p>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .table-danger {
      background-color: #f8d7da !important;
    }
  `]
})
export class OperatorMaintenanceComponent implements OnInit {
  private maintenanceService = inject(MaintenanceService);

  activeSchedules: MaintenanceScheduleResponse[] = [];
  completedSchedules: MaintenanceScheduleResponse[] = [];
  overdueSchedules: MaintenanceScheduleResponse[] = [];
  upcomingSchedules: MaintenanceScheduleResponse[] = [];

  loading = false;
  showDeleteModal = false;
  itemToDelete: any = null;
  loadingSchedules = false;
  showDeleteModal = false;
  itemToDelete: any = null;
  loadingCompleted = false;
  showDeleteModal = false;
  itemToDelete: any = null;
  loadingOverdue = false;
  showDeleteModal = false;
  itemToDelete: any = null;
  loadingUpcoming = false;
  showDeleteModal = false;
  itemToDelete: any = null;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  showOverdue = false;
  showUpcoming = false;

  ngOnInit() {
    this.loadActiveSchedules();
    this.loadCompletedSchedules();
  }

  loadActiveSchedules() {
    this.loadingSchedules = true;
  showDeleteModal = false;
  itemToDelete: any = null;
    this.maintenanceService.getAllSchedules({ status: 'InProgress' }).subscribe({
      next: (schedules) => {
        this.activeSchedules = schedules;
        this.loadingSchedules = false;
  showDeleteModal = false;
  itemToDelete: any = null;
      },
      error: () => {
        this.errorMessage = 'Failed to load active schedules.';
        this.loadingSchedules = false;
  showDeleteModal = false;
  itemToDelete: any = null;
      }
    });
  }

  loadCompletedSchedules() {
    this.loadingCompleted = true;
  showDeleteModal = false;
  itemToDelete: any = null;
    this.maintenanceService.getAllSchedules({ status: 'Completed' }).subscribe({
      next: (schedules) => {
        this.completedSchedules = schedules.slice(0, 10);
        this.loadingCompleted = false;
  showDeleteModal = false;
  itemToDelete: any = null;
      },
      error: () => {
        this.loadingCompleted = false;
  showDeleteModal = false;
  itemToDelete: any = null;
      }
    });
  }

  loadOverdueSchedules() {
    this.loadingOverdue = true;
  showDeleteModal = false;
  itemToDelete: any = null;
    this.maintenanceService.getOverdueSchedules().subscribe({
      next: (schedules) => {
        this.overdueSchedules = schedules;
        this.loadingOverdue = false;
  showDeleteModal = false;
  itemToDelete: any = null;
      },
      error: () => {
        this.errorMessage = 'Failed to load overdue schedules.';
        this.loadingOverdue = false;
  showDeleteModal = false;
  itemToDelete: any = null;
      }
    });
  }

  loadUpcomingSchedules() {
    this.loadingUpcoming = true;
  showDeleteModal = false;
  itemToDelete: any = null;
    this.maintenanceService.getUpcomingSchedules(7).subscribe({
      next: (schedules) => {
        this.upcomingSchedules = schedules;
        this.loadingUpcoming = false;
  showDeleteModal = false;
  itemToDelete: any = null;
      },
      error: () => {
        this.errorMessage = 'Failed to load upcoming schedules.';
        this.loadingUpcoming = false;
  showDeleteModal = false;
  itemToDelete: any = null;
      }
    });
  }

  toggleOverdue() {
    this.showOverdue = !this.showOverdue;
    if (this.showOverdue && this.overdueSchedules.length === 0) {
      this.loadOverdueSchedules();
    }
  }

  toggleUpcoming() {
    this.showUpcoming = !this.showUpcoming;
    if (this.showUpcoming && this.upcomingSchedules.length === 0) {
      this.loadUpcomingSchedules();
    }
  }

  completeMaintenance(scheduleId: string) {
    if (!confirm('Mark this maintenance schedule as completed?')) {
      return;
    }

    this.loading = true;
  showDeleteModal = false;
  itemToDelete: any = null;
    this.errorMessage = null;
    this.successMessage = null;

    this.maintenanceService.updateScheduleStatus(scheduleId, { status: 'Completed' }).subscribe({
      next: (response) => {
        this.loading = false;
  showDeleteModal = false;
  itemToDelete: any = null;
        this.successMessage = response.message || 'Maintenance completed successfully!';
        this.loadActiveSchedules();
        this.loadCompletedSchedules();
        if (this.showOverdue) this.loadOverdueSchedules();
        if (this.showUpcoming) this.loadUpcomingSchedules();
      },
      error: (error) => {
        this.loading = false;
  showDeleteModal = false;
  itemToDelete: any = null;
        this.errorMessage = error.error?.message || 'Failed to complete maintenance.';
      }
    });
  }
}
