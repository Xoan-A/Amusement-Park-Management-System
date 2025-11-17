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
  template: `
    <div class="container mt-4">
      <h1 class="mb-4">Visitor Entry/Exit Management</h1>

      @if (errorMessage) {
      <div class="alert alert-danger alert-dismissible">
        {{ errorMessage }}
        <button
          type="button"
          class="btn-close"
          (click)="errorMessage = ''"
        ></button>
      </div>
      }

      <!-- Register Entry Section -->
      <div class="card mb-4">
        <div class="card-header bg-success text-white">
          <h5 class="mb-0">Register Entry</h5>
        </div>
        <div class="card-body">
          <!-- Input Method Toggle -->
          <div class="mb-3">
            <label class="form-label">Input Method</label>
            <div class="btn-group w-100" role="group">
              <button
                type="button"
                class="btn"
                [class.btn-success]="entryInputMethod === 'manual'"
                [class.btn-outline-success]="entryInputMethod !== 'manual'"
                (click)="setEntryInputMethod('manual')"
              >
                Manual Input
              </button>
              <button
                type="button"
                class="btn"
                [class.btn-success]="entryInputMethod === 'camera'"
                [class.btn-outline-success]="entryInputMethod !== 'camera'"
                (click)="setEntryInputMethod('camera')"
              >
                QR Scanner
              </button>
            </div>
          </div>

          @if (entryInputMethod === 'camera') {
          <div class="mb-3">
            @if (availableDevices.length > 1) {
            <label class="form-label">Select Camera</label>
            <select
              class="form-select mb-3"
              (change)="onDeviceSelectChange($event)"
            >
              @for (device of availableDevices; track device.deviceId) {
              <option
                [value]="device.deviceId"
                [selected]="selectedDevice?.deviceId === device.deviceId"
              >
                {{ device.label || 'Camera ' + ($index + 1) }}
              </option>
              }
            </select>
            }

            <div
              class="scanner-container"
              style="max-width: 500px; margin: 0 auto;"
            >
              <zxing-scanner
                [device]="selectedDevice"
                (scanSuccess)="onEntryScanSuccess($event)"
                (permissionResponse)="onPermissionResponse($event)"
                (camerasFound)="onCamerasFound($event)"
              ></zxing-scanner>
            </div>

            @if (hasDevices === false) {
            <div class="alert alert-warning mt-3">
              No camera devices found. Please use manual input or check camera
              permissions.
            </div>
            }
          </div>
          }

          <div class="mb-3">
            <label for="entryQrCode" class="form-label">QR Code</label>
            <input
              type="text"
              class="form-control"
              id="entryQrCode"
              [(ngModel)]="entryForm.qrCode"
              [disabled]="!!entryForm.nfc"
              placeholder="Enter QR Code"
            />
          </div>

          <div class="mb-3">
            <label for="entryNfc" class="form-label">NFC ID</label>
            <input
              type="text"
              class="form-control"
              id="entryNfc"
              [(ngModel)]="entryForm.nfc"
              [disabled]="!!entryForm.qrCode"
              placeholder="Enter NFC ID"
            />
          </div>

          <div class="mb-3">
            <label for="entryAttraction" class="form-label"
              >Select Attraction</label
            >
            <select
              class="form-select"
              id="entryAttraction"
              [(ngModel)]="entryForm.attractionId"
            >
              <option value="">-- Select Attraction --</option>
              @for (attraction of attractions; track attraction.id) {
              <option [value]="attraction.id">
                {{ attraction.name }} ({{ attraction.currentCapacity }}/{{
                  attraction.maxCapacity
                }})
              </option>
              }
            </select>
          </div>

          <div class="mb-3">
            <label for="entryEvent" class="form-label"
              >Select Event (Optional)</label
            >
            <select
              class="form-select"
              id="entryEvent"
              [(ngModel)]="entryForm.eventId"
            >
              <option value="">-- No Event --</option>
              @for (event of events; track event.id) {
              <option [value]="event.id">
                {{ event.name }} - {{ event.date }} ({{ event.currentCapacity }}/{{ event.maxCapacity }})
              </option>
              }
            </select>
          </div>

          <div class="d-flex gap-2">
            <button
              class="btn btn-success"
              (click)="registerEntry()"
              [disabled]="
                (!entryForm.qrCode && !entryForm.nfc) || !entryForm.attractionId || loading
              "
            >
              <i class="bi bi-box-arrow-in-right"></i>
              {{ loading ? 'Registering...' : 'Register Entry' }}
            </button>
            <button class="btn btn-secondary" (click)="resetEntryForm()">
              Clear
            </button>
          </div>
        </div>
      </div>

      <!-- Register Exit Section -->
      <div class="card mb-4">
        <div class="card-header bg-danger text-white">
          <h5 class="mb-0">Register Exit</h5>
        </div>
        <div class="card-body">
          <div class="mb-3">
            <label for="exitUserId" class="form-label">NFC</label>
            <input
              type="text"
              class="form-control"
              id="exitUserId"
              [(ngModel)]="exitForm.userId"
              placeholder="Enter visitor NFC"
            />
          </div>

          <div class="mb-3">
            <label for="exitAttraction" class="form-label"
              >Select Attraction</label
            >
            <select
              class="form-select"
              id="exitAttraction"
              [(ngModel)]="exitForm.attractionId"
            >
              <option value="">-- Select Attraction --</option>
              @for (attraction of attractions; track attraction.id) {
              <option [value]="attraction.id">
                {{ attraction.name }} ({{ attraction.currentCapacity }}/{{
                  attraction.maxCapacity
                }})
              </option>
              }
            </select>
          </div>

          <div class="d-flex gap-2">
            <button
              class="btn btn-danger"
              (click)="registerExit()"
              [disabled]="!exitForm.userId || !exitForm.attractionId || loading"
            >
              <i class="bi bi-box-arrow-right"></i>
              {{ loading ? 'Registering...' : 'Register Exit' }}
            </button>
            <button class="btn btn-secondary" (click)="resetExitForm()">
              Clear
            </button>
          </div>
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
                <strong>Capacity:</strong> {{ attraction.currentCapacity }} /
                {{ attraction.maxCapacity }}
              </p>
              <div class="progress mb-2" style="height: 20px;">
                <div
                  class="progress-bar"
                  [style.width.%]="
                    (attraction.currentCapacity / attraction.maxCapacity) * 100
                  "
                  [class.bg-success]="
                    attraction.currentCapacity / attraction.maxCapacity < 0.7
                  "
                  [class.bg-warning]="
                    attraction.currentCapacity / attraction.maxCapacity >=
                      0.7 &&
                    attraction.currentCapacity / attraction.maxCapacity < 0.9
                  "
                  [class.bg-danger]="
                    attraction.currentCapacity / attraction.maxCapacity >= 0.9
                  "
                >
                  {{
                    (attraction.currentCapacity / attraction.maxCapacity) * 100
                      | number : '1.0-0'
                  }}%
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

    const request: any = {};

    if (this.entryForm.qrCode) {
      request.Qr = this.entryForm.qrCode;
    }

    if (this.entryForm.nfc) {
      request.NFC = this.entryForm.nfc;
    }

    if (this.entryForm.eventId) {
      request.EventId = this.entryForm.eventId;
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
