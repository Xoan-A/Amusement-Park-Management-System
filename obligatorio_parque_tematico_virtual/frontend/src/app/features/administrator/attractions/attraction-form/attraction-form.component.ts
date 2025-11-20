import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AttractionService } from '../../../../core/services/attraction.service';
import { AttractionType } from '../../../../core/models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-attraction-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ],
  templateUrl: './attraction-form.component.html'
})
export class AttractionFormComponent implements OnInit {
  attractionForm: FormGroup;
  loading = false;
  isEditMode = false;
  attractionId: string | null = null;
  attractionTypes = Object.values(AttractionType);
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private attractionService: AttractionService,
    private router: Router,
    private route: ActivatedRoute,
    private toastService: ToastService
  ) {
    this.attractionForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      type: [AttractionType.RollerCoaster, Validators.required],
      minAge: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
      maxCapacity: [1, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.attractionId = this.route.snapshot.paramMap.get('id');
    if (this.attractionId) {
      this.isEditMode = true;
      this.loadAttraction();
    }
  }

  loadAttraction(): void {
    if (!this.attractionId) return;

    this.loading = true;
    this.attractionService.getById(this.attractionId).subscribe({
      next: (attraction) => {
        this.attractionForm.patchValue({
          name: attraction.name,
          description: attraction.description,
          type: attraction.type,
          minAge: attraction.minAge,
          maxCapacity: attraction.maxCapacity
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading attraction', error);
        this.errorMessage = 'Failed to load attraction';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.attractionForm.invalid) {
      this.markFormGroupTouched(this.attractionForm);
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const attractionData = this.attractionForm.value;

    if (this.isEditMode && this.attractionId) {
      this.attractionService.update(this.attractionId, attractionData).subscribe({
        next: () => {
          this.toastService.showSuccess('Attraction updated successfully!');
          setTimeout(() => this.router.navigate(['/admin/attractions']), 1500);
        },
        error: (error) => {
          this.loading = false;
          this.errorMessage = error.error?.message || 'Failed to update attraction';
        }
      });
    } else {
      this.attractionService.create(attractionData).subscribe({
        next: () => {
          this.toastService.showSuccess('Attraction created successfully!');
          setTimeout(() => this.router.navigate(['/admin/attractions']), 1500);
        },
        error: (error) => {
          this.loading = false;
          this.errorMessage = error.error?.message || 'Failed to create attraction';
        }
      });
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }
}
