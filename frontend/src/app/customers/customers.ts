import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router'; // ✅ fixed import

@Component({
  selector: 'app-customers',
  standalone: false,
  templateUrl: './customers.html',
  styleUrls: ['./customers.css']
})
export class CustomersComponent implements OnInit {
  customers: any[] = [];
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
    private router: Router,         // ✅ fixed Router type
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.addForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      phone: ['', [Validators.required, Validators.minLength(5)]],
      email: ['', [Validators.email]]
    });

    this.loadCustomers();
  }

  // 🔹 Load all customers
  loadCustomers() {
    this.loading = true;
    this.http
      .get<any>(
        `${environment.apiBaseUrl}/api/customers?search=${this.search}&sort=${this.sort}&page=${this.page}&pageSize=${this.pageSize}`
      )
      .subscribe({
        next: (res) => {
          this.customers = res.items;
          this.totalCount = res.totalCount;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading customers:', err);
          this.loading = false;
        }
      });
  }

  // 🔹 Sorting
  onSortChange(event: Event) {
    this.sort = (event.target as HTMLSelectElement).value;
    this.page = 1;
    this.loadCustomers();
  }

  // 🔹 Searching
  onSearch(event: Event) {
    this.search = (event.target as HTMLInputElement).value;
    this.page = 1;
    this.loadCustomers();
  }

  // 🔹 Pagination
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  go(p: number) {
    if (p < 1 || p > this.totalPages) return;
    this.page = p;
    this.loadCustomers();
  }

  // 🔹 Add form controls
  openAdd() {
    this.showAddForm = true;
    this.addForm.reset();
    setTimeout(() => this.nameInput?.nativeElement.focus(), 0);
  }

  closeAdd() {
    this.showAddForm = false;
  }

  // 🔹 Save new customer (no alert, just auto-refresh)
  addCustomer() {
    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.http
      .post(`${environment.apiBaseUrl}/api/customers`, this.addForm.value)
      .subscribe({
        next: () => {
          this.closeAdd();     // ✅ close form
          this.page = 1;       // ✅ go back to first page
          this.loadCustomers(); // ✅ auto refresh list
          this.loading = false;
        },
        error: (err) => {
          console.error('Failed to add customer', err);
          this.loading = false;
        }
      });
  }

  // 🔹 Logout
  logout(): void {
    this.http.post(`${environment.apiBaseUrl}/api/Auth/logout`, {}, { withCredentials: true }).subscribe({
      next: () => {
        this.authService.clearUser();
        this.router.navigate(['/login']); // ✅ works now
      },
      error: () => {
        this.authService.clearUser();
        this.router.navigate(['/login']); // ✅ works now
      }
    });
  }
}
