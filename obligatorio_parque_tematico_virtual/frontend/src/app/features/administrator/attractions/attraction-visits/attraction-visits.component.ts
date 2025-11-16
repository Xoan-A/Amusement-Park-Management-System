import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AttractionService } from '../../../../core/services/attraction.service';
import { AttractionsVisitResponse } from '../../../../core/models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-attraction-visits',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './attraction-visits.component.html'
})
export class AttractionVisitsComponent implements OnInit {
  dateRangeForm: FormGroup;
  loading = false;
  reportData: AttractionsVisitResponse | null = null;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private attractionService: AttractionService,
    private toastService: ToastService
  ) {
    this.dateRangeForm = this.fb.group({
      startDate: ['', Validators.required],
      endDate: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadTodaysReport();
  }

  loadTodaysReport(): void {
    const today = new Date();
    const startDate = today.toISOString().split('T')[0];
    const endDate = today.toISOString().split('T')[0];

    this.dateRangeForm.patchValue({
      startDate,
      endDate
    });

    this.loadReport();
  }

  loadReport(): void {
    if (this.dateRangeForm.invalid) {
      this.errorMessage = 'Please select both start and end dates';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const request = this.dateRangeForm.value;

    this.attractionService.getVisitsReport(request).subscribe({
      next: (response) => {
        this.reportData = response;
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Failed to load visits report';
        this.toastService.showError(this.errorMessage);
      }
    });
  }
}
