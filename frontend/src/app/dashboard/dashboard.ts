import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  role: string | null = null;

  constructor(
    private http: HttpClient,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // ✅ Try to get user from memory or fetch from backend cookie
    this.authService.fetchUserFromServer().subscribe(role => {
      this.role = role;
      if (!role) {
        this.router.navigate(['/login']);
      }
    });
  }

  logout(): void {
    this.http.post(`${environment.apiBaseUrl}/api/Auth/logout`, {}, { withCredentials: true }).subscribe({
      next: () => {
        this.authService.clearUser();
        this.router.navigate(['/login']);
      },
      error: () => {
        this.authService.clearUser();
        this.router.navigate(['/login']);
      }
    });
  }
}
