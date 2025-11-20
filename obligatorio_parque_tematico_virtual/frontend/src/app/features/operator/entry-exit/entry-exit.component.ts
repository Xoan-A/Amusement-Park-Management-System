import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ZXingScannerModule } from '@zxing/ngx-scanner';
import { AttractionService } from '../../../core/services/attraction.service';
import { TicketService } from '../../../core/services/ticket.service';
import { EventService } from '../../../core/services/event.service';
import { AttractionResponse, EventResponse, TicketType } from '../../../core/models';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-entry-exit',
  standalone: true,
  imports: [CommonModule, FormsModule, ZXingScannerModule],
  templateUrl: './entry-exit.component.html',
  styles: [
    `
      .scanner-container {
        border: 2px solid #dee2e6;
        border-radius: 0.375rem;
        overflow: hidden;
      }
    `,
  ],
})
export class EntryExitComponent implements OnInit {
  attractions: AttractionResponse[] = [];
  events: EventResponse[] = [];
  errorMessage = '';
  loading = false;
  ticketType = TicketType;

  entryInputMethod: 'manual' | 'camera' = 'manual';

  entryForm = {
    qrCode: '',
    nfc: '',
    attractionId: '',
    eventId: '',
  };

  exitForm = {
    userId: '',
    attractionId: '',
  };

  availableDevices: MediaDeviceInfo[] = [];
  selectedDevice?: MediaDeviceInfo;
  hasDevices?: boolean;
  hasPermission?: boolean;

  constructor(
    private attractionService: AttractionService,
    private ticketService: TicketService,
    private eventService: EventService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadAttractions();
    this.loadEvents();
  }

  loadAttractions(): void {
    this.attractionService.getAll().subscribe({
      next: (response) => {
        this.attractions = response.attractions || [];
      },
      error: () => {
        console.error('Error loading attractions');
        this.errorMessage = 'Failed to load attractions';
      },
    });
  }

  loadEvents(): void {
    this.eventService.getAll().subscribe({
      next: (events) => {
        this.events = events || [];
      },
      error: () => {
        console.error('Error loading events');
        this.errorMessage = 'Failed to load events';
      },
    });
  }

  setEntryInputMethod(method: 'manual' | 'camera'): void {
    this.entryInputMethod = method;
  }

  onCamerasFound(devices: MediaDeviceInfo[]): void {
    this.availableDevices = devices;
    this.hasDevices = devices && devices.length > 0;
    if (this.hasDevices) {
      this.selectedDevice = devices[0];
    }
  }

  onDeviceSelectChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const deviceId = target.value;
    const device = this.availableDevices.find((d) => d.deviceId === deviceId);
    if (device) {
      this.selectedDevice = device;
    }
  }

  onPermissionResponse(hasPermission: boolean): void {
    this.hasPermission = hasPermission;
    if (!hasPermission) {
      this.errorMessage = 'Camera permission denied. Please use manual input.';
    }
  }

  onEntryScanSuccess(qrCodeData: string): void {
    this.loading = true;
    this.errorMessage = '';

    this.ticketService.getByQrCode(qrCodeData).subscribe({
      next: (ticket) => {
        this.entryForm.qrCode = ticket.qrCode;
        if (ticket.eventId) {
          this.entryForm.eventId = ticket.eventId;
        }
        this.loading = false;
        this.toastService.showSuccess(
          'QR code scanned successfully! Fields populated.'
        );
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Invalid QR code or ticket not found';
      },
    });
  }

  registerEntry(): void {
    if ((!this.entryForm.qrCode && !this.entryForm.nfc) || !this.entryForm.attractionId) return;

    this.loading = true;
    this.errorMessage = '';

    const request: { Qr?: string; NFC?: string; EventId?: string } = {};

    if (this.entryForm.qrCode) {
      request.Qr = this.entryForm.qrCode;
    }

    if (this.entryForm.nfc) {
      request.NFC = this.entryForm.nfc;
    }

    if (this.entryForm.eventId) {
      request.EventId = this.entryForm.eventId.toString();
    }

    this.attractionService
      .registerEntry(this.entryForm.attractionId, request)
      .subscribe({
        next: () => {
          this.toastService.showSuccess(
            'Visitor entry registered successfully!'
          );
          this.loading = false;
          this.loadAttractions();
          this.resetEntryForm();
        },
        error: (error) => {
          this.errorMessage =
            error.error?.message ||
            error.error?.Message ||
            'Failed to register entry';
          this.loading = false;
        },
      });
  }

  registerExit(): void {
    if (!this.exitForm.userId || !this.exitForm.attractionId) return;

    this.loading = true;
    this.errorMessage = '';

    const request = {
      userId: this.exitForm.userId,
    };

    this.attractionService
      .registerExit(this.exitForm.attractionId, request)
      .subscribe({
        next: () => {
          this.toastService.showSuccess(
            'Visitor exit registered successfully!'
          );
          this.loading = false;
          this.loadAttractions();
          this.resetExitForm();
        },
        error: (error) => {
          this.errorMessage =
            error.error?.message ||
            error.error?.Message ||
            'Failed to register exit';
          this.loading = false;
        },
      });
  }

  resetEntryForm(): void {
    this.entryForm = {
      qrCode: '',
      nfc: '',
      attractionId: '',
      eventId: '',
    };
  }

  resetExitForm(): void {
    this.exitForm = {
      userId: '',
      attractionId: '',
    };
  }
}
