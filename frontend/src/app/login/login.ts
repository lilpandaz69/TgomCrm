import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  username = '';
  password = '';
  loading = false;
  errorMessage = '';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  onLogin() {
    if (!this.username || !this.password) {
      this.errorMessage = 'Please enter both username and password';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.login(this.username, this.password)
      .subscribe({
        next: (res) => {
          // ✅ السيرفس نفسه بيخزن الـ role ويكتب الكوكي
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          this.loading = false;
          if (err.status === 401) {
            this.errorMessage = 'Invalid username or password';
          } else {
            this.errorMessage = 'An error occurred. Please try again.';
          }
        },
        complete: () => this.loading = false
      });
  }
}
