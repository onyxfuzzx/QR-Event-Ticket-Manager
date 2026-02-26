import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';


  private auth = inject(AuthService);
  private router = inject(Router);

  loading = false;
  showPassword = false; // 👈 ADD

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  login() {
    if (!this.email || !this.password) return;

    this.loading = true;

    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading = false;

        const role = localStorage.getItem('role');

        if (role === 'SuperAdmin') {
          this.router.navigate(['/superadmin']);
        } else if (role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (role === 'Worker') {
          this.router.navigate(['/worker']);
        }
      },
      error: () => {
        this.loading = false;
        alert('Login failed');
      }
    });
  }


}
