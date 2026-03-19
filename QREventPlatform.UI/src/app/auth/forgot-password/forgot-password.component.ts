import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['../login/login.component.scss'] // REUSE LOGIN STYLES
})
export class ForgotPasswordComponent {
  private auth = inject(AuthService);
  
  email = '';
  loading = false;
  message = '';
  error = '';

  submit() {
    this.loading = true;
    this.message = '';
    this.error = '';

    this.auth.forgotPassword(this.email).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.message = res.message;
      },
      error: err => {
        this.loading = false;
        this.error = err.error?.message || "Something went wrong";
      }
    });
  }
}
