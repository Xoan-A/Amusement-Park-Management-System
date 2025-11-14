import { Component, OnInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { AttractionService } from '../../../core/services/attraction.service';
import { TicketService } from '../../../core/services/ticket.service';
import { AttractionResponse, TicketResponse } from '../../../core/models';
import { BrowserMultiFormatReader, IScannerControls } from '@zxing/browser';

@Component({
  selector: 'app-entry-exit',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    <div class="container mt-4">
      <h1 class="mb-4">Visitor Entry/Exit Management</h1>

      @if (successMessage) {
        <div class="alert alert-success alert-dismissible">
          {{ successMessage }}
          <button type="button" class="btn-close" (click)="successMessage=''"></button>
        </div>
      }
      @if (errorMessage) {
        <div class="alert alert-danger alert-dismissible">
          {{ errorMessage }}
          <button type="button" class="btn-close" (click)="errorMessage=''"></button>
        </div>
      }

      <div class="card mb-4">
        <div class="card-header">
          <h5 class="mb-0">Scan or Enter Ticket</h5>
        </div>
        <div class="card-body">
          <div class="mb-3">
            <label class="form-label">Input Method</label>
            <div class="btn-group w-100" role="group">
              <button
                type="button"
                class="btn"
                [class.btn-primary]="inputMethod === 'manual'"
                [class.btn-outline-primary]="inputMethod !== 'manual'"
                (click)="setInputMethod('manual')"
              >
                Manual Input
              </button>
              <button
                type="button"
                class="btn"
                [class.btn-primary]="inputMethod === 'camera'"
                [class.btn-outline-primary]="inputMethod !== 'camera'"
                (click)="setInputMethod('camera')"
              >
                QR Scanner
              </button>
            </div>
          </div>

          @if (inputMethod === 'manual') {
            <div class="mb-3">
              <label for="ticketId" class="form-label">Ticket ID</label>
              <input
                type="text"
                class="form-control"
                id="ticketId"
                [(ngModel)]="manualTicketId"
                placeholder="Enter ticket ID"
              >
            </div>
            <button
              class="btn btn-primary"
              (click)="lookupTicket()"
              [disabled]="!manualTicketId || loading"
            >
              {{ loading ? 'Looking up...' : 'Lookup Ticket' }}
            </button>
          }

          @if (inputMethod === 'camera') {
            <div class="mb-3">
              @if (availableDevices.length > 1) {
                <label class="form-label">Select Camera</label>
                <select
                  class="form-select mb-3"
                  (change)="onDeviceSelectChange($event)"
                >
                  @for (device of availableDevices; track device.deviceId) {
                    <option [value]="device.deviceId" [selected]="selectedDevice?.deviceId === device.deviceId">
                      {{ device.label || 'Camera ' + ($index + 1) }}
                    </option>
                  }
                </select>
              }

              <div class="scanner-container" style="max-width: 500px; margin: 0 auto;">
                <video #videoElement style="width: 100%; border: 2px solid #ccc; border-radius: 8px;"></video>
              </div>

              @if (hasDevices === false) {
                <div class="alert alert-warning mt-3">
                  No camera devices found. Please use manual input or check camera permissions.
                </div>
              }
            </div>
          }

          @if (scannedTicket) {
            <div class="alert alert-info mt-3">
              <h6>Ticket Found:</h6>
              <p class="mb-1"><strong>ID:</strong> {{ scannedTicket.id }}</p>
              <p class="mb-1"><strong>Visitor:</strong> {{ scannedTicket.visitorName }} {{ scannedTicket.visitorLastName }}</p>
              <p class="mb-1"><strong>Visit Date:</strong> {{ scannedTicket.visitDate | date:'short' }}</p>
              <p class="mb-0"><strong>Type:</strong> {{ scannedTicket.type === 0 ? 'General' : 'Event Special' }}</p>
            </div>

            <div class="mb-3">
              <label class="form-label">Select Attraction</label>
              <select class="form-select" [(ngModel)]="selectedAttractionId">
                <option value="">-- Select Attraction --</option>
                @for (attraction of attractions; track attraction.id) {
                  <option [value]="attraction.id">
                    {{ attraction.name }} ({{ attraction.currentCapacity }}/{{ attraction.maxCapacity }})
                  </option>
                }
              </select>
            </div>

            <div class="d-flex gap-2">
              <button
                class="btn btn-success"
                (click)="registerEntry()"
                [disabled]="!selectedAttractionId || loading"
              >
                {{ loading ? 'Registering...' : 'Register Entry' }}
              </button>
              <button
                class="btn btn-danger"
                (click)="registerExit()"
                [disabled]="!selectedAttractionId || loading"
              >
                {{ loading ? 'Registering...' : 'Register Exit' }}
              </button>
              <button
                class="btn btn-secondary"
                (click)="resetScanner()"
              >
                Clear
              </button>
            </div>
          }
        </div>
      </div>

      <h3 class="mb-3">Current Attraction Status</h3>
      <div class="row">
        @for (attraction of attractions; track attraction.id) {
          <div class="col-md-6 col-lg-4 mb-3">
            <div class="card">
              <div class="card-body">
                <h5 class="card-title">{{ attraction.name }}</h5>
                <p class="mb-2">
                  <strong>Capacity:</strong> {{ attraction.currentCapacity }} / {{ attraction.maxCapacity }}
                </p>
                <div class="progress mb-2" style="height: 20px;">
                  <div
                    class="progress-bar"
                    [style.width.%]="(attraction.currentCapacity / attraction.maxCapacity) * 100"
                    [class.bg-success]="(attraction.currentCapacity / attraction.maxCapacity) < 0.7"
                    [class.bg-warning]="(attraction.currentCapacity / attraction.maxCapacity) >= 0.7 && (attraction.currentCapacity / attraction.maxCapacity) < 0.9"
                    [class.bg-danger]="(attraction.currentCapacity / attraction.maxCapacity) >= 0.9"
                  >
                    {{ ((attraction.currentCapacity / attraction.maxCapacity) * 100) | number:'1.0-0' }}%
                  </div>
                </div>
                @if (attraction.isActive) {
                  <span class="badge bg-success">Active</span>
                } @else {
                  <span class="badge bg-danger">Inactive (Incidents)</span>
                }
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .scanner-container {
      border: 2px solid #dee2e6;
      border-radius: 0.375rem;
      overflow: hidden;
    }
  `]
})
export class EntryExitComponent implements OnInit, OnDestroy {
  @ViewChild('videoElement') videoElement!: ElementRef<HTMLVideoElement>;

  attractions: AttractionResponse[] = [];
  successMessage = '';
  errorMessage = '';
  loading = false;

  inputMethod: 'manual' | 'camera' = 'manual';
  manualTicketId = '';
  scannedTicket: TicketResponse | null = null;
  selectedAttractionId = '';

  availableDevices: MediaDeviceInfo[] = [];
  selectedDevice?: MediaDeviceInfo;
  hasDevices?: boolean;
  hasPermission?: boolean;

  private codeReader?: BrowserMultiFormatReader;
  private scannerControls?: IScannerControls;

  constructor(
    private attractionService: AttractionService,
    private ticketService: TicketService
  ) {}

  ngOnInit(): void {
    this.loadAttractions();
  }

  ngOnDestroy(): void {
    this.stopScanner();
  }

  loadAttractions(): void {
    this.attractionService.getAll().subscribe({
      next: (response) => {
        this.attractions = response.attractions || [];
      },
      error: (error) => {
        console.error('Error loading attractions', error);
        this.errorMessage = 'Failed to load attractions';
      }
    });
  }

  async setInputMethod(method: 'manual' | 'camera'): Promise<void> {
    this.inputMethod = method;
    this.resetScanner();

    if (method === 'camera') {
      await this.initScanner();
    } else {
      this.stopScanner();
    }
  }

  async initScanner(): Promise<void> {
    try {
      this.codeReader = new BrowserMultiFormatReader();
      const devices = await BrowserMultiFormatReader.listVideoInputDevices();
      this.availableDevices = devices;
      this.hasDevices = devices && devices.length > 0;

      if (this.hasDevices) {
        this.selectedDevice = devices[0];
        this.startScanning();
      } else {
        this.errorMessage = 'No camera devices found';
      }
      this.hasPermission = true;
    } catch (error) {
      console.error('Error initializing scanner:', error);
      this.hasPermission = false;
      this.errorMessage = 'Camera permission denied or not available';
    }
  }

  async startScanning(): Promise<void> {
    if (!this.codeReader || !this.selectedDevice || !this.videoElement) return;

    try {
      this.scannerControls = await this.codeReader.decodeFromVideoDevice(
        this.selectedDevice.deviceId,
        this.videoElement.nativeElement,
        (result, error) => {
          if (result) {
            this.onScanSuccess(result.getText());
          }
        }
      );
    } catch (error) {
      console.error('Error starting scanner:', error);
      this.errorMessage = 'Failed to start camera';
    }
  }

  stopScanner(): void {
    if (this.scannerControls) {
      this.scannerControls.stop();
      this.scannerControls = undefined;
    }
  }

  lookupTicket(): void {
    if (!this.manualTicketId) return;

    this.loading = true;
    this.errorMessage = '';

    this.ticketService.getByQrCode(this.manualTicketId).subscribe({
      next: (ticket) => {
        this.scannedTicket = ticket;
        this.loading = false;
        this.successMessage = 'Ticket found successfully!';
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Ticket not found';
      }
    });
  }

  async onDeviceSelectChange(event: Event): Promise<void> {
    const target = event.target as HTMLSelectElement;
    const deviceId = target.value;
    const device = this.availableDevices.find(d => d.deviceId === deviceId);
    if (device) {
      this.selectedDevice = device;
      this.stopScanner();
      await this.startScanning();
    }
  }

  onScanSuccess(qrCodeData: string): void {
    this.loading = true;
    this.errorMessage = '';

    this.ticketService.getByQrCode(qrCodeData).subscribe({
      next: (ticket) => {
        this.scannedTicket = ticket;
        this.loading = false;
        this.successMessage = 'QR code scanned successfully!';
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = 'Invalid QR code or ticket not found';
      }
    });
  }

  registerEntry(): void {
    if (!this.scannedTicket || !this.selectedAttractionId) return;

    this.loading = true;
    this.errorMessage = '';

    // Format current local datetime as ISO string WITHOUT timezone conversion
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');
    const localDateTimeString = `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;

    const request = {
      enterDate: localDateTimeString,
      userId: this.scannedTicket.visitorId,
      qr: this.scannedTicket.qrCode,
      eventId: this.scannedTicket.eventId || undefined
    };

    this.attractionService.registerEntry(this.selectedAttractionId, request).subscribe({
      next: () => {
        this.successMessage = 'Visitor entry registered successfully!';
        this.loading = false;
        this.loadAttractions();
        this.resetScanner();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || error.error?.Message || 'Failed to register entry';
        this.loading = false;
      }
    });
  }

  registerExit(): void {
    if (!this.scannedTicket || !this.selectedAttractionId) return;

    this.loading = true;
    this.errorMessage = '';

    // Format current local datetime as ISO string WITHOUT timezone conversion
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');
    const localDateTimeString = `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;

    const request = {
      userId: this.scannedTicket.visitorId,
      exitDate: localDateTimeString
    };

    this.attractionService.registerExit(this.selectedAttractionId, request).subscribe({
      next: () => {
        this.successMessage = 'Visitor exit registered successfully!';
        this.loading = false;
        this.loadAttractions();
        this.resetScanner();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || error.error?.Message || 'Failed to register exit';
        this.loading = false;
      }
    });
  }

  resetScanner(): void {
    this.scannedTicket = null;
    this.selectedAttractionId = '';
    this.manualTicketId = '';
    this.stopScanner();
  }
}
