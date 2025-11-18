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
  templateUrl: './attractions.component.html',
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
