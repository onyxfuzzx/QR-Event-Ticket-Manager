import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent {
  private auth = inject(AuthService);

  data = {
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
  };

  loading = false;
  error: string | null = null;
  success: string | null = null;

  submit() {
    if (this.data.newPassword !== this.data.confirmNewPassword) {
      this.error = "New passwords don't match!";
      return;
    }

    this.loading = true;
    this.error = null;
    this.success = null;

    this.auth.changePassword(this.data).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.success = "Password updated successfully!";
        this.data = { currentPassword: '', newPassword: '', confirmNewPassword: '' };
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || "Something went wrong";
      }
    });
  }
}
