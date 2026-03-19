import { Injectable, NgZone } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';


export interface AdminLiveEvent {
  type: 'TICKET_VALID' | 'TICKET_INVALID' | 'REVALIDATED' | 'WORKER_ASSIGNED' | 'UNAUTHORIZED';
  message: string;
  eventId?: string;
  workerName?: string;
  time: Date;
}

@Injectable({ providedIn: 'root' })
export class AdminSignalrService {

  private hub?: HubConnection;
  private apiUrl = `${environment.apiUrl}`
  private hubUrl = `${environment.hubUrl}`
  /** Live events stream */
  private events$ = new BehaviorSubject<AdminLiveEvent | null>(null);
  liveEvents$ = this.events$.asObservable();

  /** Connection state */
  private connected$ = new BehaviorSubject<boolean>(false);
  isConnected$ = this.connected$.asObservable();

  constructor(private zone: NgZone) { }

  // =============================
  // CONNECT
  // =============================
  connect(token: string) {
    if (this.hub?.state === HubConnectionState.Connected) return;

    this.hub = new HubConnectionBuilder()
      .withUrl(`${environment.hubUrl}/admin`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.registerListeners();

    this.hub
      .start()
      .then(() => {
        console.log('Admin SignalR connected');
        this.connected$.next(true);
      })
      .catch(err => {
        console.error('SignalR connection failed', err);
        this.connected$.next(false);
      });
  }

  // =============================
  // LISTENERS
  // =============================
  private registerListeners() {
    if (!this.hub) return;

    this.hub.on('TicketValidated', (payload: any) => {
      this.emit({
        type: 'TICKET_VALID',
        message: `Ticket validated for ${payload.eventName}`,
        eventId: payload.eventId,
        workerName: payload.workerName,
        time: new Date()
      });
    });

    this.hub.on('TicketInvalid', (payload: any) => {
      this.emit({
        type: 'TICKET_INVALID',
        message: 'Invalid ticket scanned',
        eventId: payload.eventId,
        time: new Date()
      });
    });

    this.hub.on('TicketRevalidated', (payload: any) => {
      this.emit({
        type: 'REVALIDATED',
        message: 'Ticket already used',
        eventId: payload.eventId,
        time: new Date()
      });
    });

    this.hub.on('UNAUTHORIZED', (payload: any) => {
      this.emit({
        type: 'UNAUTHORIZED',
        message: `Unauthorized scan by ${payload.workerName}`,
        eventId: payload.eventId,
        time: new Date()
      });
    });

    this.hub.on('WorkerAssigned', (payload: any) => {
      this.emit({
        type: 'WORKER_ASSIGNED',
        message: `${payload.workerName} assigned to event`,
        eventId: payload.eventId,
        workerName: payload.workerName,
        time: new Date()
      });
    });
  }

  onTicketScanned(cb: (data: any) => void) {
    if (!this.hub) return;
    this.hub.on('TicketScanned', cb);
  }

  // =============================
  // EMIT SAFELY INTO ANGULAR
  // =============================
  private emit(event: AdminLiveEvent) {
    this.zone.run(() => {
      this.events$.next(event);
    });
  }

  // =============================
  // DISCONNECT
  // =============================
  disconnect() {
    if (!this.hub) return;

    this.hub.stop();
    this.connected$.next(false);
    console.log('Admin SignalR disconnected');
  }
}
