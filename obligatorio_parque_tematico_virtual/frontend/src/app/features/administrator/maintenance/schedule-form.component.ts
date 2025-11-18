import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MaintenanceService } from '../../../core/services/maintenance.service';
import { AttractionService } from '../../../core/services/attraction.service';
import { AttractionResponse, AllAttractionsResponse } from '../../../core/models/responses';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-schedule-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ],
  templateUrl: './schedule-form.component.html'
})
export class ScheduleFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private maintenanceService = inject(MaintenanceService);
  private attractionService = inject(AttractionService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  scheduleForm!: FormGroup;
  attractions: AttractionResponse[] = [];
  loading = false;
  errorMessage: string | null = null;

  ngOnInit() {
    this.initForm();
    this.loadAttractions();
  }

  initForm() {
    this.scheduleForm = this.fb.group({
      attractionId: ['', Validators.required],
      scheduledDate: ['', Validators.required],
      estimatedDuration: ['', [Validators.required, Validators.min(1), Validators.max(24)]],
      description: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  loadAttractions() {
    this.attractionService.getAll().subscribe({
      next: (response: AllAttractionsResponse) => {
        this.attractions = response.attractions;
      },
      error: () => {
        this.errorMessage = 'Failed to load attractions.';
      }
    });
  }

  onSubmit() {
    if (this.scheduleForm.invalid) {
      this.scheduleForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    const formValue = this.scheduleForm.value;
    const request = {
      attractionId: formValue.attractionId,
      scheduledDate: formValue.scheduledDate,
      estimatedDuration: parseInt(formValue.estimatedDuration, 10),
      description: formValue.description
    };

    this.maintenanceService.createSchedule(request).subscribe({
      next: (response) => {
        this.loading = false;
        this.toastService.showSuccess(response.message || 'Maintenance scheduled successfully!');
        this.router.navigate(['/admin/maintenance/schedules']);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Failed to schedule maintenance. Please try again.';
      }
    });
  }

  cancel() {
    this.router.navigate(['/admin/maintenance/schedules']);
  }
}
