import {Component, Inject} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {CurrencyPipe} from '@angular/common';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './router.component.html',
})
export class RouterComponent {
  constructor(
    public http: HttpClient,
    @Inject('BASE_URL')
    public baseUrl: string,
    private route: ActivatedRoute,
    private router: Router,
    private currencyPipe: CurrencyPipe) {
  }

  method: string;
  transactionID: string;

  // result
  rawContent: string;
  responseContent: string;
  responseCode: string;
  orderNumber: string;
  orderAmount: string;
  orderCurrency: string;
  orderDateTime: string;

  queryTransaction() {
    this.http
      .post<ResponseData>(this.baseUrl + 'api/transaction', {transactionID: this.transactionID})
      .subscribe(result => {
        this.rawContent = result.rawContent;
        this.responseContent = result.responseContent;
        this.responseCode = result.responseContent;
        this.orderNumber = result.orderNumber;
        this.orderAmount = result.orderAmount;
        this.orderCurrency = result.orderCurrency;
        this.orderDateTime = result.orderDateTime;

        this.showResult();
      }, error => console.error(error));
  }

  showResult() {
    if (this.method == 'cancel') {
      this.router.navigateByUrl('/cancel', {
        state: {
          ResponseData: this.rawContent,
          DecryptData: this.responseContent
        }
      });
    }

    if (this.method == 'success') {

      if (this.orderCurrency == "VND") {
        this.orderAmount = this.currencyPipe.transform(this.orderAmount, 'VND', false).replace("VND", "") + " VND";
      } else {
        this.orderAmount = this.currencyPipe.transform(this.orderAmount, 'USD', false).replace("USD", "") + " USD";
      }

      this.router.navigateByUrl('/success', {
        state: {
          BillNumber: this.orderNumber,
          OrderAmount: this.orderAmount,
          PayTimestamp: this.orderDateTime,
          ResponseData: this.rawContent,
          DecryptData: this.responseContent
        }
      });
    }
  }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {

      this.method = decodeURIComponent(params['method']);
      this.transactionID = decodeURIComponent(params['transactionID']);
      this.queryTransaction()
    });
  }
}

interface ResponseData {
  orderNumber: string;
  orderAmount: string;
  orderCurrency: string;
  orderDateTime: string;
  rawContent: string;
  responseContent: string;
  responseCode: string;
}
