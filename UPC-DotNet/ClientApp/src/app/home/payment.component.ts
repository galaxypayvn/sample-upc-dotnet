import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Component, Inject, ElementRef, ViewChild } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-payment-component',
  templateUrl: './payment.component.html'
})
export class PaymentComponent {
  public currentCount = 0;
  public billNumber = Math.floor((Math.random() * 100000)).toString();
  public firstName = "Galaxy";
  public lastName = "Pay";
  public lang = "vi";
  public city = "HCM";
  public email = "demo@galaxypay.vn";
  public amount = "10,000";
  public currency = "VND";
  public address = "41 Kim Long";
  public description = "Secure Page Demo";
  public cardtype = "atm";
  public bank = "970437";
  public otp = "on";

  public resultData: ResponseData;

  bankSelect = "momo";
  cardTypeSelect = "momo";

  public loading: boolean = false;
  public isDisabledButton: boolean = false;

  public internationalOption = [
    {
      value: "VISA",
      text: "VISA",
    },
    {
      value: "MASTER",
      text: "MASTER CARD",
    }
  ];

  public atmOption = [
    {
      value: "970437",
      text: "HD bank",
    },
    {
      value: "970403",
      text: "Sacombank (Napas)",
    }
  ];

  // Bank Momo
  public momoOption = [
    {
      value: "momo",
      text: "MOMO Wallet",
    }
  ]

  // Service Provider
  public cardType = [
    {
      value: "momo",
      text: "MOMO",
    },
    {
      value: "international",
      text: "International Card (VISA, MASTER CARD, JCB,...)",
    },
    {
      value: "atm",
      text: "ATM",
    },
  ];

  public data = this.momoOption;

  constructor(public http: HttpClient,
    @Inject('BASE_URL')
    public baseUrl: string,
    private route: Router,
    private currencyPipe: CurrencyPipe
  ) { }

  transformAmount(element) {
    this.amount = (<HTMLInputElement>document.getElementById("orderAmount")).value;
    this.currency = (<HTMLInputElement>document.getElementById("orderCurrency")).value;

    var numb = this.amount.match(/\d/g);
    this.amount = numb.join("");

    if (this.currency == "VND") {
      this.amount = this.currencyPipe.transform(this.amount, "VND", false);
      this.amount = this.amount.replace("VND", "");
      //element.target.value = this.amount;

      console.log("VND");
    } else {
      this.amount = this.currencyPipe.transform(this.amount, "USD", false);
      this.amount = this.amount.replace("USD", "");
      //element.target.value = this.amount;

      console.log("USD");
    }
  }

  filterCardType(filterVal: any) {
    switch (filterVal) {
      case "international":
        this.data = this.internationalOption;
        this.bankSelect = "VISA";
        break;
      case "atm":
        this.data = this.atmOption;
        this.bankSelect = "970437";
        break;
      case "momo":
        this.data = this.momoOption;
        this.bankSelect = "momo";
        break;
    } 
  }

  public inProcess() {

    this.loading = true;
    this.isDisabledButton = true;

    this.billNumber = (<HTMLInputElement>document.getElementById("billNumber")).value;
    this.firstName = (<HTMLInputElement>document.getElementById("firstName")).value;
    this.lastName = (<HTMLInputElement>document.getElementById("lastName")).value;
    this.lang = (<HTMLInputElement>document.getElementById("language")).value;
    this.city = (<HTMLInputElement>document.getElementById("city")).value;
    this.email = (<HTMLInputElement>document.getElementById("email")).value;
    this.amount = (<HTMLInputElement>document.getElementById("orderAmount")).value;
    this.currency = (<HTMLSelectElement>document.getElementById("orderCurrency")).value;
    this.description = (<HTMLInputElement>document.getElementById("orderDescription")).value;
    this.cardtype = (<HTMLSelectElement>document.getElementById("cardType")).value;
    this.bank = (<HTMLSelectElement>document.getElementById("bank")).value;
    //this.request = (<HTMLSelectElement>document.getElementById("request")).value;

    var data = {
      billNumber: this.billNumber,
      firstName: this.firstName,
      lastName: this.lastName,
      language: this.lang,
      city: this.city,
      email: this.email,
      orderAmount: this.amount,
      orderCurrency: this.currency,
      address: this.address,
      orderDescription: this.description,
      cardType: this.cardtype,
      bank: this.bank,
      otp: this.otp,
      request: "purchase"
    };

    this.http.post<ResponseData>(this.baseUrl + 'api/client', data)
      .subscribe(result => {
        this.resultData = result;
        if (result.responseCode == "00" && result.endpoint != null) {
          window.location.href = result.endpoint;
        }

        this.loading = false;
        this.isDisabledButton = false;
      }, error => console.error(error));
  }

  public inCancel() {
    this.route.navigate(['/cancel']);
  }
}

interface ResponseData {
  responseCode: string;
  orderId: string;
  endpoint: string;
  signature: string;
}


