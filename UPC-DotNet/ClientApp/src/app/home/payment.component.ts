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
      email: "Paisley@gmail.com",
      phoneNumber: "0580821083",
      phoneType: "CjcFqIPAtc",
      gender: "F",
      dateOfBirth: "19920117",
      title: "Mr"
    },
    device: {
      browser: "uL3ydX2Pcv",
      fingerprint: "ZdiijSPr0M",
      hostName: "JBddmayji5",
      ipAddress: "KU9CoAMTub",
      deviceID: "woB325my3h",
      deviceModel: "nPEDP9SyHc"
    },
    application: {
      applicationID: "V2hLZeYRHs",
      applicationChannel: "Mobile"
    },
    airline: {
      recordLocator: "VDknTdszRc",
      journeyType: 279182634,
      departureAirport: "Dm5W8daux6",
      departureDateTime: "26/04/202206:31:22",
      arrivalAirport: "DTMKu99Ucx",
      arrivalDateTime: "26/04/202215:18:30",
      services: [{
        serviceCode: "iOrEyae8km",
        quantity: 687449710,
        amount: 80000,
        tax: 0,
        fee: 10000,
        totalAmount: 80000,
        currency: "USD"
      }, {
        serviceCode: "YltyBWqm00",
        quantity: 391314729,
        amount: 60000,
        tax: 0,
        fee: 10000,
        totalAmount: 100000,
        currency: "USD"
      }
      ],
      flights: [{
        airlineCode: "qHRJ0vSJbk",
        carrierCode: "lVPkqwaoDr",
        flightNumber: 304498347,
        travelClass: "OET2hayLmS",
        departureAirport: "J5OF0jDZ0A",
        departureDate: "BBg2Vv5RrS",
        departureTime: "26/04/202213:48:33",
        departureTax: "n2ILRrqiS8",
        arrivalAirport: "u3laQZXoff",
        arrivalDate: "VR0hUprpMp",
        arrivalTime: "26/04/202203:33:43",
        fees: 10000,
        taxes: 0,
        fares: 50000,
        fareBasisCode: "DwzXajRwiv",
        originCountry: "A4uyesF2er"
      }
      ],
      passengers: [{
        passengerID: "uew9dL5JAI",
        passengerType: "SouBmUpryn",
        firstName: "Muhammad",
        lastName: "Kinsley",
        title: "Mrs",
        gender: "F",
        dateOfBirth: "20220425",
        identityNumber: "2KoxDO9XYv",
        nameInPNR: "jGFPV12jcA",
        memberTicket: "fwmplDrraT"
      }
      ]
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
    {
      value: "970400",
      text: "NAPAS (VIETNAM LOCAL BANKS)",
    }
  ];

  // Bank Momo
  public momoOption = [
    {
      value: "MOMO",
      text: "MOMO WALLET",
    }
  ]

  // Service Provider
  public cardType = [
    {
      value: "Wallet",
      text: "WALLET",
    },
    {
      value: "international",
      text: "INTERNATIONAL CARD (VISA, MASTER CARD, JCB,...)",
    },
    {
      value: "atm",
      text: "ATM CARD",
    },
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

  hostSelect = "NO";
  bankSelect = "970400";
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
    const hosted = (<HTMLInputElement>document.getElementById("hosted")).value;
    this.isDisableHosted = (filterVal === "Wallet" || hosted === "NO");

    switch (filterVal) {
      case "international":
        this.data = this.internationalOption;
        this.bankSelect = "VISA";
        this.isCVV = !(this.isDisableHosted === false);
        break;
      case "atm":
        this.data = this.atmOption;
        this.bankSelect = "970400";
        this.isCVV = true;
        break;
      case "Wallet":
        this.data = this.momoOption;
        this.bankSelect = "MOMO";
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
