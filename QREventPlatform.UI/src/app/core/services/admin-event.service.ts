import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminEventService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // =========================
  // EVENTS
  // =========================
  getEvents() {
    return this.http.get<any[]>(
      `${this.baseUrl}/admin/dashboard/events`
    );
  }

  createEvent(payload: {
    name: string;
    location: string;
    eventDate: string;
  }) {
    return this.http.post(`${this.baseUrl}/events`, payload);
  }

  deleteEvent(eventId: string) {
    return this.http.delete(`${this.baseUrl}/events/${eventId}`);
  }

  /** RESTORE EVENT */
  restoreEvent(eventId: string) {
    return this.http.post(`${this.baseUrl}/events/${eventId}/restore`, {});
  }
}
