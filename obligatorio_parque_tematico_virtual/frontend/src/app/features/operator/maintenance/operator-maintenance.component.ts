import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { MaintenanceScheduleResponse } from '../../../core/models/responses';
import { ConfirmationModalComponent } from '../../../shared/components/confirmation-modal/confirmation-modal.component';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-operator-maintenance',
  standalone: true,
  imports: [CommonModule, ConfirmationModalComponent],
  templateUrl: './operator-maintenance.component.html',
  styles: [`
    .table-danger {
      background-color: #f8d7da !important;
    }
  `]
})
export class OperatorMaintenanceComponent implements OnInit {
  private maintenanceService = inject(MaintenanceService);
  private toastService = inject(ToastService);

  activeSchedules: MaintenanceScheduleResponse[] = [];
  completedSchedules: MaintenanceScheduleResponse[] = [];
  overdueSchedules: MaintenanceScheduleResponse[] = [];
  upcomingSchedules: MaintenanceScheduleResponse[] = [];

  loading = false;
  loadingSchedules = false;
  loadingCompleted = false;
  loadingOverdue = false;
  loadingUpcoming = false;
  errorMessage: string | null = null;

  showOverdue = false;
  showUpcoming = false;
  showCompleteModal = false;
  scheduleToComplete: string | null = null;

  ngOnInit() {
    this.loadActiveSchedules();
    this.loadCompletedSchedules();
  }

  loadActiveSchedules() {
    this.loadingSchedules = true;
    this.maintenanceService.getAllSchedules().subscribe({
      next: (schedules) => {
        this.activeSchedules = schedules.filter(s => s.status === 'InProgress');
        this.loadingSchedules = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load active schedules.';
        this.loadingSchedules = false;
      }
    });
  }

  loadCompletedSchedules() {
    this.loadingCompleted = true;
    this.maintenanceService.getAllSchedules().subscribe({
      next: (schedules) => {
        this.completedSchedules = schedules.filter(s => s.status === 'Completed').slice(0, 10);
        this.loadingCompleted = false;
      },
      error: () => {
        this.loadingCompleted = false;
      }
    });
  }

  loadOverdueSchedules() {
    this.loadingOverdue = true;
    this.maintenanceService.getOverdueSchedules().subscribe({
      next: (schedules) => {
        this.overdueSchedules = schedules;
        this.loadingOverdue = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load overdue schedules.';
        this.loadingOverdue = false;
      }
    });
  }

  loadUpcomingSchedules() {
    this.loadingUpcoming = true;
    this.maintenanceService.getUpcomingSchedules(7).subscribe({
      next: (schedules) => {
        this.upcomingSchedules = schedules;
        this.loadingUpcoming = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load upcoming schedules.';
        this.loadingUpcoming = false;
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
    this.scheduleToComplete = scheduleId;
    this.showCompleteModal = true;
  }

  confirmComplete() {
    if (this.scheduleToComplete) {
      this.loading = true;

      this.maintenanceService.completeSchedule(this.scheduleToComplete).subscribe({
        next: (response) => {
          this.loading = false;
          this.toastService.showSuccess(response.message || 'Maintenance completed successfully!');
          this.loadActiveSchedules();
          this.loadCompletedSchedules();
          if (this.showOverdue) this.loadOverdueSchedules();
          if (this.showUpcoming) this.loadUpcomingSchedules();
          this.scheduleToComplete = null;
          this.showCompleteModal = false;
          this.errorMessage = null;
        },
        error: (error) => {
          this.loading = false;
          this.errorMessage = error.error?.message || 'Failed to complete maintenance.';
          this.scheduleToComplete = null;
          this.showCompleteModal = false;
        }
      });
    }
  }

  cancelComplete() {
    this.scheduleToComplete = null;
    this.showCompleteModal = false;
  }
}
