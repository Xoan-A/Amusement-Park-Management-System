import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AttractionService } from '../../../core/services/attraction.service';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { AttractionResponse } from '../../../core/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container mt-4">
      <h1 class="mb-4">Operator Dashboard</h1>

      <div class="row g-4">
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Total Attractions</h5>
              <p class="display-4">{{ attractions.length }}</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Active Attractions</h5>
              <p class="display-4 text-success">{{ getActiveCount() }}</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card text-center">
            <div class="card-body">
              <h5 class="card-title">Inactive Attractions</h5>
              <p class="display-4 text-danger">{{ getInactiveCount() }}</p>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div
            class="card text-center"
            [class.border-warning]="pendingMaintenanceCount > 0"
          >
            <div class="card-body">
              <h5 class="card-title">Pending Maintenance</h5>
              <p
                class="display-4"
                [class.text-warning]="pendingMaintenanceCount > 0"
                [class.text-muted]="pendingMaintenanceCount === 0"
              >
                {{ pendingMaintenanceCount }}
              </p>
              <a
                routerLink="/operator/maintenance"
                class="btn btn-sm"
                [class.btn-warning]="pendingMaintenanceCount > 0"
                [class.btn-outline-secondary]="pendingMaintenanceCount === 0"
              >
                Record Work
              </a>
            </div>
          </div>
        </div>
      </div>

      <div class="row mt-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Quick Actions</h5>
            </div>
            <div class="card-body">
              <div class="d-flex gap-2">
                <a routerLink="/operator/entry-exit" class="btn btn-primary"
                  >Manage Entry/Exit</a
                >
                <a routerLink="/operator/incidents" class="btn btn-warning"
                  >Manage Incidents</a
                >
                <a routerLink="/operator/maintenance" class="btn btn-info"
                  >Record Maintenance</a
                >
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="row mt-4">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Current Capacity Status</h5>
            </div>
            <div class="card-body">
              <div class="table-responsive">
                <table class="table table-striped">
                  <thead>
                    <tr>
                      <th>Attraction</th>
                      <th>Capacity</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (attraction of attractions; track attraction.id) {
                    <tr>
                      <td>{{ attraction.name }}</td>
                      <td>
                        {{ attraction.currentCapacity }} /
                        {{ attraction.maxCapacity }}
                        <div class="progress" style="height: 10px;">
                          <div
                            class="progress-bar"
                            [style.width.%]="
                              (attraction.currentCapacity /
                                attraction.maxCapacity) *
                              100
                            "
                            [class.bg-success]="
                              attraction.currentCapacity /
                                attraction.maxCapacity <
                              0.7
                            "
                            [class.bg-warning]="
                              attraction.currentCapacity /
                                attraction.maxCapacity >=
                                0.7 &&
                              attraction.currentCapacity /
                                attraction.maxCapacity <
                                0.9
                            "
                            [class.bg-danger]="
                              attraction.currentCapacity /
                                attraction.maxCapacity >=
                              0.9
                            "
                          ></div>
                        </div>
                      </td>
                      <td>
                        @if (attraction.isActive) {
                        <span class="badge bg-success">Active</span>
                        } @else {
                        <span class="badge bg-danger">Inactive</span>
                        }
                      </td>
                    </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [],
})
export class DashboardComponent implements OnInit {
  attractions: AttractionResponse[] = [];
  pendingMaintenanceCount = 0;

  constructor(
    private attractionService: AttractionService,
    private maintenanceService: MaintenanceService
  ) {}

  ngOnInit(): void {
    this.loadAttractions();
    this.loadPendingMaintenance();
    setInterval(() => {
      this.loadAttractions();
      this.loadPendingMaintenance();
    }, 30000);
  }

  loadAttractions(): void {
    this.attractionService.getAll().subscribe({
      next: (response) => {
        this.attractions = response.attractions || [];
      },
      error: (error) => console.error('Error loading attractions', error),
    });
  }

  loadPendingMaintenance(): void {
    this.maintenanceService.getAllSchedules({ status: 'Pending' }).subscribe({
      next: (schedules) => {
        this.pendingMaintenanceCount = schedules.length;
      },
      error: (error) =>
        console.error('Error loading pending maintenance', error),
    });
  }

  getActiveCount(): number {
    return this.attractions.filter((a) => a.isActive).length;
  }

  getInactiveCount(): number {
    return this.attractions.filter((a) => !a.isActive).length;
  }
}
