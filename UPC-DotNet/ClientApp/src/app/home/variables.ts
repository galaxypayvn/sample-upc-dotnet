export class UPC {
  constructor() {};

  public Providers = {
    Hub2C2P: "2C2P",
    HubPOLI: "POLI",
    MOMO: "MOMO",
    ZALOPAY: "ZALOPAY",
    GPAY: "GPAY",
    QRPAY: "QRPAY"
  }

  public apiOperationMethod = {
    pay: { value: "PAY", text: "Pay", isDefault: true, order: 1 },
    payWithCreateToken: { value: "PAY_WITH_CREATE_TOKEN", text: "Pay with create token", isDefault: false, order: 2 },
  }

  public sourceOfFund = {
    card: { value: "CARD", text: "Card", isDefault: true, order: 1 },
    token: { value: "TOKEN", text: "Token", isDefault: false, order: 2 },
  }

  // Integration Methods
  public integrationMethods = {
    Simple: { value: "SIMPLE", text: "Merchant Checkout", isDefault: true, order: 1 },
    Hosted: { value: "HOSTED", text: "Merchant Hosted Checkout", isDefault: false, order: 2 },
    Option: { value: "OPTION", text: "Pay with Options", isDefault: false, order: 3 },
  }

  public paymentMethods = {
    Domestic:       { value: "DOMESTIC", text: "ATM CARD (VIETNAM)", isDefault: true, order: 1 },
    International:  { value: "INTERNATIONAL", text: "INTERNATIONAL CARD (VISA, MASTER CARD, JCB, AMEX)", isDefault: false, order: 2 },
    Wallet:         { value: "WALLET", text: "eWALLET", isDefault: false, order: 3 },
    Hub:            { value: "HUB", text: "PAYMENT HUBS", isDefault: false, order: 4 },
    QRPay:          { value: "QRPAY", text: "QR PAYMENT", isDefault: false, order: 5 },
    BNPL:           { value: "BNPL", text: "BUY NOW - PAY LATER", isDefault: false, order: 6 },
  }

  public paymentProviders = {
    DOMESTIC: [
      { value: "", text: "VIETNAM LOCAL BANKS", isDefault: true, order: 1 },
      { value: "SAIGONBANK", text: "SAIGON BANK/NGÂN HÀNG TMCP SÀI GÒN CÔNG THƯƠNG", isDefault: false, order: 2 },
    ],
    INTERNATIONAL: [
      { value: "", text: "NONE SPECIFIED", isDefault: true, order: 1 },
      { value: "VISA", text: "VISA CARD", isDefault: false, order: 2 },
      { value: "MASTER", text: "MASTER CARD", isDefault: false, order: 3 },
      { value: "JCB", text: "JCB CARD", isDefault: false, order: 4 },
      { value: "AMEX", text: "AMERICAN EXPRESS CARD", isDefault: false, order: 5 }
    ],
    WALLET: [
      { value: "GPAY", text: "GPAY eWALLET", isDefault: true, order: 1 },
      { value: "MOMO", text: "MOMO eWALLET", isDefault: false, order: 2 },
      { value: "ZALOPAY", text: "ZALOPAY eWALLET", isDefault: false, order: 3 },
      { value: "VIETEL", text: "VIETEL MONEY eWALLET", isDefault: false, order: 4 },
    ],
    HUB: [
      { value: "2C2P", text: "2C2P HUB", isDefault: true, order: 1 },
      { value: "POLI", text: "POLI HUB", isDefault: false, order: 2 },
      { value: "PAYTM", text: "PAYTM HUB", isDefault: false, order: 3 },
    ],
    QRPAY: [
      { value: "QRPAY", text: "VIET QR", isDefault: true, order: 1 },
      { value: "VNPAY", text: "VNPAY QR", isDefault: false, order: 2 },
    ],
    BNPL: [
      { value: "KREDIVO", text: "KREDIVO", isDefault: true, order: 1 }
    ]
  }

  // currencyOption ATM and MOMO
  public currencyDomestic = [
    {
      value: "VND",
      text: "VND",
    }
  ]

  // currencyOption MPGS/CYBS
  public currencyInternational = [
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
    },
    {
      value: "AUD",
      text: "AUD",
    }
  ]

  // currencyOption 2C2P
  public currency2C2P = [
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
    },
    {
      value: "AUD",
      text: "AUD",
    }
  ]

  public currencyPOLI = [
    {
      value: "AUD",
      text: "AUD",
    }
  ]

  public paymentExtra = {
    customer: {
      firstName: "Jacob",
      lastName: "Savannah",
      identityNumber: "6313126925",
      email: "Jacob@gmail.com",
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
      country: "vn",
      state: "",
      city: "Hồ Chí Minh",
      postalCode: "70000",
      streetNumber: "673",
      address01: "Đường Nguyễn Hữu Thọ",
      address02: ""
    },
    shipping: {
      country: "vn",
      state: "",
      city: "Hồ Chí Minh",
      postalCode: "",
      streetNumber: "673",
      address01: "Đường Nguyễn Hữu Thọ",
      address02: ""
    }
  }
}
