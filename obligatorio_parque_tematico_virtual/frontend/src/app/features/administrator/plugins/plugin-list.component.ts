import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { PluginService } from '../../../core/services/plugin.service';
import { StrategyService } from '../../../core/services/strategy.service';
import {
  PluginResponse,
  StrategyResponse,
} from '../../../core/models/responses';

@Component({
  selector: 'app-plugin-list',
  standalone: true,
  imports: [CommonModule, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Available Scoring Strategy Plugins</h2>
        <button class="btn btn-primary" (click)="navigateToStrategySelection()">
          <i class="bi bi-gear"></i> Change Active Strategy
        </button>
      </div>

      <!-- Upload Strategy Section -->
      <div class="card mb-4">
        <div class="card-header bg-primary text-white">
          <h5 class="mb-0">
            <i class="bi bi-cloud-upload"></i> Upload New Strategy DLL
          </h5>
        </div>
        <div class="card-body">
          <div class="row align-items-end">
            <div class="col-md-8">
              <label for="fileInput" class="form-label"
                >Select Strategy DLL File</label
              >
              <input
                type="file"
                class="form-control"
                id="fileInput"
                accept=".dll"
                (change)="onFileSelected($event)"
                [disabled]="uploading"
              />
              @if (selectedFile) {
              <small class="text-muted d-block mt-2">
                <i class="bi bi-file-earmark-code"></i> Selected:
                {{ selectedFile.name }} ({{
                  (selectedFile.size / 1024).toFixed(2)
                }}
                KB)
              </small>
              }
            </div>
            <div class="col-md-4">
              <div class="d-flex gap-2">
                <button
                  class="btn btn-success flex-grow-1"
                  (click)="uploadStrategy()"
                  [disabled]="!selectedFile || uploading"
                >
                  @if (uploading) {
                  <span class="spinner-border spinner-border-sm me-2"></span>
                  Uploading... } @else {
                  <i class="bi bi-upload"></i> Upload }
                </button>
                @if (selectedFile && !uploading) {
                <button class="btn btn-outline-secondary" (click)="clearFile()">
                  <i class="bi bi-x"></i>
                </button>
                }
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Success Message -->
      @if (successMessage) {
      <div class="alert alert-success alert-dismissible fade show" role="alert">
        <i class="bi bi-check-circle-fill"></i> {{ successMessage }}
        <button
          type="button"
          class="btn-close"
          (click)="successMessage = null"
        ></button>
      </div>
      }

      <!-- Current Strategy Info -->
      @if (currentStrategy) {
      <div class="alert alert-info">
        <i class="bi bi-check-circle-fill"></i>
        <strong>Currently Active Strategy:</strong> {{ currentStrategy.name }}
      </div>
      }

      <!-- Error Message -->
      @if (errorMessage) {
      <div class="alert alert-danger alert-dismissible fade show" role="alert">
        {{ errorMessage }}
        <button
          type="button"
          class="btn-close"
          (click)="errorMessage = null"
        ></button>
      </div>
      }

      <!-- Loading -->
      @if (loading) {
      <div class="text-center my-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>
      }

      <!-- Plugins Grid -->
      @if (!loading && plugins.length > 0) {
      <div class="row">
        @for (plugin of plugins; track plugin.name) {
        <div class="col-md-6 mb-4">
          <div
            class="card h-100"
            [class.border-primary]="plugin.name === currentStrategy?.name"
          >
            <div
              class="card-header d-flex justify-content-between align-items-center"
            >
              <h5 class="mb-0">{{ plugin.name }}</h5>
              @if (plugin.name === currentStrategy?.name) {
              <span class="badge bg-success">Active</span>
              }
            </div>
            <div class="card-body">
              <p class="card-text">{{ plugin.description }}</p>

              <div class="mt-3">
                <p class="mb-1">
                  <strong>Author:</strong>
                  <span class="text-muted">{{
                    plugin.author || 'Unknown'
                  }}</span>
                </p>
                <p class="mb-0">
                  <strong>Version:</strong>
                  <span class="text-muted">{{
                    plugin.version || '1.0.0'
                  }}</span>
                </p>
              </div>
            </div>
            <div class="card-footer bg-transparent">
              @if (plugin.name !== currentStrategy?.name) {
              <button
                class="btn btn-outline-primary btn-sm"
                (click)="activatePlugin(plugin.name)"
              >
                <i class="bi bi-check-circle"></i> Activate This Strategy
              </button>
              } @else {
              <button class="btn btn-success btn-sm" disabled>
                <i class="bi bi-check-circle-fill"></i> Currently Active
              </button>
              }
            </div>
          </div>
        </div>
        }
      </div>
      }

      <!-- No Results -->
      @if (!loading && plugins.length === 0) {
      <div class="alert alert-warning">
        <i class="bi bi-exclamation-triangle"></i> No plugins found in the
        Plugins directory.
        <hr />
        <p class="mb-0">
          To add new scoring strategy plugins, place .dll files in the
          <code>/Plugins</code> directory and restart the application.
        </p>
      </div>
      }

      <!-- Built-in Strategies Info -->
      <div class="card mt-4 bg-light">
        <div class="card-body">
          <h5 class="card-title">
            <i class="bi bi-info-circle"></i> About Scoring Strategy Plugins
          </h5>
          <p class="card-text">
            The system supports dynamic loading of scoring strategies from
            external DLL files. Third-party developers can create custom
            strategies by implementing the
            <code>IConcreteStrategy</code> interface.
          </p>
          <ul class="mb-0">
            <li>Plugins are loaded from the <code>/Plugins</code> directory</li>
            <li>
              Strategies are discovered automatically using .NET Reflection
            </li>
            <li>No recompilation needed to add new strategies</li>
            <li>
              Use the "Change Active Strategy" button to switch between
              strategies
            </li>
          </ul>
        </div>
      </div>
    </div>
  `,
})
export class PluginListComponent implements OnInit {
  private pluginService = inject(PluginService);
  private strategyService = inject(StrategyService);
  private router = inject(Router);

  plugins: PluginResponse[] = [];
  currentStrategy: StrategyResponse | null = null;
  loading = false;
  errorMessage: string | null = null;
  selectedFile: File | null = null;
  uploading = false;
  successMessage: string | null = null;

  ngOnInit() {
    this.loadCurrentStrategy();
    this.loadPlugins();
  }

  loadCurrentStrategy() {
    this.strategyService.getCurrent().subscribe({
      next: (strategy: StrategyResponse) => {
        this.currentStrategy = strategy;
      },
      error: () => {
        // Strategy might not be set yet, that's okay
      },
    });
  }

  loadPlugins() {
    this.loading = true;
    this.errorMessage = null;

    this.pluginService.getAvailablePlugins().subscribe({
      next: (plugins) => {
        this.plugins = plugins;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load available plugins.';
        this.loading = false;
      },
    });
  }

  activatePlugin(pluginName: string) {
    if (
      !confirm(
        `Are you sure you want to activate the "${pluginName}" strategy?`
      )
    )
      return;

    this.strategyService.setStrategy({ strategyName: pluginName }).subscribe({
      next: () => {
        alert(`Strategy "${pluginName}" activated successfully!`);
        this.loadCurrentStrategy();
      },
      error: () => {
        this.errorMessage = `Failed to activate strategy "${pluginName}".`;
      },
    });
  }

  navigateToStrategySelection() {
    this.router.navigate(['/admin/strategy']);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      if (!file.name.toLowerCase().endsWith('.dll')) {
        this.errorMessage = 'Please select a valid .dll file';
        this.selectedFile = null;
        return;
      }

      this.selectedFile = file;
      this.errorMessage = null;
      this.successMessage = null;
    }
  }

  uploadStrategy() {
    if (!this.selectedFile) {
      this.errorMessage = 'Please select a file first';
      return;
    }

    this.uploading = true;
    this.errorMessage = null;
    this.successMessage = null;

    this.pluginService.uploadPlugin(this.selectedFile).subscribe({
      next: () => {
        this.successMessage = `Strategy "${
          this.selectedFile!.name
        }" uploaded successfully!`;
        this.selectedFile = null;
        const fileInput = document.getElementById(
          'fileInput'
        ) as HTMLInputElement;
        if (fileInput) fileInput.value = '';

        this.uploading = false;
        this.loadPlugins();
      },
      error: (error) => {
        this.errorMessage =
          error.error?.message ||
          'Failed to upload strategy. Please try again.';
        this.uploading = false;
      },
    });
  }

  clearFile() {
    this.selectedFile = null;
    const fileInput = document.getElementById('fileInput') as HTMLInputElement;
    if (fileInput) fileInput.value = '';
    this.errorMessage = null;
    this.successMessage = null;
  }
}
