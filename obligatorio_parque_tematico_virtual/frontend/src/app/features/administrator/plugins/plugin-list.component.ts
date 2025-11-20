import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PluginService } from '../../../core/services/plugin.service';
import { StrategyService } from '../../../core/services/strategy.service';
import { PluginResponse, StrategyResponse } from '../../../core/models/responses';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-plugin-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './plugin-list.component.html',
  })
export class PluginListComponent implements OnInit {
  private pluginService = inject(PluginService);
  private strategyService = inject(StrategyService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  plugins: PluginResponse[] = [];
  currentStrategy: StrategyResponse | null = null;
  loading = false;
  showDeleteModal = false;
  itemToDelete: PluginResponse | null = null;
  errorMessage: string | null = null;
  selectedFile: File | null = null;
  uploading = false;
  comboNValue: number | null = null;
  comboNValueError: string | null = null;

  ngOnInit() {
    this.loadCurrentStrategy();
    this.loadPlugins();
  }

  loadCurrentStrategy() {
    this.strategyService.getCurrent().subscribe({
      next: (strategy: StrategyResponse) => {
        this.currentStrategy = strategy;
      },
      error: () => {},
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
    this.errorMessage = null;
    this.comboNValueError = null;

    if (pluginName.toLowerCase() === 'combo') {
      if (!this.comboNValue || this.comboNValue <= 0) {
        this.comboNValueError =
          'Please enter a valid N value (positive number).';
        return;
      }

      const n = this.comboNValue;

      this.strategyService
        .setStrategy({ strategyName: pluginName, n })
        .subscribe({
          next: () => {
            this.toastService.showSuccess(`Strategy "${pluginName}" activated successfully with N=${n} minutes!`);
            this.loadCurrentStrategy();
            this.comboNValue = null;
          },
          error: () => {
            this.errorMessage = `Failed to activate strategy "${pluginName}".`;
          },
        });
    } else {
      this.strategyService.setStrategy({ strategyName: pluginName }).subscribe({
        next: () => {
          this.toastService.showSuccess(`Strategy "${pluginName}" activated successfully!`);
          this.loadCurrentStrategy();
        },
        error: () => {
          this.errorMessage = `Failed to activate strategy "${pluginName}".`;
        },
      });
    }
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
    }
  }

  uploadStrategy() {
    if (!this.selectedFile) {
      this.errorMessage = 'Please select a file first';
      return;
    }

    this.uploading = true;
    this.errorMessage = null;

    this.pluginService.uploadPlugin(this.selectedFile).subscribe({
      next: () => {
        this.toastService.showSuccess(`Strategy "${
          this.selectedFile!.name
        }" uploaded successfully!`);
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
  }
}
