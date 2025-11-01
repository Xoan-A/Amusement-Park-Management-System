import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PluginService } from '../../../core/services/plugin.service';
import { StrategyService } from '../../../core/services/strategy.service';
import { PluginResponse, StrategyResponse } from '../../../core/models/responses';

@Component({
  selector: 'app-plugin-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mt-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Available Scoring Strategy Plugins</h2>
        <button class="btn btn-primary" (click)="navigateToStrategySelection()">
          <i class="bi bi-gear"></i> Change Active Strategy
        </button>
      </div>

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
          <button type="button" class="btn-close" (click)="errorMessage = null"></button>
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
              <div class="card h-100" [class.border-primary]="plugin.name === currentStrategy?.name">
                <div class="card-header d-flex justify-content-between align-items-center">
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
                      <span class="text-muted">{{ plugin.author || 'Unknown' }}</span>
                    </p>
                    <p class="mb-0">
                      <strong>Version:</strong>
                      <span class="text-muted">{{ plugin.version || '1.0.0' }}</span>
                    </p>
                  </div>
                </div>
                <div class="card-footer bg-transparent">
                  @if (plugin.name !== currentStrategy?.name) {
                    <button class="btn btn-outline-primary btn-sm" (click)="activatePlugin(plugin.name)">
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
          <i class="bi bi-exclamation-triangle"></i> No plugins found in the Plugins directory.
          <hr>
          <p class="mb-0">To add new scoring strategy plugins, place .dll files in the <code>/Plugins</code> directory and restart the application.</p>
        </div>
      }

      <!-- Built-in Strategies Info -->
      <div class="card mt-4 bg-light">
        <div class="card-body">
          <h5 class="card-title">
            <i class="bi bi-info-circle"></i> About Scoring Strategy Plugins
          </h5>
          <p class="card-text">
            The system supports dynamic loading of scoring strategies from external DLL files.
            Third-party developers can create custom strategies by implementing the <code>IConcreteStrategy</code> interface.
          </p>
          <ul class="mb-0">
            <li>Plugins are loaded from the <code>/Plugins</code> directory</li>
            <li>Strategies are discovered automatically using .NET Reflection</li>
            <li>No recompilation needed to add new strategies</li>
            <li>Use the "Change Active Strategy" button to switch between strategies</li>
          </ul>
        </div>
      </div>
    </div>
  `
})
export class PluginListComponent implements OnInit {
  private pluginService = inject(PluginService);
  private strategyService = inject(StrategyService);
  private router = inject(Router);

  plugins: PluginResponse[] = [];
  currentStrategy: StrategyResponse | null = null;
  loading = false;
  errorMessage: string | null = null;

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
      }
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
      }
    });
  }

  activatePlugin(pluginName: string) {
    if (!confirm(`Are you sure you want to activate the "${pluginName}" strategy?`)) return;

    this.strategyService.setStrategy({ strategyName: pluginName }).subscribe({
      next: () => {
        alert(`Strategy "${pluginName}" activated successfully!`);
        this.loadCurrentStrategy();
      },
      error: () => {
        this.errorMessage = `Failed to activate strategy "${pluginName}".`;
      }
    });
  }

  navigateToStrategySelection() {
    this.router.navigate(['/admin/strategy']);
  }
}
