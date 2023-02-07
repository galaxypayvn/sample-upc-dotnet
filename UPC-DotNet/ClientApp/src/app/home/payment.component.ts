import {HttpClient} from '@angular/common/http';
import {Router} from '@angular/router';
import {Component, Inject} from '@angular/core';
import {CurrencyPipe} from '@angular/common';
import {UPC} from './variables';

@Component({
  selector: 'app-payment-component',
  templateUrl: './payment.component.html'
})
export class PaymentComponent {
  public UPC: UPC;
  public defaultPaymentMethod: string;
  public paymentMethod: string;
  public paymentSource: string;
  public paymentSourceOptions: any;
  public isMasterMerchant = false;
  public merchants = null;

  public lang = "vi";
  public orderNumber = Math.floor((Math.random() * 100000)).toString();
  public orderAmount = "10,000";
  public orderCurrency = "VND";
  public orderDescription = "Secure Page Demo";
  public extra: string;
  public currencyOption: any;
  public token: string;
  public cardNumber = "9704000000000018";
  public cardHolderName = "Nguyen Van A";
  public cardExpireDate = "03/07";
  public cardVerificationValue = "100";
  public labelCardDate = "Card Issue Date";
  public integrationMethod = "SIMPLE";
  public apiOperationMethod = "PAY";
  public sourceOfFund = "CARD" ;
  public merchantID = "";
  public successURL: string;
  public cancelURL: string;
  public ipnURL: string;

  public resultData: ResponseData;
  public isPayWithOption = false;
  public isDisableHosted = true;
  public isDisableToken = true;
  public isCardInfo = false;
  public isCVV = true;
  public loading: boolean = false;
  public isDisabledButton: boolean = false;

  constructor(public http: HttpClient,
              @Inject('BASE_URL')
              public baseUrl: string,
              private route: Router,
              private currencyPipe: CurrencyPipe)
  {
    this.UPC = new UPC();
    this.defaultPaymentMethod = this.UPC.paymentMethods.Domestic.value;

    this.paymentMethod = this.defaultPaymentMethod;
    this.paymentSourceOptions = this.UPC.paymentProviders[this.paymentMethod];
    this.paymentSource = this.UPC.paymentProviders[this.paymentMethod][0].value;
    this.extra = JSON.stringify(this.UPC.paymentExtra, null, 4);
    this.successURL = baseUrl + "api/result";
    this.cancelURL = baseUrl + "api/cancel";
    this.ipnURL = baseUrl + "api/ipn";
    this.currencyOption = this.UPC.currencyDomestic;

    this.getMerchants();
  }

  sortOptions = (a, b): number => { return a.value.order > b.value.order ? 1 : 0; }

  transformAmount() {
    let parts = this.orderAmount.split(".");
    let even = parts[0].match(/\d/g);
    let odd = parts[1] || "00";
    this.orderAmount = even.join("");

    if (parts.length == 1) {
      this.orderAmount = this.currencyPipe.transform(this.orderAmount, "VND", false);
      this.orderAmount = this.orderAmount.replace("VND", "");
    }
    else{
      this.orderAmount = this.currencyPipe.transform(this.orderAmount, "USD", false);
      this.orderAmount = this.orderAmount
        .replace("USD", "")
        .replace(".00", "." + odd);
    }
  }

  filterProvider(element: any) {
    const method = (<HTMLInputElement>document.getElementById("integrationMethods")).value;
    const Methods = this.UPC.integrationMethods;
    const provider = element.value;
    const Providers = this.UPC.paymentMethods;

    this.isDisableHosted = (
      provider === Providers.Wallet.value ||
      provider === Providers.Hub.value ||
      provider === Providers.QRPay.value ||
      method !== Methods.Hosted.value
    );

    switch (provider) {
      case Providers.Domestic.value:
        this.paymentSourceOptions = this.UPC.paymentProviders[provider];
        this.paymentSource = this.paymentSourceOptions[0].value;
        this.isCVV = true;
        this.currencyOption = this.UPC.currencyDomestic;
        break;

      case Providers.International.value:
        this.paymentSourceOptions = this.UPC.paymentProviders[provider];
        this.paymentSource = this.paymentSourceOptions[0].value;
        this.isCVV = !(this.isDisableHosted === false);
        this.currencyOption = this.UPC.currencyInternational;
        break;

      case Providers.Wallet.value:
        this.paymentSourceOptions = this.UPC.paymentProviders[provider];
        this.paymentSource = this.paymentSourceOptions[0].value;
        this.currencyOption = this.UPC.currencyDomestic;
        break;

      case Providers.Hub.value:
        this.paymentSourceOptions = this.UPC.paymentProviders[provider];
        this.paymentSource = this.paymentSourceOptions[0].value;
        this.currencyOption = this.paymentSource == this.UPC.Providers.Hub2C2P
          ? this.UPC.currency2C2P
          : this.UPC.currencyPOLI;
        break;

      case Providers.QRPay.value:
        this.paymentSourceOptions = this.UPC.paymentProviders[provider];
        this.paymentSource = this.paymentSourceOptions[0].value;
        this.currencyOption =  this.UPC.currencyDomestic;
        break;
    }

    // Token with GPAY
    if(provider === Providers.Wallet.value &&
      this.paymentSource === this.UPC.Providers.GPAY &&
      this.integrationMethod === this.UPC.integrationMethods.Hosted.value) {
      this.sourceOfFund = this.UPC.sourceOfFund.token.value;
      this.isDisableHosted = false;
      this.isDisableToken = false;
      this.isCardInfo = true;
    }

    this.orderCurrency = this.currencyOption[0].value;
    this.setCardInfo();
  }

  filterMethod(element: any) {
    let method = element.value;
    let Methods = this.UPC.integrationMethods;
    let Providers = this.UPC.paymentMethods;

    switch (method) {
      // HOSTED
      case Methods.Hosted.value:
        const paymentMethod = (<HTMLInputElement>document.getElementById("paymentMethods")).value;
        this.isDisableHosted = (paymentMethod === Providers.Wallet.value || paymentMethod === Providers.Hub.value);
        this.isCVV = !(this.isDisableHosted == false && paymentMethod === Providers.International.value);
        this.isPayWithOption = false;

        // pay with token GPay
        if(this.paymentMethod === Providers.Wallet.value &&
          this.paymentSource === this.UPC.Providers.GPAY) {
          this.sourceOfFund = this.UPC.sourceOfFund.token.value;
          this.isDisableHosted = false;
          this.isDisableToken = false;
          this.isCardInfo = true;
        }
        break;

      // OPTION
      case Methods.Option.value:
        this.isDisableHosted = true;
        this.isPayWithOption = true;
        this.currencyOption = this.UPC.currencyInternational;
        break;

      default:
        this.isDisableHosted = true;
        this.isPayWithOption = false;
        break;
    }

    this.setCardInfo();
  }

  filterPaymentSource() {
    switch (this.paymentSource)
    {
      case this.UPC.Providers.Hub2C2P:
        this.currencyOption = this.UPC.currency2C2P;
        break;

      case this.UPC.Providers.HubPOLI:
        this.currencyOption = this.UPC.currencyPOLI;
        this.orderCurrency = this.UPC.currencyPOLI[0].value;
        break;

      case this.UPC.Providers.GPAY:
        this.isDisableHosted = false;
        this.isDisableToken = false;
        this.isCardInfo = true;
        break;
    }

    // Wallet and (not GPay)
    if(this.paymentMethod === this.UPC.paymentMethods.Wallet.value &&
      this.paymentSource !== this.UPC.Providers.GPAY) {
      this.isDisableHosted = true;
    }

    this.setCardInfo();
  }

  filterSourceOfFund(element: any) {
    switch (this.sourceOfFund)
    {
      case this.UPC.sourceOfFund.card.value:
        this.isDisableToken = true;
        this.isCardInfo = false;

        // Token with GPAY
        if(this.paymentMethod === this.UPC.paymentMethods.Wallet.value &&
          this.paymentSource === this.UPC.Providers.GPAY &&
          this.integrationMethod === this.UPC.integrationMethods.Hosted.value) {
          this.isCardInfo = true;
        }
        break;

      case this.UPC.sourceOfFund.token.value:
        this.isDisableToken = false;
        this.isCardInfo = true;
        break;
    }

    this.setCardInfo();
  }

  setCardInfo() {
    if (this.isCVV === false)
    {
      const source = this.paymentSource;
      if (source === "" || source === "VISA")
      {
        this.labelCardDate = "Card Expire Date";
        this.cardNumber = "4456530000001096";
        this.cardHolderName = "Nguyen Van A"
        this.cardExpireDate = "12/30";
        this.cardVerificationValue = "111";
      }
      else
      {
        this.labelCardDate = "Card Expire Date";
        this.cardNumber = "5506922400634930";
        this.cardHolderName = "Nguyen Van A"
        this.cardExpireDate = "01/39";
        this.cardVerificationValue = "100";
      }
    }
    else
    {
      // NAPAS
      this.labelCardDate = "Card Issue Date/Expire Date";
      this.cardNumber = "9704000000000018";
      this.cardHolderName = "Nguyen Van A"
      this.cardExpireDate = "03/07";
    }
  }

  public inProcess() {
    this.loading = true;
    this.isDisabledButton = true;
    let sourceOfFund = this.paymentSource;

    let elementMerchant = (<HTMLSelectElement>document.getElementById("merchant"));
    if (elementMerchant != null)
    {
      this.merchantID = elementMerchant.value;
    }

    let requestData = {
      language: this.lang,
      billNumber: this.orderNumber,
      orderAmount: this.orderAmount,
      orderCurrency: this.orderCurrency,
      orderDescription: this.orderDescription,
      paymentMethod: this.paymentMethod,
      sourceType: this.paymentSource,
      extraData: this.extra,
      cardNumber: this.cardNumber,
      cardHolderName: this.cardHolderName,
      cardExpireDate: this.cardExpireDate,
      cardVerificationValue: this.cardVerificationValue,
      integrationMethod: this.integrationMethod,
      apiOperation: this.apiOperationMethod,
      merchantID: this.merchantID,
      successURL: this.successURL,
      cancelURL: this.cancelURL,
      ipnURL: this.ipnURL,
      baseUrl: this.baseUrl,
      token: this.token,
      sourceOfFund: this.sourceOfFund
    };

    this.http.post<ResponseData>(this.baseUrl + 'api/client', requestData)
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

  public getMerchants(){
    if(this.merchants != null)
    {
      return;
    }

    this.http.get<List<MerchantData>>(this.baseUrl + 'api/merchant')
      .subscribe(
        result => {
          this.merchants = result;
          this.merchants = this.merchants.sort((a, b) => (a.order < b.order) ? -1 : 1);

          if(this.merchants.length > 0){
            this.isMasterMerchant = true;
          }
        },
        result => {
          console.error(result);
          const messages = [];
          for (const property in result.error.errors) {
            messages.push(result.error.errors[property])
          }

          console.log(result.statusText + ": " + messages);
        });
  }
}

interface List<T> {
}

interface MerchantData {
  value: string;
  text: string;
  order: number;
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
