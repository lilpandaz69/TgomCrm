import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable, of } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private role: string | null = null;

  constructor(private http: HttpClient) {}

  setUser(role: string) {
    this.role = role;
  }

  getRole(): string | null {
    return this.role;
  }

  clearUser() {
    this.role = null;
  }

  // ✅ Called on app start or page reload
  fetchUserFromServer(): Observable<string | null> {
    if (this.role) return of(this.role); // already cached in memory

    return this.http
      .get<{ role: string }>(`${environment.apiBaseUrl}/api/Auth/me`, { withCredentials: true })
      .pipe(
        map(res => res?.role || null),     // ✅ Convert to string | null
        tap(role => this.role = role),     // ✅ Save in memory
        catchError(() => of(null))         // ✅ Handle errors gracefully
      );
  }
}
