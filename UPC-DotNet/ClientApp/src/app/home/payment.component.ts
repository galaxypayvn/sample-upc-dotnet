import {HttpClient} from '@angular/common/http';
import {Router} from '@angular/router';
import {Component, Inject, ElementRef, ViewChild} from '@angular/core';
import {CommonModule, CurrencyPipe} from '@angular/common';
import {ConsoleLogger} from "@angular/compiler-cli/ngcc";

@Component({
  selector: 'app-payment-component',
  templateUrl: './payment.component.html'
})
export class PaymentComponent {

  // Default Providers
  readonly paymentGroups = {
    hub: {
      text: "PAYMENT HUBS",
      value: "HUB"
    }
  };

  readonly paymentProviders = {
    hub2c2p: {
      text: "HUB 2C2P",
      value: "2C2P"
    },
    bankDefault: {
      text: "VIETNAM LOCAL BANKS",
      value: ""
    },
    bank970400: {
      text: "SAIGON BANK/NGÂN HÀNG TMCP SÀI GÒN CÔNG THƯƠNG",
      value: "970400"
    }
  };

  //
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
  public json = "";
  public cardNumber = "9704000000000018";
  public cardHolderName = "Nguyen Van A";
  public cardExpireDate = "01/39";
  public cardIssueDate = "03/07";
  public cardVerificationValue = "100";
  public labelCardDate = "Card Issue Date";
  public IsHostedMerchant = "NO";
  public extraData = {
    customer: {
      firstName: "Jacob",
      lastName: "Savannah",
      identityNumber: "6313126925",
      email: "Jacob@gmail.com",
      phoneNumber: "0580821083",
      phoneType: "Mobile",
      gender: "F",
      dateOfBirth: "19920117",
      title: "Mr"
    },
    billing: {
      countryCode: "vn",
      stateProvine: "Hồ Chí Minh",
      cityName: "Nhà Bè",
      postalCode: "",
      streetNumber: "673",
      addressLine1: "Đường Nguyễn Hữu Thọ",
      addressLine2: ""
    },
    shipping: {
      countryCode: "vn",
      stateProvine: "Hồ Chí Minh",
      cityName: "Nhà Bè",
      postalCode: "",
      streetNumber: "673",
      addressLine1: "Đường Nguyễn Hữu Thọ",
      addressLine2: ""
    }
  };

  public resultData: ResponseData;
  public isDisableHosted = true;
  public isCVV = true;
  public loading: boolean = false;
  public isDisabledButton: boolean = false;

  public internationalOption = [
    {
      value: "VISA",
      text: "VISA CARD",
    },
    {
      value: "MASTER",
      text: "MASTER CARD",
    }
  ];

  public atmOption = [
    this.paymentProviders.bankDefault,
    this.paymentProviders.bank970400
  ];

  // Bank Momo
  public momoOption = [
    {
      value: "MOMO",
      text: "MOMO WALLET",
    }
  ]

  public hupOption = [
    {
      value: this.paymentProviders.hub2c2p.value,
      text: this.paymentProviders.hub2c2p.text,
    }
  ]

  // Service Provider
  public cardType = [
    {
      value: "atm",
      text: "ATM CARD",
    },
    {
      value: "international",
      text: "INTERNATIONAL CARD (VISA, MASTER CARD, JCB,...)",
    },
    {
      value: "Wallet",
      text: "WALLET",
    },
    {
      value: this.paymentGroups.hub.value,
      text: this.paymentGroups.hub.text,
    }
  ];

  // Merchant hosted
  public hosts = [
    {
      value: "YES",
      text: "YES",
    },
    {
      value: "NO",
      text: "NO",
    }
  ];

  // currencyOption ATM and MOMO
  public currencyOptionDomestic = [
    {
      value: "VND",
      text: "VND",
    }
  ]

  // currencyOption MPGS
  public currencyOptionMPGS = [
    {
      value: "VND",
      text: "VND",
    },
    {
      value: "USD",
      text: "USD",
    }
  ]

  // currencyOption 2C2P
  public currencyOption2C2P = [
    {
      value: "VND",
      text: "VND",
    },
    {
      value: "USD",
      text: "USD",
    },
    {
      value: "THB",
      text: "THB",
    },
    {
      value: "JPY",
      text: "JPY",
    },
    {
      value: "INR",
      text: "INR",
    },
    {
      value: "TWD",
      text: "TWD",
    },
    {
      value: "MYR",
      text: "MYR",
    },
    {
      value: "SGD",
      text: "SGD",
    },
    {
      value: "KRW",
      text: "KRW",
    },
    {
      value: "KHR",
      text: "KHR",
    },
    {
      value: "MMK",
      text: "MMK",
    },
    {
      value: "IDR",
      text: "IDR",
    },
    {
      value: "HKD",
      text: "HKD",
    },
    {
      value: "CNY",
      text: "CNY",
    }
  ]

  // currencyOption
  public currencyOption = this.currencyOptionDomestic;

  hostSelect = "NO";
  bankSelect = this.paymentProviders.bankDefault.value;
  cardTypeSelect = "atm";
  public data = this.atmOption;

  constructor(public http: HttpClient,
              @Inject('BASE_URL')
              public baseUrl: string,
              private route: Router,
              private currencyPipe: CurrencyPipe
  ) {
  }

  transformAmount(element) {
    this.amount = (<HTMLInputElement>document.getElementById("orderAmount")).value;
    this.currency = (<HTMLInputElement>document.getElementById("orderCurrency")).value;

    var numb = this.amount.match(/\d/g);
    this.amount = numb.join("");

    if (this.currency == "USD") {
      this.amount = this.currencyPipe.transform(this.amount, "USD", false);
      this.amount = this.amount.replace("USD", "");
      //element.target.value = this.amount;

      console.log("USD");
    } else {
      this.amount = this.currencyPipe.transform(this.amount, "VND", false);
      this.amount = this.amount.replace("VND", "");
      //element.target.value = this.amount;

      console.log("VND");
    }
  }

  filterCardType(filterVal: any) {
    const hosted = (<HTMLInputElement>document.getElementById("hosted")).value;
    this.isDisableHosted = (filterVal === "Wallet" || hosted === "NO");

    switch (filterVal) {
      case "international":
        this.data = this.internationalOption;
        this.bankSelect = "VISA";
        this.isCVV = !(this.isDisableHosted === false);
        this.currencyOption = this.currencyOptionMPGS;
        break;
      case "atm":
        this.data = this.atmOption;
        this.bankSelect = this.paymentProviders.bankDefault.value;
        this.isCVV = true;
        this.currencyOption = this.currencyOptionDomestic;
        break;
      case "Wallet":
        this.data = this.momoOption;
        this.bankSelect = "MOMO";
        this.currencyOption = this.currencyOptionDomestic;
        break;
      case this.paymentGroups.hub.value:
        this.data = this.hupOption;
        this.bankSelect = this.paymentProviders.hub2c2p.value;
        this.currencyOption = this.currencyOption2C2P;
        break;
    }

    this.setValueCardInfo(this.bankSelect);
  }

  filterHosted(filterVal: any) {
    switch (filterVal) {
      case "YES":
        const cardType = (<HTMLInputElement>document.getElementById("cardType")).value;
        this.isDisableHosted = cardType === "Wallet";
        this.isCVV = !(this.isDisableHosted == false && cardType === "international");
        break;
      case "NO":
        this.isDisableHosted = true;
        break;
    }

    this.setValueCardInfo();
  }

  selectBank(filterVal: any) {
    this.setValueCardInfo();
  }

  setValueCardInfo(bankSelect = null) {
    if (this.isCVV === false) {
      const bank = bankSelect || (<HTMLSelectElement>document.getElementById("bank")).value;

      if (bank === "VISA") {
        this.labelCardDate = "Card Expire Date";
        this.cardNumber = "4508750015741019";
        this.cardHolderName = "Nguyen Van A"
        this.cardIssueDate = "01/39";
      } else {
        this.labelCardDate = "Card Expire Date";
        this.cardNumber = "5123450000000008";
        this.cardHolderName = "Nguyen Van A"
        this.cardIssueDate = "01/39";
      }
    } else {
      this.labelCardDate = "Card Issue Date";
      this.cardNumber = "9704000000000018";
      this.cardHolderName = "Nguyen Van A"
      this.cardIssueDate = "03/07";
    }
  }

  public inProcess() {

    this.loading = true;
    this.isDisabledButton = true;

    this.billNumber = (<HTMLInputElement>document.getElementById("billNumber")).value;
    //this.firstName = (<HTMLInputElement>document.getElementById("firstName")).value;
    //this.lastName = (<HTMLInputElement>document.getElementById("lastName")).value;
    this.lang = (<HTMLInputElement>document.getElementById("language")).value;
    //this.city = (<HTMLInputElement>document.getElementById("city")).value;
    //this.email = (<HTMLInputElement>document.getElementById("email")).value;
    this.amount = (<HTMLInputElement>document.getElementById("orderAmount")).value;
    this.currency = (<HTMLSelectElement>document.getElementById("orderCurrency")).value;
    this.description = (<HTMLInputElement>document.getElementById("orderDescription")).value;
    this.cardtype = (<HTMLSelectElement>document.getElementById("cardType")).value;
    this.bank = (<HTMLSelectElement>document.getElementById("bank")).value;
    //this.request = (<HTMLSelectElement>document.getElementById("request")).value;
    this.json = (<HTMLSelectElement>document.getElementById("extraData")).value;

    this.cardNumber = (<HTMLSelectElement>document.getElementById("cardNumber")).value;
    this.cardHolderName = (<HTMLSelectElement>document.getElementById("cardHolderName")).value;
    this.cardIssueDate = (<HTMLSelectElement>document.getElementById("cardIssueDate")).value;
    this.cardVerificationValue = (<HTMLSelectElement>document.getElementById("cardVerificationValue")).value;
    this.IsHostedMerchant = (<HTMLSelectElement>document.getElementById("hosted")).value;

    var data = {
      billNumber: this.billNumber,
      //firstName: this.firstName,
      //lastName: this.lastName,
      language: this.lang,
      //city: this.city,
      //email: this.email,
      orderAmount: this.amount,
      orderCurrency: this.currency,
      //address: this.address,
      orderDescription: this.description,
      cardType: this.cardtype,
      bank: this.bank,
      otp: this.otp,
      request: "purchase",
      extraData: this.json,
      cardNumber: this.cardNumber,
      cardHolderName: this.cardHolderName,
      cardIssueDate: this.cardIssueDate,
      cardExpireDate: this.cardIssueDate,
      cardVerificationValue: this.cardVerificationValue,
      isHostedMerchant: this.IsHostedMerchant === "YES"
    };

    this.http.post<ResponseData>(this.baseUrl + 'api/client', data)
      .subscribe(
        result => {
          this.resultData = result;

          if (result.responseCode != "200") {
            alert(result.responseMessage);
          }
          // success
          else if (result.responseCode == "200" && result.responseData.endpoint != null) {
            window.location.href = result.responseData.endpoint;
          }

          this.loading = false;
          this.isDisabledButton = false;
        },
        result => {
          console.error(result);
          this.loading = false;
          this.isDisabledButton = false;

          const messages = [];
          for (const property in result.error.errors) {
            messages.push(result.error.errors[property])
          }

          alert(result.statusText + ": " + messages);
        });
  }

  public inCancel() {
    this.route.navigate(['/cancel']);
  }
}

interface ResponseData {
  responseCode: string;
  responseData: OrderData;
  responseMessage: string;
}

interface OrderData {
  transactionId: string;
  endpoint: string;
}
