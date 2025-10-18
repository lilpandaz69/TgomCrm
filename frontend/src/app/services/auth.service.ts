import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';

export interface UserSession {
  role: string | null;
  username: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);

  // Stream لحالة المستخدم
  private session$ = new BehaviorSubject<UserSession>({ role: null, username: null });

  // للاشتراك من الكومبوننت
  sessionChanges(): Observable<UserSession> {
    return this.session$.asObservable();
  }

  // لقطة حالية
  get sessionSnapshot(): UserSession {
    return this.session$.value;
  }

  // ✅ Login — السيرفر يكتب الكوكي
  login(username: string, password: string) {
    return this.http.post<{ role: string }>(
      `${environment.apiBaseUrl}/api/Auth/login`,
      { username, password },
      { withCredentials: true }
    ).pipe(
      tap(res => this.session$.next({ role: res.role ?? null, username }))
    );
  }

  // ✅ Logout — يمسح الكوكي من السيرفر ويوجّه للّوجين
  logout() {
    this.http.post(`${environment.apiBaseUrl}/api/Auth/logout`, {}, { withCredentials: true })
      .subscribe({
        next: () => {
          this.session$.next({ role: null, username: null });
          window.location.href = '/login';
        },
        error: () => {
          this.session$.next({ role: null, username: null });
          window.location.href = '/login';
        }
      });
  }

  // ✅ عند فتح الصفحة أو الريلود — يجيب الدور من السيرفر عبر الكوكي
  fetchUserFromServer(): Observable<UserSession> {
    // لو عندنا قيمة بالفعل فالسيرفس، رجّعها (تقلل طلبات)
    const snap = this.sessionSnapshot;
    if (snap.role) return of(snap);

    return this.http
      .get<{ username: string; role: string }>(
        `${environment.apiBaseUrl}/api/Auth/me`,
        { withCredentials: true }
      )
      .pipe(
        map(res => ({ role: res?.role ?? null, username: res?.username ?? null })),
        tap(s => this.session$.next(s)),
        catchError(() => {
          const empty = { role: null, username: null };
          this.session$.next(empty);
          return of(empty);
        })
      );
  }
}
