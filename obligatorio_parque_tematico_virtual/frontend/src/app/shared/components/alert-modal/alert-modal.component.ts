import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-alert-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './alert-modal.component.html',
  styleUrl: './alert-modal.component.scss'
})
export class AlertModalComponent {
  @Input() title: string = 'Alert';
  @Input() message: string = '';
  @Input() show: boolean = false;
  @Input() type: 'error' | 'warning' | 'info' | 'success' = 'info';
  @Input() okText: string = 'OK';

  @Output() ok = new EventEmitter<void>();

  onOk(): void {
    this.ok.emit();
  }

  getIcon(): string {
    switch (this.type) {
      case 'error':
        return 'bi-x-circle-fill text-danger';
      case 'warning':
        return 'bi-exclamation-triangle-fill text-warning';
      case 'success':
        return 'bi-check-circle-fill text-success';
      case 'info':
      default:
        return 'bi-info-circle-fill text-info';
    }
  }
}
