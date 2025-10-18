export interface Customer { id: number; name: string; phone: string; email?: string; }
export interface NewCustomer { name: string; phone: string; email?: string; }

export interface Supplier { id: number; name: string; phone?: string; }
export interface NewSupplier { name: string; phone?: string; }

export interface Product { id: number; name: string; price: number; supplierId?: number; }
export interface NewProduct { name: string; price: number; supplierId?: number; }

export interface InventoryItem { productId: number; productName: string; quantity: number; }

export interface Sale { id: number; date: string; customerId?: number; total: number; }
export interface NewSale { customerId?: number; items: Array<{ productId: number; qty: number; price: number }>; }

export interface Invoice { id: number; saleId: number; total: number; createdAt: string; }
export interface InvoiceOverview { supplierId: number; productId: number; totalInvoices: number; totalAmount: number; }
export interface CustomerDto {
  customerId?: number;
  name: string;
  phone: string;
  email?: string | null;
}

export interface CustomersListResponse {
  items: CustomerDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

