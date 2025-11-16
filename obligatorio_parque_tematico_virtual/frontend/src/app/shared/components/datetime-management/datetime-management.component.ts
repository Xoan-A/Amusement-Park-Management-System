import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { DateTimeService } from '../../../core/services/datetime.service';

@Component({
  selector: 'app-datetime-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NavbarComponent],
  templateUrl: './datetime-management.component.html',
  styleUrl: './datetime-management.component.scss'
})
export class DateTimeManagementComponent implements OnInit {
  currentDateTime: Date | null = null;
  dateTimeForm: FormGroup;
  loading: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private dateTimeService: DateTimeService
  ) {
    this.dateTimeForm = this.fb.group({
      date: ['', Validators.required],
      time: ['', [Validators.required, Validators.min(0), Validators.max(23)]]
    });
  }

  ngOnInit(): void {
    this.loadCurrentDateTime();
  }

  loadCurrentDateTime(): void {
    this.loading = true;
    this.dateTimeService.getCurrentDateTime().subscribe({
      next: (response) => {
        this.currentDateTime = new Date(response.currentDateTime);
        this.updateFormWithCurrentDateTime();
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Error loading current date/time';
        this.loading = false;
      }
    });
  }

  updateFormWithCurrentDateTime(): void {
    if (this.currentDateTime) {
      const dateStr = this.currentDateTime.toISOString().split('T')[0];
      const hour = this.currentDateTime.getHours();

      this.dateTimeForm.patchValue({
        date: dateStr,
        time: hour
      });
    }
  }

  onSubmit(): void {
    if (this.dateTimeForm.invalid) {
      return;
    }

    this.loading = true;
    this.successMessage = '';
    this.errorMessage = '';

    const { date, time } = this.dateTimeForm.value;
    const hourStr = String(time).padStart(2, '0');
    const dateTimeString = `${date}T${hourStr}:00:00`;

    this.dateTimeService.setDateTime(dateTimeString).subscribe({
      next: () => {
        this.successMessage = 'Server date/time updated successfully!';
        this.loading = false;
        this.loadCurrentDateTime();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Error updating date/time';
        this.loading = false;
      }
    });
  }

  formatDateTime(date: Date | null): string {
    if (!date) return 'Loading...';
    return date.toLocaleString('en-US', {
      dateStyle: 'full',
      timeStyle: 'medium'
    });
  }
}
