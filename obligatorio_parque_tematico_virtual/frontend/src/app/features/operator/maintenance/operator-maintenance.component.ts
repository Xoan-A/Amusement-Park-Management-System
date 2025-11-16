import { Component, OnInit, inject } from '@angular/core';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { CommonModule } from '@angular/common';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { MaintenanceScheduleResponse } from '../../../core/models/responses';

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
                          <span class="badge bg-warning">{{ schedule.status }}</span>
                          <button
                            class="btn btn-sm btn-success"
                            (click)="completeMaintenance(schedule.id)"
                            [disabled]="loading"
                            title="Mark as completed">
                            <i class="bi bi-check-circle"></i> Complete
                          </button>
                      </tr>
                    }
                  </tbody>
                </table>
            @if (!loadingOverdue && overdueSchedules.length === 0) {
              <p class="text-success mb-0">
                <i class="bi bi-check-circle"></i> No overdue maintenance schedules!
              </p>
      <!-- Upcoming Schedules -->
      @if (showUpcoming) {
        <div class="card mb-4 border-info">
          <div class="card-header bg-info text-white">
              <i class="bi bi-calendar-event"></i> Upcoming Maintenance (Next 7 days)
              <span class="badge bg-white text-info ms-2">{{ upcomingSchedules.length }}</span>
            @if (loadingUpcoming) {
                <div class="spinner-border text-info" role="status"></div>
                <p class="mt-2">Loading upcoming schedules...</p>
            @if (!loadingUpcoming && upcomingSchedules.length > 0) {
                    @for (schedule of upcomingSchedules; track schedule.id) {
                      <tr>
                        <td>{{ schedule.scheduledDate | date:'short' }}</td>
            @if (!loadingUpcoming && upcomingSchedules.length === 0) {
              <p class="text-muted mb-0">No upcoming maintenance scheduled for the next 7 days.</p>
      <!-- Active Schedules -->
      <div class="card mb-4">
        <div class="card-header">
          <h5 class="mb-0">
            <i class="bi bi-gear-fill"></i> Active Maintenance (In Progress)
            <span class="badge bg-primary ms-2">{{ activeSchedules.length }}</span>
          </h5>
        <div class="card-body">
          @if (loadingSchedules) {
            <div class="text-center py-3">
              <div class="spinner-border" role="status"></div>
              <p class="mt-2">Loading schedules...</p>
            </div>
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
                        }
                      </td>
                      <td>{{ schedule.estimatedDuration }}h</td>
                      <td>{{ schedule.description }}</td>
                        <span class="badge bg-primary">{{ schedule.status }}</span>
                        <button
                          class="btn btn-sm btn-success"
                          (click)="completeMaintenance(schedule.id)"
                          [disabled]="loading"
                          title="Mark as completed">
                          <i class="bi bi-check-circle"></i> Complete
                        </button>
                  }
                </tbody>
              </table>
          @if (!loadingSchedules && activeSchedules.length === 0) {
            <p class="text-muted mb-0">No active maintenance in progress.</p>
      <!-- Completed Schedules -->
      <div class="card">
            <i class="bi bi-check-circle"></i> Recently Completed Maintenance
          @if (loadingCompleted) {
              <div class="spinner-border spinner-border-sm" role="status"></div>
          @if (!loadingCompleted && completedSchedules.length > 0) {
              <table class="table">
                  @for (schedule of completedSchedules; track schedule.id) {
                      <td>{{ schedule.scheduledDate | date:'short' }}</td>
                        <span class="badge bg-success">{{ schedule.status }}</span>
          @if (!loadingCompleted && completedSchedules.length === 0) {
            <p class="text-muted mb-0">No completed maintenance schedules.</p>
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
  loadingCompleted = false;
  loadingOverdue = false;
  loadingUpcoming = false;
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
    this.maintenanceService.getAllSchedules({ status: 'InProgress' }).subscribe({
      next: (schedules) => {
        this.activeSchedules = schedules;
        this.loadingSchedules = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load active schedules.';
    });
  loadCompletedSchedules() {
    this.loadingCompleted = true;
    this.maintenanceService.getAllSchedules({ status: 'Completed' }).subscribe({
        this.completedSchedules = schedules.slice(0, 10);
        this.loadingCompleted = false;
  loadOverdueSchedules() {
    this.loadingOverdue = true;
    this.maintenanceService.getOverdueSchedules().subscribe({
        this.overdueSchedules = schedules;
        this.loadingOverdue = false;
        this.errorMessage = 'Failed to load overdue schedules.';
  loadUpcomingSchedules() {
    this.loadingUpcoming = true;
    this.maintenanceService.getUpcomingSchedules(7).subscribe({
        this.upcomingSchedules = schedules;
        this.loadingUpcoming = false;
        this.errorMessage = 'Failed to load upcoming schedules.';
  toggleOverdue() {
    this.showOverdue = !this.showOverdue;
    if (this.showOverdue && this.overdueSchedules.length === 0) {
      this.loadOverdueSchedules();
  toggleUpcoming() {
    this.showUpcoming = !this.showUpcoming;
    if (this.showUpcoming && this.upcomingSchedules.length === 0) {
      this.loadUpcomingSchedules();
  completeMaintenance(scheduleId: string) {
    if (!confirm('Mark this maintenance schedule as completed?')) {
      return;
    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;
    this.maintenanceService.updateScheduleStatus(scheduleId, { status: 'Completed' }).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = response.message || 'Maintenance completed successfully!';
        this.loadActiveSchedules();
        this.loadCompletedSchedules();
        if (this.showOverdue) this.loadOverdueSchedules();
        if (this.showUpcoming) this.loadUpcomingSchedules();
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to complete maintenance.';
}
