import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { AttractionService } from '../../../core/services/attraction.service';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NavbarComponent],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit, AfterViewInit {
  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;
  dateRangeForm: FormGroup;
  loading = false;
  reportData: any = null;
  chart?: Chart;

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

  ngAfterViewInit(): void {
    this.initChart();
  }

  initChart(): void {
    if (this.chartCanvas) {
      this.chart = new Chart(this.chartCanvas.nativeElement, {
        type: 'bar',
        data: {
          labels: [],
          datasets: [{
            data: [],
            label: 'Total Visits',
            backgroundColor: 'rgba(54, 162, 235, 0.6)'
          }]
        },
        options: {
          responsive: true,
          scales: {
            y: { beginAtZero: true }
          }
        }
      });
    }
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
    if (this.chart && data && data.attractionVisits) {
      this.chart.data.labels = data.attractionVisits.map((v: any) => v.attractionName);
      this.chart.data.datasets[0].data = data.attractionVisits.map((v: any) => v.totalVisits);
      this.chart.update();
    }
  }
}
