import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EventRegisterService {
  private api = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // 🔹 Get event info (name, etc.)
  getEvent(eventId: string): Observable<{ id: string; name: string }> {
    return this.http.get<{ id: string; name: string }>(
      `${this.api}/public/events/${eventId}`
    );
  }

  // 🔹 Get dynamic form schema
  getForm(eventId: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.api}/public/events/${eventId}/form`
    );
  }

  // 🔹 Submit registration
  submitForm(eventId: string, data: any): Observable<any> {
    return this.http.post(
      `${this.api}/public/events/${eventId}/submit`,
      data
    );
  }
}
