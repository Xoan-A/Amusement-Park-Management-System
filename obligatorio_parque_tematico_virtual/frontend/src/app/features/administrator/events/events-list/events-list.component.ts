import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EventService } from '../../../../core/services/event.service';
import { EventResponse } from '../../../../core/models';

@Component({
  selector: 'app-events-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ],
  templateUrl: './events-list.component.html'
})
export class EventsListComponent implements OnInit {
  events: EventResponse[] = [];
  filteredEvents: EventResponse[] = [];
  loading = true;
  searchTerm = '';

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
    if (confirm('Are you sure you want to delete this event?')) {
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
