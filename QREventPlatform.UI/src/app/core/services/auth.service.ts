import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';


@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  //private apiUrl = `${environment.apiUrl}`
  private api = `${environment.apiUrl}/auth`;

  login(email: string, password: string) {
    return this.http.post<{ accessToken: string, role: string }>(`${this.api}/login`, {
      email,
      password
    }).pipe(
      tap(res => {
        localStorage.setItem('token', res.accessToken);
        localStorage.setItem('role', res.role);
      })
    );
  }

  logout() {
    localStorage.removeItem('token');
  }



  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  /** CHANGE PASSWORD (FOR LOGGED IN USERS) */
  changePassword(data: any) {
    return this.http.post(`${this.api}/change-password`, data);
  }

  /** FORGOT PASSWORD (REQUEST LINK) */
  forgotPassword(email: string) {
    return this.http.post(`${this.api}/forgot-password`, { email });
  }

  /** RESET PASSWORD (USING TOKEN FROM EMAIL) */
  resetPassword(data: any) {
    return this.http.post(`${this.api}/reset-password`, data);
  }
}
