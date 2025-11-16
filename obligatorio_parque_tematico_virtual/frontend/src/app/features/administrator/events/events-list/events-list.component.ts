import { Component, OnInit } from '@angular/core';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { CommonModule } from '@angular/common';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { RouterLink } from '@angular/router';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { FormsModule } from '@angular/forms';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { EventService } from '../../../../core/services/event.service';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { EventResponse } from '../../../../core/models';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';

@Component({
  selector: 'app-events-list',
  standalone: true,
  imports: [ConfirmationModalComponent, CommonModule, RouterLink, FormsModule, ],
  templateUrl: './events-list.component.html'
})
export class EventsListComponent implements OnInit {
  events: EventResponse[] = [];
  filteredEvents: EventResponse[] = [];
  loading = true;
  showDeleteModal = false;
  itemToDelete: any = null;
  searchTerm = '';

  constructor(private eventService: EventService) {}

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.loading = true;
  showDeleteModal = false;
  itemToDelete: any = null;
    this.eventService.getAll().subscribe({
      next: (events) => {
        this.events = events || [];
        this.applyFilters();
        this.loading = false;
  showDeleteModal = false;
  itemToDelete: any = null;
      },
      error: (error) => {
        console.error('Error loading events', error);
        this.loading = false;
  showDeleteModal = false;
  itemToDelete: any = null;
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
  }

  confirmDelete(): void {
    if (this.itemToDelete) {
      this.eventService.delete(this.itemToDelete).subscribe({
        next: () => { this.loadEvents(); this.itemToDelete = null; },
        error: (error) => { console.error("Error", error); this.itemToDelete = null; }
      });
    }
  }

  cancelDelete(): void {
    this.itemToDelete = null;
    this.itemToDelete = id; this.showDeleteModal = true;
      this.eventService.delete(id).subscribe({
        next: () => {
          this.loadEvents();
        },
        error: (error) => {
          console.error('Error deleting event', error);
          alert('Failed to delete event');
        }
      });
    }
  }
}
