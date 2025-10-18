import { Component, computed, effect, inject, signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { CustomersService } from '../../services/customers.service';
import { AuthService, UserSession } from '../../services/auth.service';
import { CustomerDto } from '../../models/api-models';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './customers.html',
  styleUrls: ['./customers.css'],
})
export class Customers {
  private fb = inject(FormBuilder);
  private api = inject(CustomersService);
  private auth = inject(AuthService);

  // UI states
  loading = signal(false);
  errorMsg = signal('');
  showAdd = signal(false);

  // Auth state
  role = signal<string | null>(null);
  username = signal<string | null>(null);

  // Server-driven state
  items = signal<CustomerDto[]>([]);
  totalCount = signal(0);
  page = signal(1);
  pageSize = 8;
  sort = signal<'newest'|'oldest'>('newest');
  search = signal('');

  // Forms
  searchForm = this.fb.nonNullable.group({ q: [''] });
  addForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    phone: ['', [Validators.required, Validators.minLength(5)]],
    email: ['', [Validators.email]],
  });

  // Pagination helpers
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));
  startIdx = computed(() => this.totalCount() === 0 ? 0 : (this.page() - 1) * this.pageSize + 1);
  endIdx   = computed(() => Math.min(this.page() * this.pageSize, this.totalCount()));

  @ViewChild('nameInput') nameInput?: ElementRef<HTMLInputElement>;

  constructor() {
    // 🔐 حمّل جلسة المستخدم (role/username) من السيرفر عبر الكوكي
    this.auth.fetchUserFromServer().subscribe((s: UserSession) => {
      this.role.set(s.role);
      this.username.set(s.username);
    });

    // search with debounce
    this.searchForm.controls.q.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(v => {
        this.search.set(v ?? '');
        this.page.set(1);
        this.load();
      });

    // reload on sort/page changes
    effect(() => {
      this.sort(); this.page();
      this.load();
    });

    // initial load
    this.load();
  }

  async load() {
    this.loading.set(true);
    this.errorMsg.set('');
    try {
      const res = await this.api.getAll({
        page: this.page(),
        pageSize: this.pageSize,
        sort: this.sort(),
        search: this.search() || undefined,
      }).toPromise();

      this.items.set(res?.items ?? []);
      this.totalCount.set(res?.totalCount ?? 0);
    } catch (e: any) {
      this.errorMsg.set(e?.error ?? e?.message ?? 'Failed to load customers.');
    } finally {
      this.loading.set(false);
    }
  }

  // Sorting
  onSortChange(event: Event) {
    const val = (event.target as HTMLSelectElement).value as 'newest'|'oldest';
    this.sort.set(val);
    this.page.set(1);
  }

  // Pagination
  go(p: number) {
    if (p < 1 || p > this.totalPages()) return;
    this.page.set(p);
  }

  // Modal controls
  openAdd() {
    this.addForm.reset();
    this.showAdd.set(true);
    setTimeout(() => this.nameInput?.nativeElement.focus(), 0);
  }
  closeAdd() { this.showAdd.set(false); }

  // Submit Add
  async submitAdd() {
    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }
    try {
      this.loading.set(true);
      await this.api.create(this.addForm.getRawValue());
      this.closeAdd();
      this.page.set(1);
      await this.load();
    } catch (e: any) {
      this.errorMsg.set(e?.error ?? e?.message ?? 'Failed to create customer.');
    } finally {
      this.loading.set(false);
    }
  }

  // Logout button in sidebar
  logout() { this.auth.logout(); }
}
