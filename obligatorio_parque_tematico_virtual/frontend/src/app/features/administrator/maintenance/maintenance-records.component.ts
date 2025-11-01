import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { AttractionService } from '../../../core/services/attraction.service';
import { MaintenanceRecordResponse, AttractionResponse, AllAttractionsResponse } from '../../../core/models/responses';

@Component({
  selector: 'app-maintenance-records',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    <div class="container mt-4">
      <h2 class="mb-4">Maintenance History</h2>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row">
            <div class="col-md-4">
              <label for="attractionFilter" class="form-label">Attraction</label>
              <select id="attractionFilter" class="form-select" [(ngModel)]="selectedAttractionId" (change)="loadRecords()">
                <option value="">All Attractions</option>
                @for (attraction of attractions; track attraction.id) {
                  <option [value]="attraction.id">{{ attraction.name }}</option>
                }
              </select>
            </div>
            <div class="col-md-4">
              <label for="typeFilter" class="form-label">Maintenance Type</label>
              <select id="typeFilter" class="form-select" [(ngModel)]="selectedType" (change)="loadRecords()">
                <option value="">All Types</option>
                <option value="Inspection">Inspection</option>
                <option value="Cleaning">Cleaning</option>
                <option value="Repair">Repair</option>
                <option value="SafetyCheck">Safety Check</option>
              </select>
            </div>
            <div class="col-md-2">
              <label for="dateFrom" class="form-label">From Date</label>
              <input type="date" id="dateFrom" class="form-control" [(ngModel)]="dateFrom" (change)="loadRecords()">
            </div>
            <div class="col-md-2">
              <label for="dateTo" class="form-label">To Date</label>
              <input type="date" id="dateTo" class="form-control" [(ngModel)]="dateTo" (change)="loadRecords()">
            </div>
          </div>
          <div class="row mt-3">
            <div class="col-md-12">
              <button class="btn btn-secondary btn-sm me-2" (click)="showUnscheduledOnly()">
                <i class="bi bi-exclamation-circle"></i> Show Unscheduled Only
              </button>
              <button class="btn btn-outline-secondary btn-sm" (click)="clearFilters()">
                <i class="bi bi-x-circle"></i> Clear Filters
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Error Message -->
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

      <!-- Records Table -->
      @if (!loading && records.length > 0) {
        <div class="card">
          <div class="card-body">
            <div class="table-responsive">
              <table class="table table-hover">
                <thead>
                  <tr>
                    <th>Performed Date</th>
                    <th>Attraction</th>
                    <th>Type</th>
                    <th>Performed By</th>
                    <th>Duration</th>
                    <th>Description</th>
                    <th>Notes</th>
                    <th>Scheduled</th>
                  </tr>
                </thead>
                <tbody>
                  @for (record of records; track record.id) {
                    <tr>
                      <td>{{ record.performedDate | date:'short' }}</td>
                      <td>{{ record.attractionName }}</td>
                      <td>
                        <span class="badge bg-secondary">{{ record.maintenanceType }}</span>
                      </td>
                      <td>{{ record.performedByName }}</td>
                      <td>{{ record.durationMinutes }} min</td>
                      <td>{{ record.description }}</td>
                      <td>
                        @if (record.notes) {
                          <small class="text-muted">{{ record.notes }}</small>
                        } @else {
                          <span class="text-muted">-</span>
                        }
                      </td>
                      <td>
                        @if (record.maintenanceScheduleId) {
                          <span class="badge bg-success">Scheduled</span>
                        } @else {
                          <span class="badge bg-warning">Unscheduled</span>
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
      @if (!loading && records.length === 0) {
        <div class="alert alert-info">
          <i class="bi bi-info-circle"></i> No maintenance records found.
        </div>
      }
    </div>
  `
})
export class MaintenanceRecordsComponent implements OnInit {
  private maintenanceService = inject(MaintenanceService);
  private attractionService = inject(AttractionService);

  records: MaintenanceRecordResponse[] = [];
  attractions: AttractionResponse[] = [];
  loading = false;
  errorMessage: string | null = null;

  // Filters
  selectedAttractionId = '';
  selectedType = '';
  dateFrom = '';
  dateTo = '';

  ngOnInit() {
    this.loadAttractions();
    this.loadRecords();
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

  loadRecords() {
    this.loading = true;
    this.errorMessage = null;

    const params: any = {};
    if (this.selectedAttractionId) params.attractionId = this.selectedAttractionId;
    if (this.selectedType) params.maintenanceType = this.selectedType;
    if (this.dateFrom) params.dateFrom = this.dateFrom;
    if (this.dateTo) params.dateTo = this.dateTo;

    this.maintenanceService.getAllRecords(params).subscribe({
      next: (records) => {
        this.records = records;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load maintenance records.';
        this.loading = false;
      }
    });
  }

  showUnscheduledOnly() {
    this.loading = true;
    this.clearFilterValues();
    this.maintenanceService.getUnscheduledRecords().subscribe({
      next: (records) => {
        this.records = records;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load unscheduled records.';
        this.loading = false;
      }
    });
  }

  clearFilters() {
    this.clearFilterValues();
    this.loadRecords();
  }

  private clearFilterValues() {
    this.selectedAttractionId = '';
    this.selectedType = '';
    this.dateFrom = '';
    this.dateTo = '';
  }
}
