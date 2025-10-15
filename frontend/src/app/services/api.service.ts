import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Customer, InventoryItem, Invoice, InvoiceOverview,
  Product, Sale, Supplier, NewProduct, NewCustomer, NewSupplier, NewSale
} from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private base = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  // -------- Customers ----------
  getCustomerByPhone(phone: string): Observable<Customer> {
    return this.http.get<Customer>(`${this.base}/api/Customers/by-phone/${encodeURIComponent(phone)}`);
  }
  createCustomer(body: NewCustomer): Observable<Customer> {
    return this.http.post<Customer>(`${this.base}/api/Customers`, body);
  }

  // -------- Inventory ----------
  getInventory(): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>(`${this.base}/api/Inventory`);
  }

  // -------- Invoices -----------
  getInvoicesBySale(saleId: number): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${this.base}/api/Invoices/by-sale/${saleId}`);
  }
  getInvoiceOverview(supplierId: number, productId: number): Observable<InvoiceOverview> {
    return this.http.get<InvoiceOverview>(`${this.base}/api/Invoices/overview/${supplierId}/${productId}`);
  }

  // -------- Products -----------
  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.base}/api/Products`);
  }
  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.base}/api/Products/${id}`);
  }
  createProduct(body: NewProduct): Observable<Product> {
    return this.http.post<Product>(`${this.base}/api/Products`, body);
  }

  // -------- Sales --------------
  getSales(): Observable<Sale[]> {
    return this.http.get<Sale[]>(`${this.base}/api/Sales`);
  }
  createSale(body: NewSale): Observable<Sale> {
    return this.http.post<Sale>(`${this.base}/api/Sales`, body);
  }
  returnSale(saleId: number, body?: any): Observable<any> {
    // لو الـ endpoint محتاج body عدّله هنا
    return this.http.post(`${this.base}/api/Sales/return/${saleId}`, body ?? {});
  }

  // -------- Suppliers ----------
  getSuppliers(): Observable<Supplier[]> {
    return this.http.get<Supplier[]>(`${this.base}/api/Suppliers`);
  }
  getSupplierById(id: number): Observable<Supplier> {
    return this.http.get<Supplier>(`${this.base}/api/Suppliers/${id}`);
  }
  createSupplier(body: NewSupplier): Observable<Supplier> {
    return this.http.post<Supplier>(`${this.base}/api/Suppliers`, body);
  }
}
