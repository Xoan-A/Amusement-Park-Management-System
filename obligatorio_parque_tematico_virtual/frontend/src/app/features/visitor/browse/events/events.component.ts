import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../../shared/components/navbar/navbar.component';
import { EventService } from '../../../../core/services/event.service';
import { EventResponse } from '../../../../core/models';

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    <div class="container mt-4">
      <h1 class="mb-4">Browse Events</h1>

      <div class="row mb-3">
        <div class="col-md-8">
          <input
            type="text"
            class="form-control"
            placeholder="Search events..."
            [(ngModel)]="searchTerm"
            (ngModelChange)="applyFilters()"
          />
        </div>
      </div>

      <div class="row">
        @for (event of filteredEvents; track event.id) {
        <div class="col-md-6 mb-4">
          <div class="card">
            <div class="card-body">
              <h5 class="card-title">{{ event.name }}</h5>
              <p><strong>Date:</strong> {{ event.date | date : 'fullDate' }}</p>
              <p><strong>Time:</strong> {{ event.hour }}:00</p>
              <p><strong>Cost:</strong> {{ '$' + event.cost }}</p>
              <p>
                <strong>Capacity:</strong> {{ event.currentCapacity }} /
                {{ event.maxCapacity }}
              </p>
              <p><strong>Attractions:</strong></p>
              <ul>
                @for (attraction of event.attractions; track attraction.id) {
                <li>{{ attraction.name }}</li>
                }
              </ul>
            </div>
          </div>
        </div>
        }
      </div>
    </div>
  `,
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
