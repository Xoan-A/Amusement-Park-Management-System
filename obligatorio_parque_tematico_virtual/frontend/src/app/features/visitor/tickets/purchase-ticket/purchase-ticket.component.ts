import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NavbarComponent } from '../../../../shared/components/navbar/navbar.component';
import { TicketService } from '../../../../core/services/ticket.service';
import { EventService } from '../../../../core/services/event.service';
import { AuthService } from '../../../../core/services/auth.service';
import { EventResponse, TicketType } from '../../../../core/models';

@Component({
  selector: 'app-purchase-ticket',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NavbarComponent],
  templateUrl: './purchase-ticket.component.html',
  styleUrl: './purchase-ticket.component.scss'
})
export class PurchaseTicketComponent implements OnInit {
  ticketForm: FormGroup;
  events: EventResponse[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private ticketService: TicketService,
    private eventService: EventService,
    private authService: AuthService,
    private router: Router
  ) {
    this.ticketForm = this.fb.group({
      visitDate: ['', Validators.required],
      type: [TicketType.General, Validators.required],
      eventId: ['']
    });
  }

  ngOnInit(): void {
    this.loadEvents();

    // Watch type changes
    this.ticketForm.get('type')?.valueChanges.subscribe(type => {
      const eventControl = this.ticketForm.get('eventId');
      if (type === TicketType.EventSpecial) {
        eventControl?.setValidators(Validators.required);
      } else {
        eventControl?.clearValidators();
        eventControl?.setValue('');
      }
      eventControl?.updateValueAndValidity();
    });
  }

  loadEvents(): void {
    this.eventService.getAll().subscribe({
      next: (events) => {
        this.events = events || [];
      },
      error: (error) => console.error('Error loading events', error)
    });
  }

  purchaseTicket(): void {
    if (this.ticketForm.invalid) return;

    const userId = this.authService.getUserId();
    if (!userId) {
      this.errorMessage = 'Please log in to purchase tickets';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const ticketData = {
      visitorId: userId,
      visitDate: this.ticketForm.value.visitDate,
      type: parseInt(this.ticketForm.value.type),
      eventId: this.ticketForm.value.eventId || undefined
    };

    this.ticketService.purchase(ticketData).subscribe({
      next: () => {
        this.successMessage = 'Ticket purchased successfully!';
        setTimeout(() => this.router.navigate(['/visitor/tickets']), 2000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to purchase ticket';
        this.loading = false;
      }
    });
  }
}
