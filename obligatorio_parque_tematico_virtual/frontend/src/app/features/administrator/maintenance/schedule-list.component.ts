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
  templateUrl: './schedule-list.component.html',
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

    this.maintenanceService.getAllSchedules().subscribe({
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
