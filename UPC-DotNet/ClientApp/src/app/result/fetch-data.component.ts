import { Component, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  constructor(
    private route: ActivatedRoute,
    private currencyPipe: CurrencyPipe) { }

  responseCode: string;
  responseMessage: string;
  pay_timestamp: string;
  pay_amount: string;
  language: string;
  orderId: string;
  billNumber: string;
  orderAmount: string;
  orderDescription: string;
  cardType: string;
  orderCurrency: string;
  cardScheme: string;
  token: string;
  order_timestamp: string;
  client_ip_addr: string;
  //product

  ngOnInit() {
    console.log('Called Constructor');
    //this.product = history.state;

    this.route.queryParams.subscribe(params => {
      this.responseCode = decodeURIComponent(params['responseCode']);
      this.billNumber = decodeURIComponent(params['billNumber']);
      this.orderAmount = decodeURIComponent(params['orderAmount']);
      this.orderCurrency = decodeURIComponent(params['orderCurrency']);
      this.order_timestamp = decodeURIComponent(params['order_timestamp']);

      //this.order_timestamp = new Date(parseInt(this.order_timestamp) * 1000).toLocaleString();

      if (this.orderCurrency == "VND") {
        this.orderAmount = this.currencyPipe.transform(this.orderAmount, 'VND', false).replace("VND", "") + " VND";
      } else {
        this.orderAmount = this.currencyPipe.transform(this.orderAmount, 'USD', false).replace("USD", "") + " USD";
      }
    });
  }
}
