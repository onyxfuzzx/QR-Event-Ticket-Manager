import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SuperAdminService {

  private base = `${environment.apiUrl}/superadmin`;

  constructor(private http: HttpClient) { }

  /* =========================
     DASHBOARD
     ========================= */
  getSummary() {
    return this.http.get<any>(`${this.base}/dashboard/summary`);
  }

  getAdminActivity() {
    return this.http.get<any[]>(`${this.base}/dashboard/admins/activity`);
  }

  /* =========================
     ADMINS
     ========================= */
  getAdmins() {
    return this.http.get<any[]>(`${this.base}/admins`);
  }

  getDeletedAdmins() {
    return this.http.get<any[]>(`${this.base}/admins/deleted`);
  }

  createAdmin(data: {
    name: string;
    email: string;
    password: string;
  }) {
    return this.http.post(`${this.base}/create-admin`, data);
  }

  deleteAdmin(adminId: string) {
    return this.http.delete(`${this.base}/admins/${adminId}`);
  }

  restoreAdmin(id: string) {
    return this.http.post(`${this.base}/admins/${id}/restore`, {});
  }
}
