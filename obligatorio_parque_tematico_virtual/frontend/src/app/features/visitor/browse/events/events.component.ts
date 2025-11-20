import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventService } from '../../../../core/services/event.service';
import { EventResponse } from '../../../../core/models';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [CommonModule, FormsModule, ],
  templateUrl: './events.component.html',
  styles: [],
})
export class EventsComponent implements OnInit {
  events: EventResponse[] = [];
  filteredEvents: EventResponse[] = [];
  searchTerm = '';

  constructor(private eventService: EventService) {}

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.eventService.getAll().subscribe({
      next: (events) => {
        this.events = events || [];
        this.applyFilters();
      },
      error: (error) => console.error('Error loading events', error),
    });
  }

  applyFilters(): void {
    this.filteredEvents = this.events.filter(
      (event) =>
        !this.searchTerm ||
        event.name.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }
}
