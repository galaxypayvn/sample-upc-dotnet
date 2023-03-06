import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { CancelComponent } from './cancel/cancel.component';
import { PaymentComponent } from './home/payment.component';
import { RouterComponent } from './router/router.component';
import { ResultComponent } from './result/result.component';
import { CommonModule, CurrencyPipe } from '@angular/common';
import {DocumentComponent} from "./swagger/swagger.component";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    CancelComponent,
    PaymentComponent,
    RouterComponent,
    DocumentComponent,
    ResultComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: PaymentComponent, pathMatch: 'full' },
      { path: 'cancel', component: CancelComponent },
      { path: 'success', component: ResultComponent },
      { path: 'router', component: RouterComponent },
      { path: 'documents', component: DocumentComponent }
    ])
  ],
  providers: [CurrencyPipe],
  bootstrap: [AppComponent]
})
export class AppModule { }
