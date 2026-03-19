import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SuperAdminService } from '../../core/services/superadmin.service';
import { Router } from '@angular/router';
import { AdminSignalrService, AdminLiveEvent } from '../../core/signalr/admin-signalr.service';
import { ChangePasswordComponent } from '../components/change-password/change-password.component';


@Component({
  standalone: true,
  selector: 'app-superadmin',
  imports: [CommonModule, FormsModule, ChangePasswordComponent],
  templateUrl: './superadmin.component.html',
  styleUrls: ['./superadmin.component.scss']
})
export class SuperAdminComponent implements OnInit {

  summary = {
    totalAdmins: 0,
    totalEvents: 0,
    totalTickets: 0,
    usedTickets: 0,
    revalidations: 0
  };

  admins: any[] = [];
  deletedAdmins: any[] = [];
  activity: any[] = [];

  activeTab: 'admins' | 'deleted' | 'activity' | 'settings' = 'admins';

  newAdmin = {
    name: '',
    email: '',
    password: ''
  };

  loading = false;

  constructor(private sa: SuperAdminService, private router: Router, private signalr: AdminSignalrService) { }

  ngOnInit(): void {
    this.loadSummary();
    this.loadAdmins();
  }

  /* =========================
     TAB HANDLING
     ========================= */
  switchTab(tab: 'admins' | 'deleted' | 'activity' | 'settings') {
    this.activeTab = tab;

    if (tab === 'admins') this.loadAdmins();
    if (tab === 'deleted') this.loadDeletedAdmins();
    if (tab === 'activity') this.loadActivity();
  }

  /* =========================
     DASHBOARD
     ========================= */
  loadSummary() {
    this.sa.getSummary().subscribe(res => {
      this.summary = {
        totalAdmins: res.totalAdmins ?? 0,
        totalEvents: res.totalEvents ?? 0,
        totalTickets: res.totalTickets ?? 0,
        usedTickets: res.usedTickets ?? 0,
        revalidations: res.revalidations ?? 0
      };
    });
  }

  /* =========================
     ADMINS
     ========================= */
  loadAdmins() {
    this.sa.getAdmins().subscribe(res => {
      this.admins = res.map(a => ({
        id: a.id,
        name: a.name,
        email: a.email,
        isActive: a.isActive,
        createdAt: a.createdAt
      }));
    });
  }
  logout() {
    // Optional confirmation
    if (!confirm('Logout from superadmin?')) return;

    // Clear auth data
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');

    // Disconnect SignalR if present
    try {
      this.signalr?.disconnect?.();
    } catch { }

    // Redirect to login
    this.router.navigate(['/login']);
  }

  createAdmin() {
    if (!this.newAdmin.name || !this.newAdmin.email || !this.newAdmin.password) {
      alert('All fields are required');
      return;
    }

    this.loading = true;

    this.sa.createAdmin(this.newAdmin).subscribe({
      next: () => {
        this.newAdmin = { name: '', email: '', password: '' };
        this.loadAdmins();
        this.loadSummary();
        this.loading = false;
      },
      error: err => {
        alert(err?.error ?? 'Failed to create admin');
        this.loading = false;
      }
    });
  }

  deleteAdmin(id: string) {
    if (!confirm('Delete this admin?')) return;

    this.sa.deleteAdmin(id).subscribe(() => {
      this.loadAdmins();
      this.loadSummary();
    });
  }

  /* =========================
     DELETED ADMINS
     ========================= */
  loadDeletedAdmins() {
    this.sa.getDeletedAdmins().subscribe(res => {
      this.deletedAdmins = res.map(a => ({
        id: a.id,
        name: a.name,
        email: a.email,
        createdAt: a.createdAt
      }));
    });
  }

  restoreAdmin(id: string) {
    if (!confirm('Restore this admin?')) return;

    this.sa.restoreAdmin(id).subscribe(() => {
      this.loadAdmins();
      this.loadDeletedAdmins();
      this.loadSummary();
    });
  }

  /* =========================
     ACTIVITY
     ========================= */
  loadActivity() {
    this.sa.getAdminActivity().subscribe(res => {
      this.activity = res;
    });
  }
}
