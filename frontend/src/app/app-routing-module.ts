import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login';
import { DashboardComponent } from './dashboard/dashboard';
import { AuthGuard } from './services/auth.guard';
import { Sale } from './sale/sale';
import { ProductComponent } from './product/product';
import { CustomersComponent } from './customers/customers';
import { SuppliersComponent } from './supplier/supplier';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'sale', component: Sale },
  { path: 'product', component: ProductComponent },
  { path: 'customers', component: CustomersComponent },
  { path: 'supplier', component: SuppliersComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
