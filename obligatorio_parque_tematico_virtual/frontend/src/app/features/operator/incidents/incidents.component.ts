import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttractionService } from '../../../core/services/attraction.service';
import { IncidentService } from '../../../core/services/incident.service';
import { AttractionResponse } from '../../../core/models';

@Component({
  selector: 'app-incidents',
  standalone: true,
  imports: [CommonModule, FormsModule, ],
  template: `
    <app-navbar></app-navbar>
    <div class="container mt-4">
      <h1 class="mb-4">Incident Management</h1>

      @if (successMessage) {
        <div class="alert alert-success">{{ successMessage }}</div>
      }
      @if (errorMessage) {
        <div class="alert alert-danger">{{ errorMessage }}</div>
      }

      <div class="row">
        @for (attraction of attractions; track attraction.id) {
          <div class="col-md-6 mb-3">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">
                  {{ attraction.name }}
                  @if (!attraction.isActive) {
                    <span class="badge bg-danger ms-2">INACTIVE</span>
                  }
                </h5>
              </div>
              <div class="card-body">
                @if (attraction.incidents && attraction.incidents.length > 0) {
                  <ul class="list-group mb-3">
                    @for (incident of attraction.incidents; track incident) {
                      <li class="list-group-item d-flex justify-content-between align-items-center">
                        {{ incident }}
                        <button
                          class="btn btn-sm btn-outline-danger"
                          (click)="removeIncident(attraction.id, incident)"
                        >
                          Remove
                        </button>
                      </li>
                    }
                  </ul>
                } @else {
                  <p class="text-muted">No incidents reported</p>
                }

                <div class="input-group">
                  <input
                    type="text"
                    class="form-control"
                    [(ngModel)]="newIncidents[attraction.id]"
                    placeholder="Report new incident..."
                  >
                  <button
                    class="btn btn-primary"
                    (click)="addIncident(attraction.id)"
                    [disabled]="!newIncidents[attraction.id]"
                  >
                    Add
                  </button>
                </div>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: []
})
export class IncidentsComponent implements OnInit {
  attractions: AttractionResponse[] = [];
  newIncidents: { [key: string]: string } = {};
  successMessage = '';
  errorMessage = '';

  constructor(
    private attractionService: AttractionService,
    private incidentService: IncidentService
  ) {}

  ngOnInit(): void {
    this.loadAttractions();
  }

  loadAttractions(): void {
    this.attractionService.getAll().subscribe({
      next: (response) => {
        this.attractions = response.attractions || [];
      },
      error: (error) => console.error('Error loading attractions', error)
    });
  }

  addIncident(attractionId: string): void {
    const incident = this.newIncidents[attractionId];
    if (!incident) return;

    this.incidentService.addIncident(attractionId, { incident }).subscribe({
      next: () => {
        this.successMessage = 'Incident added successfully!';
        this.newIncidents[attractionId] = '';
        this.loadAttractions();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to add incident';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  removeIncident(attractionId: string, incident: string): void {
    this.incidentService.removeIncident(attractionId, { incident }).subscribe({
      next: () => {
        this.successMessage = 'Incident removed successfully!';
        this.loadAttractions();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to remove incident';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }
}
