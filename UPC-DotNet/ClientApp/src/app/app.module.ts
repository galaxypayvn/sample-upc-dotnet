import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './cancel/home.component';
import { PaymentComponent } from './home/payment.component';
import { FetchDataComponent } from './result/fetch-data.component';
import { CommonModule, CurrencyPipe } from '@angular/common';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    PaymentComponent,
    FetchDataComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: PaymentComponent, pathMatch: 'full' },
      { path: 'cancel', component: HomeComponent },
      { path: 'success', component: FetchDataComponent },
      { path: 'dynamic', component: FetchDataComponent },
    ])
  ],
  providers: [CurrencyPipe],
  bootstrap: [AppComponent]
})
export class AppModule { }
