namespace Demo.Model
{
    public class ModelData
    {
    }

    public class RequestData
    {
        public string billNumber { get; set; }
        public string firstName { get; set; }
        public string lastname { get; set; }
        public string language { get; set; }
        public string city { get; set; }
        public string email { get; set; }
        public string orderAmount { get; set; }
        public string orderCurrency { get; set; }
        public string address { get; set; }
        public string orderDescription { get; set; }
        public string cardType { get; set; }
        public string bank { get; set; }
        public string otp { get; set; }
        public string request { get; set; }
        public string extraData { get; set; }
    }

    public class PaymentData
    {
        public string billNumber { get; set; }
        public string orderAmount { get; set; }
        public string orderCurrency { get; set; }
        public string orderDescription { get; set; }
        public string cardType { get; set; }
        public string bank { get; set; }
        public string otp { get; set; }
        public string request { get; set; }
        public string token { get; set; }
        public string language { get; set; }
        public string client_ip_addr { get; set; }
        public long order_timestamp { get; set; }
        public UPHParameters uphParameters { get; set; } = new UPHParameters();
        //public long hoa { get; set; }
        public object extraData { get; set; } //= new FlightInfo();
        public string signature { get; set; }
    }

    public class UPHParameters
    {
        public string ip { get; set; }
        public string agent { get; set; }
        public string device { get; set; }
    }

    public class ResponseData
    {
        public string responseCode { get; set; }
        public string orderId { get; set; }
        public string endpoint { get; set; }
        public string responseMessage { get; set; }
        public string signature { get; set; }
    }

    public class MomoRequestData
    {
        public string BillNumber { get; set; }
        public decimal Amount { get; set; }
        public string Lang { get; set; }
        public string ExtraData { get; set; }
        public string ResultUrl { get; set; }
        public string CancelUrl { get; set; }
        public string IpnUrl { get; set; }
    }
}
