// auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router, private auth: AuthService) {}

  async canActivate(): Promise<boolean> {
    try {
      const role = await firstValueFrom(this.auth.fetchUserFromServer());
      if (role) return true;

      this.router.navigate(['/login']);
      return false;
    } catch {
      this.router.navigate(['/login']);
      return false;
    }
  }
}
