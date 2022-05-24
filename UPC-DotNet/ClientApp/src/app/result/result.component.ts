import { Component, Inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './result.component.html'
})
export class ResultComponent {
  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private currencyPipe: CurrencyPipe) { }

  responseCode: string;
  billNumber: string;
  orderAmount: string;
  orderCurrency: string;
  pay_timestamp: string;
  product;

  param1: string;
  param2: string;

  ngOnInit() {
    console.log('Called Constructor');
    this.product = history.state;

    this.billNumber = history.state.BillNumber;
    this.orderAmount = history.state.OrderAmount;
    this.pay_timestamp = history.state.PayTimestamp;

    this.param1 = JSON.parse(history.state.ResponseData);
    this.param2 = JSON.parse(history.state.DecryptData);
  }
}
