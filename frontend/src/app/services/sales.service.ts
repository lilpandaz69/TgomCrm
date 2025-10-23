import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Customer {
  customerId: number;
  name: string;
  phone: string;
  email: string;
}

export interface Supplier {
  supplierId: number;
  name: string;
}

export interface Product {
  productId: number;
  name: string;
  price: number;
  supplierId: number;
}

export interface SaveSaleResponse {
  saleId: number;
}

@Injectable({ providedIn: 'root' })
export class SalesService {
  private base = environment.apiBaseUrl;
  constructor(private http: HttpClient) {}

  async listCustomers(): Promise<Customer[]> {
    const url = `${this.base}/api/Customers`;
    const res = await firstValueFrom(this.http.get<any>(url, { withCredentials: true }));
    // السيرفر بيرجع object فيه data؟ ولا array؟ لو بيرجع object فيه list غيّر دي
    return res.items ?? res ?? [];
  }

  async listSuppliers(): Promise<Supplier[]> {
    const url = `${this.base}/api/Suppliers`;
    return firstValueFrom(this.http.get<Supplier[]>(url, { withCredentials: true }));
  }

  async listProductsBySupplier(supplierId: number): Promise<Product[]> {
    const url = `${this.base}/api/Products?supplierId=${supplierId}`;
    return firstValueFrom(this.http.get<Product[]>(url, { withCredentials: true }));
  }

  async saveSale(customerPhone: string, lines: { productId: number; quantity: number; }[]): Promise<SaveSaleResponse> {
    const body = {
      saleId: 0,
      customerPhone,
      productId: lines[0].productId,
      quantity: lines[0].quantity
    };
    const res = await firstValueFrom(this.http.post<any>(`${this.base}/api/Sales`, body, { withCredentials: true }));
    return { saleId: res.saleId ?? 0 };
  }
}
