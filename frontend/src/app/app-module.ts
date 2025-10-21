import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule, provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { FormsModule ,ReactiveFormsModule} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { DashboardComponent } from './dashboard/dashboard';
import { LoginComponent } from './login/login';
import { ProductComponent } from './product/product';
import { SuppliersComponent } from './supplier/supplier';
import { CustomersComponent } from './customers/customers';
import { Sale } from './sale/sale';

@NgModule({
  declarations: [
    App,
    DashboardComponent,
    LoginComponent,
    ProductComponent,
    SuppliersComponent,
    CustomersComponent,
    Sale,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch())
  ],
  bootstrap: [App]
})
export class AppModule { }
