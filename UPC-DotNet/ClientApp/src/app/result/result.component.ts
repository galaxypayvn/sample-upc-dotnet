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

    //this.route.queryParams.subscribe(params => {
    //  this.responseCode = decodeURIComponent(params['responseCode']);
    //  this.billNumber = decodeURIComponent(params['billNumber']);
    //  this.orderAmount = decodeURIComponent(params['orderAmount']);
    //  this.orderCurrency = decodeURIComponent(params['orderCurrency']);
    //  this.order_timestamp = decodeURIComponent(params['order_timestamp']);

    //  this.param1 = decodeURIComponent(params['param1']);
    //  this.param2 = decodeURIComponent(params['param2']);

    //  if (this.orderCurrency == "VND") {
    //    this.orderAmount = this.currencyPipe.transform(this.orderAmount, 'VND', false).replace("VND", "") + " VND";
    //  } else {
    //    this.orderAmount = this.currencyPipe.transform(this.orderAmount, 'USD', false).replace("USD", "") + " USD";
    //  }

    //  this.param1 = JSON.parse(this.param1);
    //  this.param2 = JSON.parse(this.param2);
    //});
  }
}
