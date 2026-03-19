import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private baseUrl = `${environment.apiUrl}/admin`;
  private ticketUrl = `${environment.apiUrl}/tickets`;
  private apiUrl = `${environment.apiUrl}`
  private eventUrl = `${environment.apiUrl}`
  constructor(private http: HttpClient) { }

  // =========================
  // DASHBOARD
  // =========================
  getDashboardSummary(): Observable<any> {
    return this.http.get(`${this.baseUrl}/dashboard/summary`);
  }

  // =========================
  // WORKERS
  // =========================
  getWorkers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/workers`);
  }

  createWorker(payload: {
    name: string;
    email: string;
    password: string;
  }) {
    return this.http.post(`${this.baseUrl}/create-worker`, payload);
  }
  getTicketsByEvent(eventId: string) {
    return this.http.get<any[]>(
      `${this.apiUrl}/tickets/event/${eventId}`
    );
  }
  getEventTickets(eventId: string) {
    return this.http.get<{
      stats: {
        totalTickets: number;
        usedTickets: number;
        revalidations: number;
      };
      tickets: any[];
    }>(`${this.apiUrl}/tickets/event/${eventId}`);
  }

  getScanHistory() {
    return this.http.get<any[]>(
      `${this.apiUrl}/tickets/scan-history`
    );
  }


  restoreWorker(workerId: string) {
    return this.http.post(
      `${this.baseUrl}/workers/${workerId}/restore`,
      {}
    );
  }





  getFormAudit(
    eventId: string
  ): Observable<{ createdAt: string; data: Record<string, string> }[]> {
    return this.http.get<
      { createdAt: string; data: Record<string, string> }[]
    >(`${this.baseUrl}/form-audit/${eventId}`);
  }




  

 


  getDeletedWorkers() {
    return this.http.get<any[]>(
      `${this.baseUrl}/workers/deleted`
    );
  }

  deleteWorker(workerId: string) {
    return this.http.delete(`${this.baseUrl}/workers/${workerId}`);
  }

 

  // =========================
  // ASSIGNMENT
  // =========================
  assignWorker(payload: { eventId: string; workerId: string }) {
    return this.http.post(`${this.baseUrl}/assign-worker`, payload);
  }

  bulkAssignWorkers(eventId: string, workerIds: string[]) {
    return this.http.post(`${this.baseUrl}/assign-workers-bulk`, {
      eventId,
      workerIds
    });
  }

  unassignWorker(assignmentId: string) {
    return this.http.delete(`${this.baseUrl}/event-workers/${assignmentId}`);
  }

  /** 🔁 RESTORE ASSIGNMENT */
  restoreAssignment(assignmentId: string) {
    return this.http.post(
      `${this.baseUrl}/event-workers/${assignmentId}/restore`,
      {}
    );
  }

  // =========================
  // EVENT → WORKERS
  // =========================
  getEventWorkers(eventId: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.baseUrl}/events/${eventId}/workers`
    );
  }

  // =========================
  // TICKETS
  // =========================
  createTicket(eventId: string, email: string) {
    return this.http.post<any>(
      `${this.ticketUrl}/${eventId}`,
      { email }
    );
  }

  deleteTicket(ticketId: string) {
    return this.http.delete(`${this.ticketUrl}/${ticketId}`);
  }

  /** 🔁 RESTORE TICKET */
  restoreTicket(ticketId: string) {
    return this.http.post(`${this.ticketUrl}/${ticketId}/restore`, {});
  }

  // =========================
  // EMAIL TEMPLATES
  // =========================
  getEventEmailTemplate(eventId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/events/${eventId}/template`);
  }

  saveEventEmailTemplate(eventId: string, payload: { layoutJson: string, htmlContent: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/events/${eventId}/template`, payload);
  }

  testEmailTemplate(eventId: string, payload: { toEmail: string, htmlContent: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/events/${eventId}/template/test`, payload);
  }
}
