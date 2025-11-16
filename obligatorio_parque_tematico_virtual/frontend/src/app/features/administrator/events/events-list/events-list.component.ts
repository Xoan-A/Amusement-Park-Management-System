import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EventService } from '../../../../core/services/event.service';
import { EventResponse } from '../../../../core/models';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';

@Component({
  selector: 'app-events-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ConfirmationModalComponent],
  templateUrl: './events-list.component.html'
})
export class EventsListComponent implements OnInit {
  events: EventResponse[] = [];
  filteredEvents: EventResponse[] = [];
  loading = true;
  searchTerm = '';
  showDeleteModal = false;
  eventToDelete: string | null = null;

  constructor(private eventService: EventService) {}

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.loading = true;
    this.eventService.getAll().subscribe({
      next: (events) => {
        this.events = events || [];
        this.applyFilters();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading events', error);
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.filteredEvents = this.events.filter(event =>
      !this.searchTerm ||
      event.name.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }

  deleteEvent(id: string): void {
    this.eventToDelete = id;
    this.showDeleteModal = true;
  }

  confirmDelete(): void {
    if (this.eventToDelete) {
      this.eventService.delete(this.eventToDelete).subscribe({
        next: () => {
          this.loadEvents();
          this.eventToDelete = null;
        },
        error: (error) => {
          console.error('Error deleting event', error);
          alert('Failed to delete event');
          this.eventToDelete = null;
        }
      });
    }
  }

  cancelDelete(): void {
    this.eventToDelete = null;
  }
}
