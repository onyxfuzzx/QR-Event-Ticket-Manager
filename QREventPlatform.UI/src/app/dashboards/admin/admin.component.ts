import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { AdminEventService } from '../../core/services/admin-event.service';
import { Router } from '@angular/router';
import { AdminSignalrService, AdminLiveEvent } from '../../core/signalr/admin-signalr.service';
import { EventFormBuilderComponent } from './components/event-form-builder/event-form-builder.component';
import { EmailTemplateBuilderComponent } from './components/email-template-builder/email-template-builder.component';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';




type LiveLog = {
  message: string;
  time: Date;
};


@Component({
  standalone: true,
  selector: 'app-admin',
  imports: [CommonModule, FormsModule, EventFormBuilderComponent, EmailTemplateBuilderComponent],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit, OnDestroy {

  summary = {
    totalEvents: 0,
    totalWorkers: 0,
    totalTickets: 0,
    usedTickets: 0,
    revalidations: 0,
    unauthorized: 0
  };

  auditEventId = '';
  auditRows: any[] = [];
  auditColumns: string[] = [];

  activeTab:
    | 'events'
    | 'tickets'
    | 'workers'
    | 'assign'
    | 'deleted'
    | 'scans'
    | 'formaudit'
    | 'email' = 'events';
  liveLogs: { message: string; time: Date }[] = [];
  scanLogs: any[] = [];          // persistent logs
  liveFeed: AdminLiveEvent[] = []; // live events (top bar / toast)



  events: any[] = [];
  workers: any[] = [];
  assignableWorkers: any[] = [];
  eventWorkers: Record<string, any[]> = {};
  assignedWorkerIds = new Set<string>();
  deletedWorkers: any[] = [];
  showDeletedWorkers = false;
  eventTicketStats: any = null;
  eventTickets: any[] = [];
  loadingEventTickets = false;

  newEvent = { name: '', location: '', date: '' };
  newWorker = { name: '', email: '', password: '' };
  workerError = '';

  selectedEventId = '';
  selectedWorkerId = '';
  selectedWorkerIds: string[] = [];


  private token = localStorage.getItem('token')!;
  constructor(
    private adminService: AdminService,
    private eventService: AdminEventService,
    private http: HttpClient,
    private router: Router,
    private signalr: AdminSignalrService) { }

  ngOnDestroy() {
    this.signalr.disconnect();
  }

  switchTab(tab:
    | 'events'
    | 'tickets'
    | 'workers'
    | 'assign'
    | 'deleted'
    | 'scans'
    | 'formaudit'
    | 'email'
  ) {
    this.activeTab = tab;

    switch (tab) {
      case 'events':
        this.loadEvents();
        this.loadSummary();
        break;

      case 'tickets':
        this.loadSummary();
        if (this.selectedEventId) {
          this.onTicketEventChange();
        }
        break;

      case 'workers':
        this.loadWorkers();
        break;

      case 'assign':
        if (this.selectedEventId) {
          this.onEventChange();
        }
        break;

      case 'deleted':
        this.loadDeletedWorkers();
        break;

      case 'scans':
        this.loadScanHistory();
        break;
      case 'formaudit':
        this.loadFormAudit();
        break;
    }
  }
  
  ngOnInit() {
    this.loadSummary();
    this.loadEvents();
    this.loadWorkers();
    this.loadDeletedWorkers();
    this.loadScanHistory();
    

    this.signalr.connect(this.token);

    // ✅ CALL IT
    this.listenLiveEvents();

    // Live feed (top bar / toast)
    this.signalr.liveEvents$.subscribe(ev => {
      if (!ev) return;
      this.liveFeed.unshift(ev);
      this.liveFeed = this.liveFeed.slice(0, 10);
    });

    // Live scan table
    this.signalr.onTicketScanned(scan => {
      this.scanLogs.unshift({
        ticketCode: scan.ticketCode,
        eventName: scan.eventName,
        workerName: scan.workerName,
        scanResult: scan.result,
        scannedAt: scan.time
      });
    });
  }
  onTicketEventChange() {
    if (!this.selectedEventId) {
      this.eventTicketStats = null;
      this.eventTickets = [];
      return;
    }

    

    this.loadingEventTickets = true;

    this.adminService.getEventTickets(this.selectedEventId).subscribe({
      next: res => {
        this.eventTicketStats = res.stats;
        this.eventTickets = res.tickets;
        this.loadingEventTickets = false;
      },
      error: () => {
        this.loadingEventTickets = false;
      }
    });
  }

  logout() {
    // Optional confirmation
    if (!confirm('Logout from admin?')) return;

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
  loadEventTicketStats(eventId: string) {
    const event = this.events.find(e => e.id === eventId);
    if (!event) return;

    this.eventTicketStats = {
      totalTickets: event.tickets,
      usedTickets: event.usedTickets,
      revalidations: this.scanLogs.filter(
        s => s.eventName === event.name && s.scanResult === 'REVALIDATED'
      ).length
    };
  }

  loadFormAudit() {
    if (!this.auditEventId) return;

    this.adminService
      .getFormAudit(this.auditEventId)
      .subscribe(rows => {
        this.auditRows = rows;

        this.auditColumns = rows.length
          ? Object.keys(rows[0].data)
          : [];
      });
  }


  loadEventTickets(eventId: string) {
    this.loadingEventTickets = true;

    this.adminService.getTicketsByEvent(eventId).subscribe({
      next: res => {
        this.eventTickets = res;
        this.loadingEventTickets = false;
      },
      error: () => {
        this.eventTickets = [];
        this.loadingEventTickets = false;
      }
    });
  }



  loadScanHistory() {
    this.adminService.getScanHistory().subscribe(res => {
      this.scanLogs = res.map((s: any) => ({
        ticketCode: s.ticketCode,
        eventName: s.eventName,
        workerName: s.workerName,
        scanResult: s.scanResult,
        scannedAt: s.scannedAt
      }));
    });
  }

  

  listenLiveEvents() {
    this.signalr.liveEvents$.subscribe(event => {
      if (!event) return;

      this.liveLogs.unshift({
        message: event.message,
        time: new Date()
      });

      this.liveLogs = this.liveLogs.slice(0, 20);

      if (!this.selectedEventId || !this.eventTicketStats) return;

      if (event.type === 'TICKET_VALID') {
        this.summary.usedTickets++;
      }

      if (event.type === 'REVALIDATED') {
        this.summary.revalidations++;
      }

      if (event.type == 'UNAUTHORIZED') {
        this.summary.unauthorized++;
      }

    });
  }


  

  loadSummary() {
    this.adminService.getDashboardSummary().subscribe(r => {
      this.summary = {
        totalEvents: r.events ?? 0,
        totalWorkers: r.workers ?? 0,
        totalTickets: r.tickets ?? 0,
        usedTickets: r.usedTickets ?? 0,
        revalidations: r.revalidations ?? 0,
        unauthorized: r.unauthorized ?? 0
      };
    });
  }



  loadDeletedWorkers() {
    this.adminService.getDeletedWorkers().subscribe(res => {
      this.deletedWorkers = res;
    });
  }

  restoreWorker(workerId: string) {
    if (!confirm('Restore this worker?')) return;

    this.adminService.restoreWorker(workerId).subscribe(() => {
      this.loadWorkers();
      this.loadDeletedWorkers();
    });
  }

  loadEvents() {
    this.eventService.getEvents().subscribe(res => {
      this.events = res.map(e => ({
        id: e.id,
        name: e.name,
        location: e.location ?? '-',
        eventDate: e.eventDate ? new Date(e.eventDate) : null,
        tickets: e.tickets,
        usedTickets: e.usedTickets
      }));
    });
  }

  createEvent() {
    this.eventService.createEvent({
      name: this.newEvent.name,
      location: this.newEvent.location,
      eventDate: new Date(this.newEvent.date).toISOString()
    }).subscribe(() => {
      this.newEvent = { name: '', location: '', date: '' };
      this.loadEvents();
    });
  }

  deleteEvent(id: string) {
    if (!confirm('Delete event?')) return;
    this.eventService.deleteEvent(id).subscribe(() => this.loadEvents());
  }

  loadWorkers() {
    this.adminService.getWorkers().subscribe(res => {
      this.workers = res.map(w => ({
        id: w.id,
        name: w.name,
        email: w.email,
        isActive: w.isActive
      }));
    });
  }

  createWorker() {
    this.workerError = '';

    if (!this.newWorker.name || !this.newWorker.email || !this.newWorker.password) {
      this.workerError = 'All fields are required';
      return;
    }

    if (this.newWorker.password.length < 8) {
      this.workerError = 'Password must be at least 8 characters';
      return;
    }

    this.adminService.createWorker(this.newWorker).subscribe({
      next: () => {
        this.newWorker = { name: '', email: '', password: '' };
        this.workerError = '';
        this.loadWorkers();
      },
      error: (err: any) => {
        // Handle structured validation errors
        if (err?.error?.errors) {
          const firstErrorKey = Object.keys(err.error.errors)[0];
          this.workerError = err.error.errors[firstErrorKey][0];
        } else {
          this.workerError = err?.error || err?.message || 'Failed to create worker. Please try again.';
        }
      }
    });
  }

  deleteWorker(id: string) {
    if (!confirm('Delete worker?')) return;
    this.adminService.deleteWorker(id).subscribe(() => this.loadWorkers());

  }

  onEventChange() {
    if (!this.selectedEventId) return;

    this.loadAssignedWorkers(this.selectedEventId);

    const assigned =
      this.eventWorkers[this.selectedEventId]?.map(w => w.workerId) ?? [];

    this.assignableWorkers =
      this.workers.filter(w => !assigned.includes(w.id));

    this.selectedWorkerIds = [];
    this.selectedWorkerId = '';
  }

  assignWorker() {
    if (!this.selectedWorkerId) return;
    this.adminService.assignWorker({
      eventId: this.selectedEventId,
      workerId: this.selectedWorkerId
    }).subscribe(() => this.onEventChange());
  }

  onWorkerCheckboxChange(workerId: string, event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) this.selectedWorkerIds.push(workerId);
    else this.selectedWorkerIds =
      this.selectedWorkerIds.filter(id => id !== workerId);
  }

  bulkAssign() {
    if (!this.selectedEventId || this.selectedWorkerIds.length === 0) return;

    this.adminService.bulkAssignWorkers(
      this.selectedEventId,
      this.selectedWorkerIds
    ).subscribe(() => {
      // 🔄 refresh UI immediately
      this.onEventChange();
    });
  }



  loadAssignedWorkers(eventId: string) {
    this.adminService.getEventWorkers(eventId).subscribe((res: any[]) => {

      const mapped = res.map(w => ({
        assignmentId: w.assignmentId,
        workerId: w.workerId,
        name: w.name,
        email: w.email,
        assignedAt: w.assignedAt
      }));

      this.eventWorkers[eventId] = mapped;

      this.assignedWorkerIds.clear();
      mapped.forEach(w => this.assignedWorkerIds.add(w.workerId));
    });
  }



  


  unassignWorker(assignmentId: string, eventId: string) {
    this.eventWorkers[eventId] =
      this.eventWorkers[eventId].filter(w => w.assignmentId !== assignmentId);

    this.adminService.unassignWorker(assignmentId).subscribe({
      error: () => this.loadAssignedWorkers(eventId) // rollback if needed
    });
  }


  
  openQr(url: string) {
    if (!url) return;
  
    // If backend sends relative path like /qr/xxxx
    if (url.startsWith('/')) {
      url = environment.apiUrl + url;
    }
  
    // If protocol missing
    if (!/^https?:\/\//i.test(url)) {
      url = 'https://' + url;
    }
  
    window.open(url, '_blank', 'noopener,noreferrer');
  }
  

  formOpen(eventId: string) {
    const angularUrl = window.location.origin; 
    const url = `${angularUrl}/event/${eventId}`;
    window.open(url, '_blank', 'noopener,noreferrer');
  }


  generateTicket(eventId: string) {
    const email = prompt('Enter customer email');
    if (!email) return;

    this.adminService.createTicket(eventId, email).subscribe({
      next: res => {
        alert(`Ticket sent to ${email}`);
        window.open(res.qrUrl, '_blank');

        // 🔄 refresh event-specific data
        this.loadSummary();
        this.loadEventTicketStats(eventId);
        this.loadEventTickets(eventId);
        this.onTicketEventChange();

      },
      error: err => alert(err.error ?? 'Ticket generation failed')
    });
  }



}
