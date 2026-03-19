import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrls: ['../login/login.component.scss']
})
export class ResetPasswordComponent implements OnInit {
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  token = '';
  data = {
    newPassword: '',
    confirmPassword: ''
  };

  loading = false;
  success = false;
  error = '';

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token') || '';
    if (!this.token) {
      this.error = 'Invalid reset link. No token found.';
    }
  }

  submit() {
    if (this.data.newPassword !== this.data.confirmPassword) {
      this.error = 'Passwords do not match.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.auth.resetPassword({
      token: this.token,
      newPassword: this.data.newPassword,
      confirmPassword: this.data.confirmPassword
    }).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.success = true;
        // Optional auto-login or redirect
      },
      error: err => {
        this.loading = false;
        this.error = err.error?.message || 'Token may be expired or invalid.';
      }
    });
  }
}
