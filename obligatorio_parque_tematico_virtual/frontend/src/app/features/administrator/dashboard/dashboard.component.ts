import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { AttractionService } from '../../../core/services/attraction.service';
import { EventService } from '../../../core/services/event.service';
import { StrategyService } from '../../../core/services/strategy.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  totalAttractions = 0;
  totalEvents = 0;
  currentStrategy = '';
  loading = true;

  constructor(
    private attractionService: AttractionService,
    private eventService: EventService,
    private strategyService: StrategyService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;

    this.attractionService.getAll().subscribe({
      next: (response) => {
        this.totalAttractions = response.attractions?.length || 0;
      },
      error: (error) => console.error('Error loading attractions', error)
    });

    this.eventService.getAll().subscribe({
      next: (events) => {
        this.totalEvents = events?.length || 0;
      },
      error: (error) => console.error('Error loading events', error)
    });

    this.strategyService.getCurrent().subscribe({
      next: (strategy) => {
        this.currentStrategy = strategy.name;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading strategy', error);
        this.loading = false;
      }
    });
  }
}
