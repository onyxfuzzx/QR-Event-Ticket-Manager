import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WorkerService } from '../../core/services/worker.service';
import { WorkerScannerComponent } from './worker-scanner.component';
import { playScanFeedback } from './scan-feedback';
import { Router } from '@angular/router';
import { AdminSignalrService, AdminLiveEvent } from '../../core/signalr/admin-signalr.service';

type ScanResult = 'VALID' | 'INVALID' | 'REVALIDATED';

@Component({
  standalone: true,
  selector: 'app-worker',
  imports: [CommonModule, FormsModule, WorkerScannerComponent],
  templateUrl: './worker.component.html',
  styleUrls: ['./worker.component.scss']
})
export class WorkerComponent {

  code = '';
  loading = false;

  result: ScanResult | null = null;
  message = '';

  scanHistory: {
    code: string;
    status: ScanResult;
    time: Date;
  }[] = [];

  scannerOpen = false;

  constructor(private worker: WorkerService, private router: Router, private signalr: AdminSignalrService) { }

  // =========================
  // VALIDATE
  // =========================
  validate() {
    if (!this.code.trim() || this.loading) return;

    this.loading = true;

    this.worker.validateTicket(this.code).subscribe({
      next: (res: any) => {
        if (res.status === 'VALID') {
          this.feedback('VALID', 'Ticket validated');
        }
        else if (res.status === 'REVALIDATED') {
          this.feedback('REVALIDATED', 'Ticket already used');
        }
      },
      error: err => {
        if (err.status === 403) {
          this.feedback('INVALID', 'Worker not assigned');
        }
        else {
          this.feedback('INVALID', 'Invalid ticket');
        }
      }
    });



  }
  logout() {
    // Optional confirmation
    if (!confirm('Logout from scanner?')) return;

    // 🔥 Clear auth data
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');

    // 🔌 Disconnect SignalR if present
    try {
      this.signalr?.disconnect?.();
    } catch { }

    // 🚪 Redirect to login
    this.router.navigate(['/login']);
  }
  // =========================
  // QR CALLBACK
  // =========================
  onQrScanned(code: string) {
    this.scannerOpen = false;
    this.code = code;
    this.validate();
  }

  // =========================
  // FEEDBACK
  // =========================
  feedback(status: ScanResult, msg: string) {
    this.result = status;
    this.message = msg;
    this.loading = false;

    playScanFeedback(status === 'VALID' );

    this.scanHistory.unshift({
      code: this.code,
      status,
      time: new Date()
    });

    this.scanHistory = this.scanHistory.slice(0, 10);
    this.code = '';
  }

  openScanner() {
    this.scannerOpen = true;
  }

  closeScanner() {
    this.scannerOpen = false;
  }
}
