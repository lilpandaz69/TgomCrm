import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { firstValueFrom } from 'rxjs';
import { CustomerDto, CustomersListResponse } from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class CustomersService {
  private http = inject(HttpClient);
  private BASE = `${environment.apiBaseUrl}/api/customers`;

  getAll(opts: { page: number; pageSize: number; search?: string; sort?: 'newest'|'oldest' }) {
    let params = new HttpParams()
      .set('page', opts.page)
      .set('pageSize', opts.pageSize)
      .set('sort', opts.sort ?? 'newest');

    if (opts.search?.trim()) {
      params = params.set('search', opts.search.trim());
    }

    return this.http.get<CustomersListResponse>(this.BASE, { params, withCredentials: true });
  }

  getByPhone(phone: string) {
    return this.http.get<CustomerDto>(`${this.BASE}/by-phone/${encodeURIComponent(phone)}`, { withCredentials: true });
  }

  async create(dto: CustomerDto) {
    return await firstValueFrom(this.http.post<CustomerDto>(this.BASE, dto, { withCredentials: true }));
  }
}
