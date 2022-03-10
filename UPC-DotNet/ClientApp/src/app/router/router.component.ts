import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-home',
  templateUrl: './router.component.html',
})
export class RouterComponent {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private currencyPipe: CurrencyPipe) { }

  param1: string;
  param2: string;
  method: string;

  // result
  responseCode: string;
  billNumber: string;
  orderAmount: string;
  orderCurrency: string;
  pay_timestamp: string;

  ngOnInit() {
    console.log('Called Constructor');

    this.route.queryParams.subscribe(params => {

      this.method = decodeURIComponent(params['method']);
      this.param1 = decodeURIComponent(params['param1']);
      this.param2 = decodeURIComponent(params['param2']);

      if (this.method == 'cancel') {
        this.router.navigateByUrl('/cancel', { state: { ResponseData: this.param1, DecryptData: this.param2 } });
      }

      if (this.method == 'success') {
        this.responseCode = decodeURIComponent(params['responseCode']);
        this.billNumber = decodeURIComponent(params['billNumber']);
        this.orderAmount = decodeURIComponent(params['orderAmount']);
        this.orderCurrency = decodeURIComponent(params['orderCurrency']);
        this.pay_timestamp = decodeURIComponent(params['pay_timestamp']);

        if (this.orderCurrency == "VND") {
          this.orderAmount = this.currencyPipe.transform(this.orderAmount, 'VND', false).replace("VND", "") + " VND";
        } else {
          this.orderAmount = this.currencyPipe.transform(this.orderAmount, 'USD', false).replace("USD", "") + " USD";
        }

        this.router.navigateByUrl('/success', {
          state: {
            BillNumber: this.billNumber,
            OrderAmount: this.orderAmount,
            PayTimestamp: this.pay_timestamp,
            ResponseData: this.param1,
            DecryptData: this.param2
          }
        });
      }
    });
  }
}
