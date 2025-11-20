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
  templateUrl: './dashboard.component.html',
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
