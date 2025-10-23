import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { SalesService, Customer, Supplier, Product } from '../services/sales.service';

@Component({
  selector: 'app-sale',
  templateUrl: './sale.html',
  standalone: false,
  styleUrls: ['./sale.css']
})
export class Sale implements OnInit {

  constructor(private fb: FormBuilder, private api: SalesService, private router: Router) {}

  // ▼▼ New: sidebar user + logout
  username = 'User';
  role = 'Member';

  form!: FormGroup;
  customers: Customer[] = [];
  suppliers: Supplier[] = [];
  products: Product[] = [];

  loading = false;
  successMsg = '';
  errorMsg = '';

  ngOnInit(): void {
    // ▼▼ hydrate user info (safe fallback)
    try {
      const raw = localStorage.getItem('tg_user') || localStorage.getItem('user');
      const u = raw ? JSON.parse(raw) : null;
      this.username = u?.name ?? u?.username ?? this.username;
      this.role = u?.role ?? this.role;
    } catch {}

    this.form = this.fb.group({
      customerPhone: ['', [Validators.required]],
      supplierId: [null, [Validators.required]],
      productId: [null, [Validators.required]],
      quantity: [1, [Validators.required, Validators.min(1)]]
    });

    this.loadCustomers();
    this.loadSuppliers();
  }

  // ▼▼ New: logout
  logout(): void {
    try {
      localStorage.removeItem('tg_user');
      localStorage.removeItem('token');
      sessionStorage.clear();
    } catch {}
    this.router.navigateByUrl('/login');
  }

  async loadCustomers(): Promise<void> {
    try {
      this.customers = await this.api.listCustomers(); // زي ما هي
    } catch (err) {
      console.error('Error loading customers', err);
    }
  }

  async loadSuppliers(): Promise<void> {
    try {
      this.suppliers = await this.api.listSuppliers();
    } catch (err) {
      console.error('Error loading suppliers', err);
    }
  }

  async onSupplierChange(): Promise<void> {
    const supplierId = this.form.get('supplierId')!.value;
    this.products = [];
    if (!supplierId) return;

    try {
      this.products = await this.api.listProductsBySupplier(supplierId);
    } catch (err) {
      console.error('Error loading products', err);
    }
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.errorMsg = 'Please fill all fields correctly.';
      return;
    }

    this.loading = true;
    this.errorMsg = '';
    this.successMsg = '';

    try {
      const body = this.form.value;
      const res = await this.api.saveSale(body.customerPhone, [{
        productId: body.productId,
        quantity: body.quantity
      }]);
      this.successMsg = `Sale saved successfully! ID: ${res.saleId}`;
      this.form.reset({ quantity: 1 });
    } catch (err: any) {
      console.error(err);
      this.errorMsg = 'Error saving sale.';
    } finally {
      this.loading = false;
    }
  }

  // Getter صغير لو كنت بتستخدمه في الكروت
  get selectedCustomerName(): string {
    const phone = this.form?.value?.customerPhone;
    const c = this.customers.find(x => x.phone === phone);
    return c ? c.name : '-';
  }
}
