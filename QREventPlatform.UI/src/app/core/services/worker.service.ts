import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class WorkerService {

  private base = `${environment.apiUrl}`;

  constructor(private http: HttpClient) { }

  validateTicket(code: string) {
    return this.http.post<any>(`${this.base}/tickets/validate`, {
      code
    });
  }
}
