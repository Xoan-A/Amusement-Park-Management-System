import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttractionService } from '../../../../core/services/attraction.service';
import { AttractionResponse } from '../../../../core/models';
import { EnumToDisplayPipe } from '../../../../shared/pipes/enum-to-display.pipe';

@Component({
  selector: 'app-attractions',
  standalone: true,
  imports: [CommonModule, FormsModule,  EnumToDisplayPipe],
  template: `
    <div class="container mt-4">
      <h1 class="mb-4">Browse Attractions</h1>

      <div class="row mb-3">
        <div class="col-md-8">
          <input
            type="text"
            class="form-control"
            placeholder="Search attractions..."
            [(ngModel)]="searchTerm"
            (ngModelChange)="applyFilters()"
          >
        </div>
      </div>

      <div class="row">
        @for (attraction of filteredAttractions; track attraction.id) {
          <div class="col-md-6 col-lg-4 mb-4">
            <div class="card h-100">
              <div class="card-body">
                <h5 class="card-title">{{ attraction.name }}</h5>
                <h6 class="card-subtitle mb-2 text-muted">{{ attraction.type | enumToDisplay }}</h6>
                <p class="card-text">{{ attraction.description }}</p>
                <ul class="list-unstyled">
                  <li><strong>Min Age:</strong> {{ attraction.minAge }} years</li>
                  <li><strong>Capacity:</strong> {{ attraction.currentCapacity }} / {{ attraction.maxCapacity }}</li>
                  <li>
                    <strong>Status:</strong>
                    @if (attraction.isActive) {
                      <span class="badge bg-success">Active</span>
                    } @else {
                      <span class="badge bg-danger">Inactive</span>
                    }
                  </li>
                </ul>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: []
})
export class AttractionsComponent implements OnInit {
  attractions: AttractionResponse[] = [];
  filteredAttractions: AttractionResponse[] = [];
  searchTerm = '';

  constructor(private attractionService: AttractionService) {}

  ngOnInit(): void {
    this.loadAttractions();
  }

  loadAttractions(): void {
    this.attractionService.getAll().subscribe({
      next: (response) => {
        this.attractions = response.attractions || [];
        this.applyFilters();
      },
      error: (error) => console.error('Error loading attractions', error)
    });
  }

  applyFilters(): void {
    this.filteredAttractions = this.attractions.filter(attraction =>
      !this.searchTerm ||
      attraction.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
      attraction.description.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }
}
