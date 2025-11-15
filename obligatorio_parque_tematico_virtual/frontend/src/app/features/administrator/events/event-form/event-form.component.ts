import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NavbarComponent } from '../../../../shared/components/navbar/navbar.component';
import { EventService } from '../../../../core/services/event.service';
import { AttractionService } from '../../../../core/services/attraction.service';
import { AttractionResponse } from '../../../../core/models';

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, NavbarComponent],
  templateUrl: './event-form.component.html'
})
export class EventFormComponent implements OnInit {
  eventForm: FormGroup;
  loading = false;
  attractions: AttractionResponse[] = [];
  selectedAttractions: Set<string> = new Set();
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private eventService: EventService,
    private attractionService: AttractionService,
    private router: Router
  ) {
    this.eventForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      date: ['', Validators.required],
      hour: [0, [Validators.required, Validators.min(0), Validators.max(23)]],
      maxCapacity: [1, [Validators.required, Validators.min(1)]],
      cost: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadAttractions();
  }

  loadAttractions(): void {
    this.attractionService.getAll().subscribe({
      next: (response) => {
        this.attractions = response.attractions || [];
      },
      error: (error) => {
        console.error('Error loading attractions', error);
      }
    });
  }

  toggleAttraction(attractionId: string): void {
    if (this.selectedAttractions.has(attractionId)) {
      this.selectedAttractions.delete(attractionId);
    } else {
      this.selectedAttractions.add(attractionId);
    }
  }

  isAttractionSelected(attractionId: string): boolean {
    return this.selectedAttractions.has(attractionId);
  }

  onSubmit(): void {
    if (this.eventForm.invalid) {
      this.markFormGroupTouched(this.eventForm);
      return;
    }

    if (this.selectedAttractions.size === 0) {
      this.errorMessage = 'Please select at least one attraction';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const eventData = {
      ...this.eventForm.value,
      attractionIds: Array.from(this.selectedAttractions)
    };

    this.eventService.create(eventData).subscribe({
      next: () => {
        this.successMessage = 'Event created successfully!';
        setTimeout(() => this.router.navigate(['/admin/events']), 1500);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Failed to create event';
      }
    });
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }
}
