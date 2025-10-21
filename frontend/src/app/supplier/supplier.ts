import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-suppliers',
  standalone: false,
  templateUrl: './supplier.html',
  styleUrls: ['./supplier.css']
})
export class SuppliersComponent implements OnInit {
  suppliers: any[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 8;
  sort = 'newest';
  search = '';
  loading = false;
  showAddForm = false;
  role = 'Owner';
  username = 'Samir';

  addForm!: FormGroup;
  @ViewChild('nameInput') nameInput!: ElementRef;

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.addForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      phone: ['', [Validators.required, Validators.minLength(5)]]
    });

    // ✅ Clear duplicate error automatically when user types again
    this.addForm.get('phone')?.valueChanges.subscribe(() => {
      if (this.addForm.get('phone')?.hasError('duplicate')) {
        this.addForm.get('phone')?.setErrors(null);
      }
    });

    this.loadSuppliers();
  }

  // ✅ Load suppliers from backend
  loadSuppliers() {
    this.loading = true;
    this.http
      .get<any>(
        `${environment.apiBaseUrl}/api/suppliers?search=${this.search}&sort=${this.sort}&page=${this.page}&pageSize=${this.pageSize}`
      )
      .subscribe({
        next: (res) => {
          // Handle possible response shapes
          if (Array.isArray(res)) {
            this.suppliers = res;
            this.totalCount = res.length;
          } else if (res.items) {
            this.suppliers = res.items;
            this.totalCount = res.totalCount || this.suppliers.length;
          } else if (res.data) {
            this.suppliers = res.data;
            this.totalCount = res.count || this.suppliers.length;
          } else {
            this.suppliers = [];
            this.totalCount = 0;
          }

          this.loading = false;
        },
        error: (err) => {
          console.error('❌ Error loading suppliers:', err);
          this.loading = false;
          this.suppliers = [];
          this.totalCount = 0;
        }
      });
  }

  // ✅ Handle sorting
  onSortChange(event: Event) {
    this.sort = (event.target as HTMLSelectElement).value;
    this.page = 1;
    this.loadSuppliers();
  }

  // ✅ Handle searching
  onSearch(event: Event) {
    this.search = (event.target as HTMLInputElement).value;
    this.page = 1;
    this.loadSuppliers();
  }

  // ✅ Pagination
  get totalPages(): number {
    return Math.max(1, Math.ceil((this.totalCount || 0) / this.pageSize));
  }

  getPages() {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  go(p: number) {
    if (p < 1 || p > this.totalPages) return;
    this.page = p;
    this.loadSuppliers();
  }

  // ✅ Modal controls
  openAdd() {
    this.showAddForm = true;
    this.addForm.reset();
    setTimeout(() => this.nameInput?.nativeElement.focus(), 0);
  }

  closeAdd() {
    this.showAddForm = false;
  }

  // ✅ Add supplier (with frontend duplicate phone check)
  addSupplier() {
    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }

    const phoneValue = this.addForm.value.phone.trim();
    const phoneExists = this.suppliers.some(
      (s) => s.phone.trim().toLowerCase() === phoneValue.toLowerCase()
    );

    if (phoneExists) {
      this.addForm.get('phone')?.setErrors({ duplicate: true });
      return;
    }

    this.loading = true;

    this.http
      .post<any>(`${environment.apiBaseUrl}/api/suppliers`, this.addForm.value)
      .subscribe({
        next: (res) => {
          console.log('✅ Supplier Created:', res);
          this.closeAdd();
          this.loadSuppliers();
          this.loading = false;
        },
        error: (err) => {
          console.error('❌ Failed to add supplier', err);
          alert('An error occurred while saving. Please try again.');
          this.loading = false;
        }
      });
  }

  // ✅ Logout
  logout(): void {
    this.http
      .post(`${environment.apiBaseUrl}/api/Auth/logout`, {}, { withCredentials: true })
      .subscribe({
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
