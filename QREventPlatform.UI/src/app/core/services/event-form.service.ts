import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { EventFormField } from '../models/event-form-field';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EventFormService {
  private baseUrl = `${environment.apiUrl}/event-forms`;

  constructor(private http: HttpClient) { }

  getForm(eventId: string) {
    return this.http.get<EventFormField[] | null>(
      `${this.baseUrl}/${eventId}`
    );
  }

  saveForm(eventId: string, fields: EventFormField[]) {
    return this.http.post(
      `${this.baseUrl}/${eventId}`,
      fields
    );
  }
}
