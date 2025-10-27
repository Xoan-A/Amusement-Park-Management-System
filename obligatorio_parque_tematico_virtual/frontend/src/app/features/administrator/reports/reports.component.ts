import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { AttractionService } from '../../../core/services/attraction.service';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NavbarComponent, BaseChartDirective],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  dateRangeForm: FormGroup;
  loading = false;
  reportData: any = null;

  public barChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [{
      data: [],
      label: 'Total Visits',
      backgroundColor: 'rgba(54, 162, 235, 0.6)'
    }]
  };

  public barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    scales: {
      y: { beginAtZero: true }
    }
  };

  constructor(
    private fb: FormBuilder,
    private attractionService: AttractionService
  ) {
    const today = new Date().toISOString().split('T')[0];
    const weekAgo = new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

    this.dateRangeForm = this.fb.group({
      startDate: [weekAgo],
      endDate: [today]
    });
  }

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.loading = true;
    const { startDate, endDate } = this.dateRangeForm.value;

    this.attractionService.getVisitsReport({ startDate, endDate }).subscribe({
      next: (data) => {
        this.reportData = data;
        this.updateChart(data);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading report', error);
        this.loading = false;
      }
    });
  }

  updateChart(data: any): void {
    if (data && data.attractionVisits) {
      this.barChartData.labels = data.attractionVisits.map((v: any) => v.attractionName);
      this.barChartData.datasets[0].data = data.attractionVisits.map((v: any) => v.totalVisits);
    }
  }
}
