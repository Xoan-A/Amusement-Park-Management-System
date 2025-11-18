import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttractionService } from '../../../core/services/attraction.service';
import { IncidentService } from '../../../core/services/incident.service';
import { AttractionResponse } from '../../../core/models';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-incidents',
  standalone: true,
  imports: [CommonModule, FormsModule, ],
  templateUrl: './incidents.component.html',
  styles: []
})
export class IncidentsComponent implements OnInit {
  attractions: AttractionResponse[] = [];
  newIncidents: { [key: string]: string } = {};
  errorMessage: string | null = null;

  constructor(
    private attractionService: AttractionService,
    private incidentService: IncidentService,
    private toastService: ToastService
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
        this.toastService.showSuccess('Incident added successfully!');
        this.newIncidents[attractionId] = '';
        this.loadAttractions();
        this.errorMessage = null;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to add incident';
      }
    });
  }

  removeIncident(attractionId: string, incident: string): void {
    this.incidentService.removeIncident(attractionId, { incident }).subscribe({
      next: () => {
        this.toastService.showSuccess('Incident removed successfully!');
        this.loadAttractions();
        this.errorMessage = null;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to remove incident';
      }
    });
  }
}
