import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AttractionService } from '../../../../core/services/attraction.service';
import { AttractionResponse, AttractionType } from '../../../../core/models';
import { EnumToDisplayPipe } from '../../../../shared/pipes/enum-to-display.pipe';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';

@Component({
  selector: 'app-attractions-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EnumToDisplayPipe, ConfirmationModalComponent],
  templateUrl: './attractions-list.component.html'
})
export class AttractionsListComponent implements OnInit {
  attractions: AttractionResponse[] = [];
  filteredAttractions: AttractionResponse[] = [];
  loading = true;
  searchTerm = '';
  selectedType: string = '';
  attractionTypes = Object.values(AttractionType);
  showDeleteModal = false;
  attractionToDelete: string | null = null;

  constructor(private attractionService: AttractionService) {}

  ngOnInit(): void {
    this.loadAttractions();
  }

  loadAttractions(): void {
    this.loading = true;
    this.attractionService.getAll().subscribe({
      next: (response) => {
        this.attractions = response.attractions || [];
        this.applyFilters();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading attractions', error);
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.filteredAttractions = this.attractions.filter(attraction => {
      const matchesSearch = !this.searchTerm ||
        attraction.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        attraction.description.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesType = !this.selectedType || attraction.type === this.selectedType;

      return matchesSearch && matchesType;
    });
  }

  deleteAttraction(id: string): void {
    this.attractionToDelete = id;
    this.showDeleteModal = true;
  }

  confirmDelete(): void {
    if (this.attractionToDelete) {
      this.attractionService.delete(this.attractionToDelete).subscribe({
        next: () => {
          this.loadAttractions();
          this.attractionToDelete = null;
        },
        error: (error) => {
          console.error('Error deleting attraction', error);
          alert('Failed to delete attraction');
          this.attractionToDelete = null;
        }
      });
    }
  }

  cancelDelete(): void {
    this.attractionToDelete = null;
  }
}
