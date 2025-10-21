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
  filteredSuppliers: any[] = [];
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
    // Initialize Add Supplier form
    this.addForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      phone: ['', [Validators.required, Validators.minLength(5)]]
    });

    // Clear duplicate error automatically when editing phone
    this.addForm.get('phone')?.valueChanges.subscribe(() => {
      if (this.addForm.get('phone')?.hasError('duplicate')) {
        this.addForm.get('phone')?.setErrors(null);
      }
    });

    this.loadSuppliers();
  }

  // Search input event
  onSearch(event: Event) {
    this.search = (event.target as HTMLInputElement).value.toLowerCase();
    this.page = 1;
    this.applyFilter();
  }

  // Filter suppliers in frontend
  applyFilter() {
    if (!this.search) {
      this.filteredSuppliers = [...this.suppliers];
    } else {
      this.filteredSuppliers = this.suppliers.filter(
        s =>
          s.name.toLowerCase().includes(this.search) ||
          s.phone.toLowerCase().includes(this.search)
      );
    }
    this.totalCount = this.filteredSuppliers.length;
  }

  // Load suppliers from API
  loadSuppliers() {
    this.loading = true;
    const params = `?search=${encodeURIComponent(this.search)}&sort=${this.sort}&page=${this.page}&pageSize=${this.pageSize}`;

    this.http.get<any>(`${environment.apiBaseUrl}/api/suppliers${params}`).subscribe({
      next: res => {
        if (res && Array.isArray(res)) {
          this.suppliers = res;
          // Remove exact duplicates (same name + phone)
          this.suppliers = this.suppliers.filter((s, index, self) =>
            index === self.findIndex(t => t.name === s.name && t.phone === s.phone)
          );
        } else if (res.items) {
          this.suppliers = res.items;
        } else {
          this.suppliers = [];
        }

        // Apply frontend filter
        this.applyFilter();
        this.loading = false;
      },
      error: err => {
        console.error('Error loading suppliers:', err);
        this.suppliers = [];
        this.filteredSuppliers = [];
        this.totalCount = 0;
        this.loading = false;
      }
    });
  }

  // Sort change
  onSortChange(event: Event) {
    this.sort = (event.target as HTMLSelectElement).value;
    this.page = 1;
    this.loadSuppliers();
  }

  // Pagination
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  go(p: number) {
    if (p < 1 || p > this.totalPages) return;
    this.page = p;
    this.loadSuppliers();
  }

  // Modal controls
  openAdd() {
    this.showAddForm = true;
    this.addForm.reset();
    setTimeout(() => this.nameInput?.nativeElement.focus(), 0);
  }

  closeAdd() {
    this.showAddForm = false;
  }

  // Add new supplier
  addSupplier() {
    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }

    const phoneValue = this.addForm.value.phone.trim();
    const exists = this.suppliers.some(s => s.phone?.trim() === phoneValue);
    if (exists) {
      this.addForm.get('phone')?.setErrors({ duplicate: true });
      return;
    }

    this.loading = true;
    this.http.post(`${environment.apiBaseUrl}/api/suppliers`, this.addForm.value).subscribe({
      next: () => {
        this.closeAdd();
        this.page = 1;
        this.loadSuppliers();
        this.loading = false;
      },
      error: err => {
        console.error('Error adding supplier', err);
        this.loading = false;
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
