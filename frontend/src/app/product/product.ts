import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-product',
  standalone: false,
  templateUrl: './product.html',
  styleUrls: ['./product.css']
})
export class ProductComponent implements OnInit {
  products: any[] = [];
  allProducts: any[] = []; // store all products for client-side filtering
  suppliers: any[] = [];
  searchTerm = '';
  loading = false;

  showModal = false;
  editing = false;
  imagePreview: string | ArrayBuffer | null = null;

  productForm: any = {
    id: null,
    name: '',
    category: '',
    price: 0,
    stock: 0,
    supplierId: '',
    originalPrice: 0,
    imageFile: null
  };

  role = 'Owner';
  username = 'Samir';

  constructor(
    private http: HttpClient,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadSuppliers();
  }

  // Load products from API
  loadProducts() {
    this.loading = true;
    this.http.get(`${environment.apiBaseUrl}/api/products`).subscribe({
      next: (res: any) => {
        this.allProducts = res.map((p: any) => ({
          id: p.productId,
          name: p.name,
          category: p.category,
          price: p.price,
          stock: p.stock,
          originalPrice: p.orignailprice,
          imageUrl: p.imageUrl ? `${environment.apiBaseUrl}${p.imageUrl}` : null,
          supplierName: p.supplier?.name || 'Unknown'
        }));
        this.filterProducts(); // apply initial filter
        this.loading = false;
      },
      error: err => {
        console.error('Error loading products', err);
        this.loading = false;
      }
    });
  }

  // Filter products by search term (client-side)
  filterProducts() {
    const term = this.searchTerm.toLowerCase();
    this.products = this.allProducts.filter(p =>
      p.name.toLowerCase().includes(term) ||
      p.category.toLowerCase().includes(term) ||
      p.supplierName.toLowerCase().includes(term)
    );
  }

  // Load suppliers
  loadSuppliers() {
    this.http.get(`${environment.apiBaseUrl}/api/suppliers`).subscribe({
      next: (res: any) => {
        this.suppliers = res.map((s: any) => ({ id: s.supplierId, name: s.name }));
      },
      error: err => console.error('Error loading suppliers:', err)
    });
  }

  // Open modal for adding
  openAddModal() {
    this.editing = false;
    this.showModal = true;
    this.imagePreview = null;
    this.productForm = {
      id: null,
      name: '',
      category: '',
      price: 0,
      stock: 0,
      supplierId: '',
      originalPrice: 0,
      imageFile: null
    };
  }

  // Open modal for editing
  openEditModal(product: any) {
    this.editing = true;
    this.showModal = true;
    this.imagePreview = product.imageUrl;
    this.productForm = {
      id: product.id,
      name: product.name,
      category: product.category,
      price: product.price,
      stock: product.stock,
      supplierId: this.suppliers.find(s => s.name === product.supplierName)?.id || '',
      originalPrice: product.originalPrice || 0,
      imageFile: null
    };
  }

  closeModal() {
    this.showModal = false;
  }

  // File input change
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.productForm.imageFile = file;
      const reader = new FileReader();
      reader.onload = () => (this.imagePreview = reader.result);
      reader.readAsDataURL(file);
    }
  }

  // Save product (add or edit)
  saveProduct() {
    const formData = new FormData();
    formData.append('Name', this.productForm.name);
    formData.append('Category', this.productForm.category);
    formData.append('Price', this.productForm.price.toString());
    formData.append('Stock', this.productForm.stock.toString());
    formData.append('SupplierId', Number(this.productForm.supplierId).toString());
    formData.append('Orignailprice', this.productForm.originalPrice?.toString() || '0');

    if (this.productForm.imageFile) {
      formData.append('ImageFile', this.productForm.imageFile, this.productForm.imageFile.name);
    }

    const request$ = this.editing && this.productForm.id
      ? this.http.put(`${environment.apiBaseUrl}/api/products/${this.productForm.id}/update`, formData)
      : this.http.post(`${environment.apiBaseUrl}/api/products`, formData);

    request$.subscribe({
      next: () => {
        this.loadProducts();
        this.closeModal();
      },
      error: err => {
        console.error('Error saving product:', err);
        alert('Failed to save product. Check console for details.');
      }
    });
  }

  // Delete product instantly (no confirmation)
  deleteProduct(id: number) {
    this.http.delete(`${environment.apiBaseUrl}/api/products/${id}/delete`).subscribe({
      next: () => this.loadProducts(),
      error: err => console.error('Error deleting product:', err)
    });
  }

  // Logout
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
