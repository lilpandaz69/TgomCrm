import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, UserSession } from '../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  role: string | null = null;
  username: string | null = null;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.fetchUserFromServer().subscribe((session: UserSession) => {
      this.role = session.role;
      this.username = session.username;

      if (!session.role) {
        this.router.navigate(['/login']);
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
